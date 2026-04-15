const char index_html[] PROGMEM = R"rawliteral(
<!DOCTYPE html>
<html lang="de">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Cocktail Selector ESP</title>
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<style>
body { font-family: Arial, sans-serif; margin:0; padding:0; background:#f0f0f0; }
header { background:#333; color:#fff; padding:1rem; display:flex; justify-content:space-between; align-items:center; }
header h1 { margin:0; font-size:1.2rem; }
header #weight-display { font-size:1rem; font-weight:bold; margin-left:1rem; }
header button { padding:0.4rem 0.8rem; border:none; border-radius:5px; background:#ff6600; color:#fff; cursor:pointer; transition:0.2s; }
header button:hover { background:#e65500; }

.filter-buttons { text-align:center; margin:1rem 0; }
.filter-buttons button { padding:0.5rem 1rem; margin:0 0.3rem; cursor:pointer; border:none; border-radius:5px; background:#555; color:#fff; transition:0.2s; font-size:0.9rem; }
.filter-buttons button:hover, .filter-buttons button.active { background:#ff6600; }

.container { display:grid; gap:1rem; padding:1rem; justify-content:center; max-height: calc(100vh - 120px); overflow-y:auto; }
.cocktail-card { background:#fff; border-radius:10px; box-shadow:0 2px 6px rgba(0,0,0,0.2); cursor:pointer; text-align:center; transition: transform 0.2s, box-shadow 0.2s; }
.cocktail-card:hover { transform:translateY(-5px); box-shadow:0 6px 12px rgba(0,0,0,0.3); }
.cocktail-card img { width:100%; height:150px; object-fit:cover; border-radius:10px 10px 0 0; background:#ccc; display:block; }
.cocktail-card h3 { margin:0.5rem 0; font-size:1rem; padding:0 0.3rem; }

/* Modal */
.modal { display:none; position:fixed; z-index:1000; left:0; top:0; width:100%; height:100%; overflow:auto; background-color:rgba(0,0,0,0.6); }
.modal-content { background:#fff; margin:3% auto; padding:1rem; border-radius:10px; width:95%; max-width:600px; text-align:left; position:relative; }
.modal-content img { width:100%; max-height:250px; object-fit:cover; border-radius:10px; margin-bottom:1rem; }
.modal-content button { padding:0.5rem 1rem; margin:0.5rem 0; cursor:pointer; border:none; border-radius:5px; background:#ff6600; color:#fff; transition:0.2s; width:100%; font-size:1rem; }
.modal-content button:hover { background:#e65500; }
.close { position:absolute; top:10px; right:15px; font-size:24px; font-weight:bold; cursor:pointer; color:#333; }
.ingredients, .steps { margin-bottom:1rem; padding-left:1rem; }
.ingredients li, .steps li { margin-bottom:0.3rem; }
.status-container { width:100%; background:#ddd; border-radius:10px; overflow:hidden; margin:0.5rem 0; height:20px; }
.status-bar { width:0%; height:100%; background:#ff6600; transition:width 0.3s; }
.current-step { margin-bottom:0.5rem; font-weight:bold; color:#333; }

/* Settings Modal Styles */
#settings-modal .modal-content { max-width:700px; }

#wifi-networks li {
  background:#eee;
  margin:5px 0;
  padding:6px;
  border-radius:5px;
  cursor:pointer;
}

#wifi-networks li:hover {
  background:#ddd;
}

/* Responsive Grid */
@media (min-width:1200px) { .container { grid-template-columns: repeat(4,1fr); } }
@media (min-width:700px) and (max-width:1199px) { .container { grid-template-columns: repeat(3,1fr); } }
@media (min-width:500px) and (max-width:699px) { .container { grid-template-columns: repeat(2,1fr); } }
@media (max-width:499px) { .container { grid-template-columns: 1fr; } }
</style>
</head>
<body>

<header>
  <h1 id="weight-display">Weight: -- kg</h1>
  <div>
    <button onclick="openFunctions()">Funktionen</button>
    <button onclick="openSettings()">Einstellungen</button>
    <button onclick="openWiFiModal()">WiFi</button>
  </div>
</header>

<div class="filter-buttons">
  <button id="filter-alc" class="active" onclick="filterType('Alc')">Alc</button>
  <button id="filter-nonalc" onclick="filterType('Non-Alc')">Non-Alc</button>
</div>

<div class="filter-buttons">
  <button id="filter-250ml"   class="active"  onclick="filterType('Alc')">250ml</button>
  <button id="filter-300ml"                   onclick="filterType('Non-Alc')">300ml</button>
</div>

<div class="container" id="cocktail-container"></div> 

<!-- WiFi Modal -->
<div id="wifi-modal" class="modal">
  <div class="modal-content">
    <span class="close" onclick="closeWiFiModal()">&times;</span>
    
    <h2>WiFi Manager</h2>

    <h3>Status</h3>
    <pre id="wifi-status"></pre>

    <h3>Netzwerke</h3>
    <button onclick="scanWiFi()">Scan</button>
    <ul id="wifi-networks" style="list-style:none;padding:0;"></ul>

    <h3>Verbinden</h3>
    <input id="wifi-ssid" placeholder="SSID"><br><br>
    <input id="wifi-pass" type="password" placeholder="Passwort"><br><br>
    <button onclick="saveWiFi()">Speichern</button>
  </div>
</div>

<!-- Cocktail Modal -->
<div id="cocktail-modal" class="modal">
  <div class="modal-content">
    <span class="close" onclick="closeModal()">&times;</span>
    <h2 id="modal-name"></h2>
    <img id="modal-img" src="" alt="">
    <h3>Zutaten</h3>
    <ul class="ingredients" id="modal-ingredients"></ul>
    <h3>Schritte</h3>
    <ol class="steps" id="modal-steps"></ol>
    <div class="current-step" id="current-step">Schritt: -</div>
    <div class="status-container"><div class="status-bar" id="status-bar"></div></div>
    <button id="make-btn">Start Make</button>
    <button id="next-step-btn" style="display:none;">Weiter</button>
  </div>
</div>



<!-- Functions Modal -->
<div id="functions-modal" class="modal">
  <div class="modal-content">
    <span class="close" onclick="closeSettings()">&times;</span>
    <h2>Funktionen</h2>

    <div class="mb-3">
      <label for="port-select" class="form-label">Port auswählen</label>
      <select class="form-select" id="port-select" onchange="loadPortSettings()">
        <option value="1">All</option>
        <option value="1">Port 1</option>
        <option value="2">Port 2</option>
        <option value="3">Port 3</option>
        <option value="4">Port 4</option>
        <option value="5">Port 5</option>
        <option value="6">Port 6</option>
        <option value="7">Port 7</option>
        <option value="8">Port 8</option>
      </select>
    </div>

    <div id="port-settings">
      <div class="row">
        <div class="col">
          <div class="mb-3">
            <label for="ml-output" class="form-label">ML-Ausgabe</label>
            <input type="number" class="form-control" id="ml-output" value="50">
          </div>
        </div>
        <div class="col d-flex align-items-end">
          <button class="btn btn-primary" onclick="dispense()">Ausgeben</button>
        </div>
      </div>
      <div class="row">
        <div class="col">
          <div class="mb-3">
            <label for="ml-output" class="form-label">MS-Ausgabe</label>
            <input type="number" class="form-control" id="mS-output" value="500">
          </div>
        </div>
        <div class="col d-flex align-items-end">
          <button class="btn btn-primary" onclick="dispense()">Ausgeben</button>
        </div>
      </div>
    </div>
      <div class="row">
        <div class="col d-flex align-items-end">
          <button class="btn btn-primary" onclick="dispense()">Ansaugen</button>
        </div>
      </div>
      <div class="row">
        <div class="col d-flex align-items-end">
          <button class="btn btn-primary" onclick="dispense()">Ausleeren</button>
        </div>
      </div>
    </div>
  </div>
</div>

<!-- Settings Modal -->
<div id="settings-modal" class="modal">
  <div class="modal-content">
    <span class="close" onclick="closeSettings()">&times;</span>
    <h2>Einstellungen</h2>

    <h3>Grundeinstellungen</h3>

    <div class="mb-3">
      <label for="threshold" class="form-label" >Erlaubte Differenz Abgabemenge/Gemessen</label>
      <input type="number" class="form-control" id="MaxDiff" value="3">
    </div>

    <div class="mb-3">
      <label for="threshold-max" class="form-label">Nachpumpdauer (ms)</label>
      <input type="number" class="form-control" id="threshold-max" value="100">
    </div>

    <div class="mb-3">
      <label for="threshold-max" class="form-label">Waage Messinterval (ms)</label>
      <input type="number" class="form-control" id="threshold-max" value="50">
    </div>

    <div class="mb-3">
      <label for="threshold-max" class="form-label">Pumpendruck in g während der Ausgabe:</label>
      <input type="number" class="form-control" id="threshold-max" value="10">
    </div>

    <div class="mb-3">
      <label for="threshold-max" class="form-label">Schwingdauer (ms)</label>
      <input type="number" class="form-control" id="swingtime" value="100">
    </div>

    <h3>Ports konfigurieren</h3>
    <div class="mb-3">
      <label for="port-select" class="form-label">Port auswählen</label>
      <select class="form-select" id="port-select" onchange="loadPortSettings()">
        <option value="1">Port 1</option>
        <option value="2">Port 2</option>
        <option value="3">Port 3</option>
        <option value="4">Port 4</option>
        <option value="5">Port 5</option>
        <option value="6">Port 6</option>
        <option value="7">Port 7</option>
        <option value="8">Port 8</option>
      </select>
    </div>

    <div id="port-settings">
      <div class="row">
        <div class="col">
          <div class="mb-3">
            <label for="port-amount" class="form-label">Menge g/ml</label>
            <input type="number" class="form-control" id="port-amount" value="1">
          </div>
        </div>
        <div class="col">
          <div class="mb-3">
            <label for="port-drink" class="form-label">Getränk</label>
            <select class="form-select" id="port-drink">${generateDrinkOptions(drinksTree)}</select>
          </div>
        </div>
      </div>
    <button class="btn btn-success" onclick="savePortSettings()">Speichern</button>
    <button class="btn btn-secondary" onclick="closeSettings()">Schließen</button>
  </div>
</div>

<script>
var ws = new WebSocket('ws://' + window.location.host + '/ws');
ws.onmessage = function(event) {
  var data = JSON.parse(event.data);
  if (data.weight !== undefined) {
    document.getElementById('weight-display').innerText = 'Weight: ' + data.weight.toFixed(2) + ' kg';
  }
};

// Getränke Baum
const drinksTree = {
  "Vodka":   ["Vodka Standard","Vodka Premium","Vodka Lemon"],
  "Rum":     ["Weißer Rum","Dunkler Rum","Aged Rum"],
  "Liqueur": ["Cointreau","Amaretto","Kahlua"],
  "Juice":   ["Orange","Limette","Ananas","Cranberry", "Kirsche", "Banane", "Birne", "Apfel", "Grapefruit", "Mango", "Maracuja", "Pfirsich", "Traube"],
  "Soda":    ["Mineralwasser","Tonic Water","Ginger Ale"]
};

const cocktailsJson = [];
for(let i=1;i<=20;i++){
  cocktailsJson.push({
    id:i,
    name:`Cocktail ${i}`,
    type: i%2===0?'Alc':'Non-Alc',
    img:`images/mojito.jpg`,
    ingredients:[`Zutat A${i}`,`Zutat B${i}`,`Zutat C${i}`],
    steps:[
      {text:`Manueller Schritt A${i}`, manual:true},
      {text:`Automatischer Schritt B${i}`, manual:false},
      {text:`Automatischer Schritt C${i}`, manual:false}
    ]
  });
}

let currentFilter='Alc';
let manualStep=0;

function showCocktails(){
  const container=document.getElementById('cocktail-container'); container.innerHTML='';
  cocktailsJson.filter(c=>c.type===currentFilter).forEach(c=>{
    const card=document.createElement('div'); card.className='cocktail-card';
    card.innerHTML=`<img src="${c.img}" alt="${c.name}"><h3>${c.name}</h3>`;
    card.onclick=()=>openModal(c);
    container.appendChild(card);
  });
}

function filterType(type){
  currentFilter=type;
  document.getElementById('filter-alc').classList.remove('active');
  document.getElementById('filter-nonalc').classList.remove('active');
  if(type==='Alc') document.getElementById('filter-alc').classList.add('active');
  else document.getElementById('filter-nonalc').classList.add('active');
  showCocktails();
}

function openModal(c){
  document.getElementById('modal-name').textContent=c.name;
  document.getElementById('modal-img').src=c.img;
  const ingList=document.getElementById('modal-ingredients'); ingList.innerHTML='';
  c.ingredients.forEach(i=>{const li=document.createElement('li');li.textContent=i;ingList.appendChild(li);});
  const stepsList=document.getElementById('modal-steps'); stepsList.innerHTML='';
  c.steps.forEach(s=>{const li=document.createElement('li');li.textContent=s.text;stepsList.appendChild(li);});

  document.getElementById('current-step').textContent='Schritt: -';
  document.getElementById('status-bar').style.width='0%';

  const makeBtn=document.getElementById('make-btn'); const nextBtn=document.getElementById('next-step-btn');
  manualStep=0; nextBtn.style.display='none'; makeBtn.style.display='inline-block';
  makeBtn.onclick=()=>startMake(c);

  document.getElementById('cocktail-modal').style.display='block';
}

function closeModal(){document.getElementById('cocktail-modal').style.display='none';}
window.onclick=function(e){if(e.target==document.getElementById('cocktail-modal')) closeModal();}

function startMake(c){manualStep=0; processNextStep(c);}
function processNextStep(c){
  if(manualStep<c.steps.length){
    const step=c.steps[manualStep];
    const currentStepElem=document.getElementById('current-step');
    const statusBar=document.getElementById('status-bar');
    const nextBtn=document.getElementById('next-step-btn');
    if(step.manual){
      nextBtn.style.display='inline-block';
      nextBtn.onclick=function(){
        currentStepElem.textContent="Schritt: "+step.text;
        statusBar.style.width=((manualStep+1)/c.steps.length*100)+'%';
        manualStep++; nextBtn.style.display='none'; processNextStep(c);
      };
    } else {
      currentStepElem.textContent="Schritt: "+step.text;
      statusBar.style.width=((manualStep+1)/c.steps.length*100)+'%';
      manualStep++;
      setTimeout(()=>processNextStep(c),500);
    }
  } else {
    document.getElementById('current-step').textContent="Fertig!";
    document.getElementById('status-bar').style.width='100%';
    fetch(`/make?id=${c.id}`).then(res=>console.log(`Befehl gesendet für ${c.name}`));
  }
}

// Settings
function openSettings(){
  document.getElementById('settings-modal').style.display='block';
  // Load default for port 1
  loadPortSettings();
}
  
// Functions
function openFunctions(){
  document.getElementById('functions-modal').style.display='block';
  loadPortSettings();
}

function closeSettings(){document.getElementById('settings-modal').style.display='none';}

function loadPortSettings(){
  const port = document.getElementById('port-select').value;
  document.getElementById('port-amount').value = '1';
  document.getElementById('port-drink').value = 'Vodka Standard'; 
}

function generateDrinkOptions(tree){
  let html='<option value="">-- Wählen --</option>';
  for(const cat in tree){
    html+=`<optgroup label="${cat}">`;
    tree[cat].forEach(d=>{html+=`<option value="${d}">${d}</option>`;});
    html+='</optgroup>';
  }
  return html;
}

function dispense(){
  const port = document.getElementById('port-select').value;
  const amount = document.getElementById('ml-output').value;
  fetch('/dispense', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      port: parseInt(port),
      amount: parseInt(amount)
    })
  })
  .then(response => response.json())
  .then(data => {
    console.log('Dispensed:', data);
  })
  .catch(error => {
    console.error('Error:', error);
    alert('Fehler bei der Ausgabe!: ' + error.message);
  });
}

function savePortSettings(){
  const port = document.getElementById('port-select').value;
  const drink = document.getElementById('port-drink').value;
  const amount = parseFloat(document.getElementById('port-amount').value);
  const thresholdMin = parseFloat(document.getElementById('threshold-min').value);
  const thresholdMax = parseFloat(document.getElementById('threshold-max').value);
  // Save logic here, e.g., send to server or store locally
  console.log('Port:', port, 'Drink:', drink, 'Amount:', amount, 'Threshold:', thresholdMin, thresholdMax);
  alert('Einstellungen für Port ' + port + ' gespeichert!');
  // Optionally, close or stay open
}

// ================= WIFI MODAL =================

let wifiWS;

function openWiFiModal(){
  document.getElementById("wifi-modal").style.display = "block";

  wifiWS = new WebSocket("ws://" + location.host + "/ws");

  wifiWS.onmessage = function(event){
    document.getElementById("wifi-status").textContent = event.data;
  };
}

function closeWiFiModal(){
  document.getElementById("wifi-modal").style.display = "none";
  if(wifiWS) wifiWS.close();
}

// Scan
function scanWiFi(){
  fetch("/scan")
  .then(r => r.json())
  .then(data => {
    let list = document.getElementById("wifi-networks");
    list.innerHTML = "";

    data.networks.forEach(n => {
      let li = document.createElement("li");

      li.textContent = `${n.ssid} (${n.rssi} dBm)`;

      li.onclick = () => {
        document.getElementById("wifi-ssid").value = n.ssid;
      };

      list.appendChild(li);
    });
  });
}

// Save
function saveWiFi(){
  fetch("/setup", {
    method: "POST",
    headers: {"Content-Type":"application/json"},
    body: JSON.stringify({
      ssid: document.getElementById("wifi-ssid").value,
      pass: document.getElementById("wifi-pass").value
    })
  })
  .then(r => r.json())
  .then(() => {
    alert("Gespeichert! Neustart...");
  });
}

window.onload = function() {
  showCocktails();
};
</script>
</body>
</html>
    
    )rawliteral";