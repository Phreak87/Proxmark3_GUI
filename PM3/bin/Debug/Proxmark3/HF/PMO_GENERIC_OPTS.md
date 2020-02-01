* [Introduction](#Introduction)
* [Details](#Details)


***

## Introduction
Commands related to generic ISO 14443 cards can be invoked from the `hf 14a` command subtree.  

```ruby
proxmark3>  hf 14a
help             This help
hf list 14a      List ISO 14443a history 
reader           Act like an ISO14443 Type A reader
cuids            <n> Collect n>0 ISO14443 Type A UIDs in one go
sim              <UID> -- Fake ISO 14443a tag
snoop            Eavesdrop ISO 14443 Type A
raw              Send raw hex data to tag
```

## Details
### hf 14a snoop
In this mode the Proxmark will log all traffic between RFID readers and devices in range until the hardware button is pressed.

The logged traffic can then be printed with `hf list 14a`.

### hf list 14a
This command lists all logged ISO 14443 Type A messages from either a previous communication between the Proxmark and a card or from a previous `hf 14a snoop` operation.

```ruby
proxmark3> hf list 14a 
Recorded Activity          
Start = Start of Start Bit, End = End of last modulation. Src = Source of Transfer          
iso14443a - All times are in carrier periods (1/13.56Mhz)          
iClass    - Timings are not as accurate          
     Start |       End | Src | Data (! denotes parity error)                                   | CRC | Annotation         |          
-----------|-----------|-----|-----------------------------------------------------------------|-----|--------------------|          
         0 |       992 | Rdr | 52                                                              |     | WUPA          
      2228 |      4596 | Tag | 04  00                                                          |     |           
      7040 |      9504 | Rdr | 93  20                                                          |     | ANTICOLL          
     10676 |     16500 | Tag | 9e  f5  69  0b  09                                              |     |           
     18560 |     29088 | Rdr | 93  70  9e  f5  69  0b  09  77  7f                              |     | ANTICOLL          
     30260 |     33780 | Tag | 08  b6  dd                                                      |     |           
    419456 |    424224 | Rdr | e0  80  31  73                                                  |     | RATS          
    425396 |    426036 | Tag | 04                                                              |     |           
```

### hf list save / hf list 14a l

`hf list save` saves a trace stored in Proxmark3's RAM to a file:

```ruby
proxmark3> hf list save myTrace.trc
Recorded Activity (TraceLen = 700 bytes) written to file myTrace.trc
```

This can be read later with `hf list 14a l`:

```ruby
proxmark3> hf list 14a l myTrace.trc
Recorded Activity          
     Start |       End | Src | Data (! denotes parity error)                                   | CRC | Annotation         |          
-----------|-----------|-----|-----------------------------------------------------------------|-----|--------------------|          
         0 |       992 | Rdr | 52                                                              |     | WUPA          
      2228 |      4596 | Tag | 04  00                                                          |     |           
```
   
### hf 14a reader
This command executes the ISO 14443-3 Type A anticollision procedure and prints out the info gathered from the card's responses.   

Example:
```ruby
proxmark3> hf 14a reader
ATQA : 00 04          
 UID : 9e f5 69 0b           
 SAK : 08 [2]          
TYPE : NXP MIFARE CLASSIC 1k | Plus 2k SL1          
proprietary non iso14443-4 card found, RATS not supported 
```

### hf 14a cuids
This essentially does nothing else than `hf 14a reader` except that it only prints out the card's UID and can do so an arbitrary number of times unattended.

This is useful e.g. to analyze the RNG of a card that generates random UIDs. The output is encased in timestamps to indicate the duration of the complete procedure.

Example:
```ruby
proxmark3> hf 14a cuids 5
Collecting 5 UIDs          
Start: 1345850802          
08F400A2          
08333494          
08B0A8B8          
085D35CE          
0862A7CB          
End: 1345850804      
``` 

### hf 14a sim
With this the Proxmark can emulate an ISO 14443 compliant RFID card. _No more information available, feel free to add some_

### hf 14a raw
This option allows to send standard ISO14443A or non-standard ISO14443A commands to a ISO14443A compatible tags. This command supports crc auto-calculation and also partial byte sending (useful in case of proprietary commands as happens with mifare of mifare-chinese-changeable-UID tags which uses some 7bits commands).

Example:
``` ruby
proxmark3> hf 14a raw  -c  -s  3000
received 4 octets          
9E F5 69 0B           
received 1 octets          
04     
``` 