The ProxmarkIII device has the capability of reading and writing almost any RFID that operates at 125kHz, 134kHz or 13.56MHz. As of GitHub build (after google code r850), the Proxmark III supports the following formats:

### Card types / Formats
Note the following list can be out of date...    

### 13.56MHz
* iClass
* Legic
* Mifare Classic (officials and changeable UID)
* Mifare Ultralight (officials and changeable UID)
* Mifare Ultralight C (officials and changeable UID)
* Mifare Ultralight EV1
* NTAG 203, 213, 215, 216 (part of hf mfu)
* SRI512
* SRIX4K (authenticate command not supported)
* Some EID (Electronic Identification Documents)

 
**NOTE**    
With RAW COMMANDS (pass-through) almost any 13.56 tag using the ISO14443A, 14443B and ISO15693 standard can be supported (some functions may require password/authentication sequence - read specific product datasheet for more info); ISO14443B' (Innovatron pre-ISO14443B standard) is not offically supported (lack of documentation). Some non-standard commands (ex. less-than-8-bits commands) are also supported.

### 125 / 134 kHz
* AWID
* Cotag R/O
* EM410x R/O
* EM4x05 R/W
* FDX-B
* FlexPass
* HID
* HiTAG R/W
* Indala
* Kantech ioProx
* Paradox
* PCF7931 R/W
* Presco
* Pyramid (Farpointe Data)
* T55xx R/W
* TI R/O
* Visa2000

An extended tag list with specific features can be found [here](https://docs.google.com/spreadsheet/ccc?key=0AhSppbiz67RGdDZlSWlBQnF5RDN5Nm1HckRBU1VtQ3c#gid=0). Please note that many of them have never been tested with Proxmark III hardware.