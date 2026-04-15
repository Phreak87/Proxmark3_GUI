#include <Arduino.h>
#include "HX711.h"
#include <WiFi.h>
#include <ESPAsyncWebServer.h>
#include <AsyncTCP.h>
#include <ArduinoJson.h>
#include <ArduinoOTA.h>
#include <LittleFS.h>
#include "Index.html.h"

const int RelaisPins[8] = {32, 33, 25, 26, 27, 14, 12, 13};

#define HX_DT  4 // HX711 data pin
#define HX_SCK 16
#define AP_SSID " Cocktail"
#define AP_PASS "12345678"

AsyncWebServer server(80);
AsyncWebSocket ws("/ws");
HX711 scale;

const char* WIFI_FILE = "/wifi.json";

String sta_ssid = "";
String sta_pass = "";

void setupOTA() {
  ArduinoOTA.setHostname("cocktail-esp32");

  ArduinoOTA.onStart([]() {
    Serial.println("OTA Start");
  });

  ArduinoOTA.onEnd([]() {
    Serial.println("\nOTA End");
  });

  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress * 100) / total);
  });

  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
    else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
    else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
    else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
    else if (error == OTA_END_ERROR) Serial.println("End Failed");
  });

  ArduinoOTA.begin();
}

void dispenseMS(int relais, int ms) {
    digitalWrite(RelaisPins[relais], HIGH);
    delay(ms);
    digitalWrite(RelaisPins[relais], LOW);
}
void dispenseML(int relais, int ms) {
    digitalWrite(RelaisPins[relais], HIGH);
    delay(ms);
    digitalWrite(RelaisPins[relais], LOW);
}

void TestAllPins() {
  for (int i = 0; i < 8; i++) {
    dispenseMS (i, 200);
  }
}

bool connectToWiFi() {
  if (sta_ssid.isEmpty()) {
      Serial.println("No WiFi credentials stored.");
      return false;
  }

  WiFi.mode(WIFI_STA);
  WiFi.begin(sta_ssid.c_str(), sta_pass.c_str());

  Serial.print("Connecting to WiFi: ");
  Serial.println(sta_ssid);

  unsigned long start = millis();

  while (WiFi.status() != WL_CONNECTED &&
         millis() - start < 10000) {
      Serial.print(".");
      delay(500);
  }

  if (WiFi.status() == WL_CONNECTED) {
      Serial.println("\nWiFi connected!");
      Serial.print("SSID: ");
      Serial.println(WiFi.SSID());
      Serial.print("IP address: ");
      Serial.println(WiFi.localIP());
      Serial.print("RSSI: ");
      Serial.print(WiFi.RSSI());
      Serial.println(" dBm");
      return true;
  } else {
      Serial.println("\nWiFi connection failed!");
      return false;
  }
}
void startAP() {
  WiFi.mode(WIFI_AP);
  WiFi.softAP(AP_SSID, AP_PASS);
}
bool loadWiFiConfig() {
  if (!LittleFS.exists(WIFI_FILE)) return false;

  File file = LittleFS.open(WIFI_FILE, "r");
  if (!file) return false;

  JsonDocument doc;
  if (deserializeJson(doc, file)) {
    file.close();
    return false;
  }

  sta_ssid = doc["ssid"].as<String>();
  sta_pass = doc["pass"].as<String>();
  file.close();
  return true;
}
void saveWiFiConfig(String ssid, String pass) {
  JsonDocument  doc;
  doc["ssid"] = ssid;
  doc["pass"] = pass;

  File file = LittleFS.open(WIFI_FILE, "w");
  serializeJson(doc, file);
  file.close();
}
String getWiFiStatusJson() {
  JsonDocument doc;

  doc["mode"] = WiFi.getMode() == WIFI_STA ? "STA" : "AP";
  doc["connected"] = WiFi.status() == WL_CONNECTED;

  if (WiFi.status() == WL_CONNECTED) {
    doc["ip"] = WiFi.localIP().toString();
    doc["ssid"] = WiFi.SSID();
    doc["rssi"] = WiFi.RSSI();
  }

  String json;
  serializeJson(doc, json);
  return json;
}
String scanNetworksJson() {
  int n = WiFi.scanNetworks();
  JsonDocument doc;
  JsonArray arr = doc.createNestedArray("networks");

  for (int i = 0; i < n; i++) {
    JsonObject net = arr.createNestedObject();
    net["ssid"] = WiFi.SSID(i);
    net["rssi"] = WiFi.RSSI(i);
    net["secure"] = WiFi.encryptionType(i) != WIFI_AUTH_OPEN;
  }

  String json;
  serializeJson(doc, json);
  WiFi.scanDelete();
  return json;
}

String getScaleJson() {
  JsonDocument doc;
  doc["weight"] = scale.get_units(10);
  String json;
  serializeJson(doc, json);
  return json;
}
void sendWeight() {
  static unsigned long lastSend = 0;
  if (millis() - lastSend > 1000) {
    ws.textAll(getScaleJson());
    lastSend = millis();
  }
}

void onWebSocketEvent(AsyncWebSocket *server, AsyncWebSocketClient *client, AwsEventType type, void *arg, uint8_t *data, size_t len){
  if (type == WS_EVT_CONNECT) {
    client->text(getWiFiStatusJson());
    client->text(getScaleJson()); 
  }

  if (type == WS_EVT_DATA) {
    String msg;
    for (size_t i = 0; i < len; i++)
      msg += (char)data[i];

    if (msg == "status") {
      client->text(getWiFiStatusJson());
    }
  }
}

void setup() {
  Serial.begin(115200);
  scale.begin(HX_DT, HX_SCK);
  scale.set_scale();
  scale.tare();

  for (int i = 0; i < 8; i++) {
    pinMode(RelaisPins[i], OUTPUT);
    digitalWrite(RelaisPins[i], LOW);
  }

  TestAllPins();

  if (!LittleFS.begin(true)) {
    Serial.println("LittleFS mount failed");
    return;
  }

  loadWiFiConfig();

  if (!connectToWiFi()) {
    startAP();    // ✅ Start in AP mode if WiFi connection fails (OTA won't work in this mode, but user can connect to AP and set up WiFi)
  } else {
    setupOTA();   // ✅ ONLY when connected to WiFi
  }

  ws.onEvent(onWebSocketEvent);
  server.addHandler(&ws);

  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send_P(200, "text/html", index_html);
  });

  server.on("/status", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send(200, "application/json", getWiFiStatusJson());
  });

  server.on("/scan", HTTP_GET, [](AsyncWebServerRequest *request) {
    request->send(200, "application/json", scanNetworksJson());
  });

  server.on("/setup", HTTP_POST, 
    [](AsyncWebServerRequest *request) {}, NULL, [](AsyncWebServerRequest *request, uint8_t *data, size_t len, size_t index, size_t total){
      JsonDocument doc;
      deserializeJson(doc, data);
      String ssid = doc["ssid"];
      String pass = doc["pass"];
      saveWiFiConfig(ssid, pass);
      request->send(200, "application/json",
                    "{\"status\":\"saved\"}");
      delay(1000);
      ESP.restart(); });

  server.on("/dispense", HTTP_POST,
  [](AsyncWebServerRequest *request){}, NULL,[](AsyncWebServerRequest *request,uint8_t *data, size_t len, size_t index, size_t total) {
      JsonDocument doc; deserializeJson(doc, data);
      int port = doc["port"];
      int amount = doc["amount"];
      dispenseMS (port, amount);
      request->send(200, "application/json", "{\"status\":\"dispensed\"}");
    });

  server.begin();
}

void loop() {
  ws.cleanupClients();
  ArduinoOTA.handle();
  sendWeight();
}
