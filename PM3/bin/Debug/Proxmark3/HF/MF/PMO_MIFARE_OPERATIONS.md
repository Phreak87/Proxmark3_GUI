### MIFARE Tag Operations
Operations can be executed from menu "hf 14a" or "hf mf"
```ruby
proxmark3> hf 14a
help             Display help
list             List ISO 14443a history
reader           Acts same as ISO14443 Type A reader
sim              <UID> -- Fake ISO 14443a tag
snoop            Eavesdrop ISO 14443 Type A

proxmark3> hf mf
help             This help
dbg              Set default debug mode
rdbl             Read MIFARE classic block
rdsc             Read MIFARE classic sector
dump             Dump MIFARE classic tag to binary file
restore          Restore MIFARE classic binary file to BLANK tag
wrbl             Write MIFARE classic block
chk              Test block up to 8 keys
mifare           Read parity error messages. param - <used card nonce>
nested           Test nested authentication
sim              Simulate MIFARE 1k card
eclr             Clear simulator memory block
eget             Get simulator memory block
eset             Set simulator memory block
eload            Load from file emul dump
esave            Save to file emul dump
ecfill           Fill simulator memory with help of keys from simulator
ekeyprn          Print keys from simulator memory
.....
```
Short MIFARE HOWTO can be found here: [MIFARE HOW-TO](https://github.com/Proxmark/proxmark3/wiki/Mifare-HowTo) 
 
******

**THERE ARE MORE COMMANDS IN THE LATEST PROXMARK3 EXECUTABLE WHICH ARE NOT DESCRIBED HERE**: they are mainly used to interact with Chinese Changeable UID Mifare cards and with Mifare Ultralight/C cards

******

### Command set 'hf 14a'
#### hf 14a list
It displays the log of communication between the card and either proxmark or reader in 'hf 14a snoop'

Sample:
```ruby
proxmark3> hf mf rdbl 0 a ffffffffffff
--block no:00 key type:00 key:ff ff ff ff ff ff
#db# READ BLOCK FINISHED
isOk:01 data:e6 84 87 f3 16 88 04 00 46 8e 45 55 4d 70 41 04

proxmark3> hf 14a list
recorded activity:
 ETU     :rssi: who bytes
---------+----+----+-----------
 +      0:    :     52
 +    236:   0: TAG 04  00
 +      0:    :     93  20
 +    452:   0: TAG e6  84  87  f3  16
 +      0:    :     93  70  e6  84  87  f3  16  5e  35
 +    308:   0: TAG 08  b6  dd
 +      0:    :     60  00  f5  7b
 +    428:   0: TAG 11  67  0f  29
 +      0:    :     62  c6  da  97  5a  07  ab  21     !crc
 +    380:   0: TAG eb! 31  34! 96!
 +      0:    :     e0  08  2b  d1     !crc
 +   1396:   0: TAG c4! 33! 62! 23! 46! 3d  6d  60  38  22! 04! b0  b8  82! 05 e0! 80  fe!    !crc
 +      0:    :     40  48  02  fd     !crc
```

#### hf 14a reader
It employs anticollision and reads several parameters from the card

Sample:
```ruby
proxmark3> hf 14a reader
ATQA : 04 00
 UID : e6 84 87 f3 00 00 00 00 00 00 00 00
 SAK : 08 [2]
 SAK : MIFARE CLASSIC 1K
proprietary non-iso14443a card found, RATS not supported
```

#### hf 14a sim
Simulator up to end of anticollision state of the card.

#### hf 14a snoop
It sniffs communication between the card and the reader.

Sample of reading card from another reader:

* anticollision
* select [WUPA]
* authenticate sector 1 with key A and key ffffffffffff
* read block 4 (<09090909090909090909090909090909>)
* halt

```ruby
proxmark3> hf 14a snoop
#db# cancelled_a
#db# 4 0 4
#db# 20 af 7f
proxmark3> #db# COMMAND FINISHED                 
proxmark3> #db# maxDataLen=3, Uart.state=0, Uart.len=0                 
proxmark3> #db# traceLen=2980, Uart.output[0]=00000018                 
proxmark3> hf list 14a 
Recorded Activity          
Start = Start of Start Bit, End = End of last modulation. Src = Source of Transfer          
iso14443a - All times are in carrier periods (1/13.56Mhz)          
iClass    - Timings are not as accurate          
     Start |       End | Src | Data (! denotes parity error)                                   | CRC | Annotation         |          
-----------|-----------|-----|-----------------------------------------------------------------|-----|--------------------|          
   4077024 |   4079392 | Tag | 04  00                                                          |     |           
   4086924 |   4097452 | Rdr | 93  20                                                          |     | ANTICOLL          
   4098640 |   4102160 | Tag | e6  84  87  f3  16                                              |     |           
   4186428 |   4185666 | Rdr | 93  70  e6  84  87  f3  16  5e  35                              |     | ANTICOLL  
   4243152 |   4243288 | Tag | 5c  22  92  7f                                                  |     |        
   4256752 |   4256988 | Rdr | 60  04  d1  3d                                                  |     | AUTH-A         
   4289152 |   4289288 | Tag | 5c  22  92  7f                                                  |     |          
   4345152 |   4345288 | Rdr | 3b  7c  58  2b  07  32  e7  e9                                  |     |          
   4379052 |   4379388 | Tag | 76  b5! 82  f7!                                                 |     |          
   4380987 |   4382788 | Rdr | d5  c1  1c  d9     !crc                                         |     |           
.......      
```

### Command set 'hf mf'
#### hf mf dbg   
It sets debug level of commands in "hf mf" menu.

Debug levels:
0 - no debug messages  
1 - error messages  
2 - all messages  
4 - extended debug mode  
 
Level 1 or 0 is recommended. With advanced debugging, some commands may work abnormally (because of the time required to print debug message).

#### hf mf rdbl  
It reads block from a mifare card.

`hf mf rdbl <block number> <key A/B> <key (12 hex symbols)>`

* block number must be between 0x00 and 0xFF  
* key must be either 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* key must be 12 hex symbols (for example: FFFFFFFFFFFF)  
Command to read block 0 from mifare card with authentication params: key A, key FFFFFFFFFFFF:

**correct execution**
```ruby
proxmark3> hf mf rdbl 0 a ffffffffffff
--block no:00 key type:00 key:ff ff ff ff ff ff
#db# READ BLOCK FINISHED
isOk:01 data:e6 84 87 f3 16 88 04 00 46 8e 45 55 4d 70 41 04
```

`isOk:01` - the command is executed correctly 'data:...' - block data

**incorrect execution**
```ruby
proxmark3> hf mf rdbl 0 a ffffffffffff
--block no:00 key type:00 key:ff ff ff ff ff ff
#db# Can't select card
#db# READ BLOCK FINISHED
isOk:00
```

`Can't select card` - text error.  
`isOk:00` - the command is not executed.

#### hf mf rdsc  
It reads sector from a mifare card.

`hf mf rdsc <sector number> <key A/B> <key (12 hex symbols)>`

* sector number must be between 0x00 and 0x3F  
* key must be 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* key must be 12 hex symbols (for example: FFFFFFFFFFFF)  
Command to read sector 0 from mifare card with authentication params: key A, key FFFFFFFFFFFF:

**correct execution**
```ruby
proxmark3> hf mf rdsc 0 a ffffffffffff
--sector no:00 key type:00 key:ff ff ff ff ff ff
#db# READ SECTOR FINISHED

isOk:01
data:e6 84 87 f3 16 88 04 00 46 8e 45 55 4d 70 41 04
data:02 02 02 02 02 02 02 02 02 02 02 02 02 02 02 02

data:00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
data:00 00 00 00 00 00 ff 07 80 69 ff ff ff ff ff ff
```

`isOk:01` - the command is executed correctly.  
`data:...` - sector data. blocks 0-4 of sector.


**incorrect execution**
```ruby
--block no:4, key type:A, key:ff ff ff ff ff ff  
#db# Cmd Error: 04       
#db# Read block error       
isOk:00
Key must include 12 HEX symbols

isOk:00
```

`Key is not correct` - text error.  
`isOk:00` the command is not executed.

#### hf mf wrbl  
It writes block onto mifare card.  

`hf mf wrbl <block number> <key A/B> <key (12 hex symbols)> <block data (32 hex symbols)>`  

* block number must be between 0x00 and 0xFF  
* key must be 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* key must be 12 hex symbols (8 bytes) (for example: FFFFFFFFFFFF)  
* block data must be 32 hex symbols (16 bytes) (for example: FFFFFFFFFFFF)  
Do not try to write into the sector trailers unless you know what you are doing!!!  
It may damage the card! 
 
**correct execution**
```ruby
proxmark3> hf mf wrbl 4 a ffffffffffff 01010101010101010101010101010101
--block no:04 key type:00 key:ff ff ff ff ff ff
--data: 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01
#db# WRITE BLOCK FINISHED
isOk:01
```

`isOk:01` - the command is executed correctly.  

After that, I have issued the read command to check:  

```ruby
proxmark3> hf mf rdbl 4 a ffffffffffff
--block no:04 key type:00 key:ff ff ff ff ff ff
#db# READ BLOCK FINISHED
isOk:01 data:01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01
```
**incorrect execution**  

```ruby
proxmark3> hf mf wrbl 0 a ffffffffffff 01010101010101010101010101010101
--block no:00 key type:00 key:ff ff ff ff ff ff
--data: 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01 01
#db# Can't select card
#db# WRITE BLOCK FINISHED
isOk:00
```

`Can't select card` - text error.  
`isOk:00` the command is not executed.


#### hf mf chk  
It checks several keys (up to 8) in specific card sector.  

`hf mf chk <block number> <key A/B> [<key (12 hex symbols)>]`

* block number must be between 0x00 and 0xFF  
* key type must be either 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* key must be 12 hex symbols (8 bytes) (for example: FFFFFFFFFFFF)  

Sample:  
```ruby
 proxmark3> hf mf chk 0 A FFFFFFFFFFFF a0a1a2a3a4a5 b0b1b2b3b4b5
--block no:00 key type:00 key count:3
isOk:01 valid key:ffffffffffff
```
`isOk:01` - the command is executed correctly.  
`valid key` - the correct key

#### hf mf mifare  
It implements mifare "darkside" attack.  

`hf mf mifare [wrong Nt]`

* wrong Nt - it dont use this Nt to collect statistical information.  
It is recommended that if 'hf mf mifare' found wrong key then run hf mf mifare <Nt>, where Nt - from previous run of the command.

After executing of the attack it tests key. If it found an invalid key it prints `Found invalid key. ( Nt=XXXXXXXX`.

Example of a correct execution:

```ruby
proxmark3>  hf mf mifare
-------------------------------------------------------------------------
Executing command. Expected execution time: 25sec on average  :-)
Press the key on the proxmark3 device to abort both proxmark3 and client.
-------------------------------------------------------------------------
..................
uid(aa810a1a) nt(5e012841) par(3ce4e41ce41c8c84) ks(0209080903070606) nr(2400000000)
|diff|{nr}    |ks3|ks3^5|parity         |
+----+--------+---+-----+---------------+
| 00 |00000000| 2 |  7  |0,0,1,1,1,1,0,0|
| 20 |00000020| 9 |  c  |0,0,1,0,0,1,1,1|
| 40 |00000040| 8 |  d  |0,0,1,0,0,1,1,1|
| 60 |00000060| 9 |  c  |0,0,1,1,1,0,0,0|
| 80 |00000080| 3 |  6  |0,0,1,0,0,1,1,1|
| a0 |000000a0| 7 |  2  |0,0,1,1,1,0,0,0|
| c0 |000000c0| 6 |  3  |0,0,1,1,0,0,0,1|
| e0 |000000e0| 6 |  3  |0,0,1,0,0,0,0,1|
key_count:1
------------------------------------------------------------------
Key found:ffffffffffff 
Found valid key:ffffffffffff   
```
If it founds an **invalid key** (for example ec49e598), run this:
```ruby
proxmark3> hf mf mifare ec49e598
-------------------------------------------------------------------------
Executing command. It may take up to 30 min.
Press the key on proxmark3 device to abort proxmark3.
Press the key on the proxmark3 device to abort both proxmark3 and client.
-------------------------------------------------------------------------
...................................................

isOk:01


uid(e68487f3) nt(d993ca31) par(2c4ce424d44c6c84) ks(0b0f0e0f0c0f0108)


|diff|{nr}    |ks3|ks3^5|parity         |
+----+--------+---+-----+---------------+
| 00 |00000000| b |  e  |0,0,1,1,0,1,0,0|
| 20 |00000020| f |  a  |0,0,1,1,0,0,1,0|
| 40 |00000040| e |  b  |0,0,1,0,0,1,1,1|
| 60 |00000060| f |  a  |0,0,1,0,0,1,0,0|
| 80 |00000080| c |  9  |0,0,1,0,1,0,1,1|
| a0 |000000a0| f |  a  |0,0,1,1,0,0,1,0|
| c0 |000000c0| 1 |  4  |0,0,1,1,0,1,1,0|
| e0 |000000e0| 8 |  d  |0,0,1,0,0,0,0,1|
#db# COMMAND mifare FINISHED
------------------------------------------------------------------
Key found:ffffffffffff

Found valid key:ffffffffffff
```
Result is ok.


#### hf mf nested  
It implements mifare "nested authentication" attack. It needs to know at least one sector key to use it.

There are 2 main modes:

* all sectors attack.  
Firstly it tries public keys for all sector, then makes nested attack and then tests the keys. It makes up to 10 attempts to attack one sector. You can cpecify a block number and it calculates offset from the beginning of the sector (block shift) and attacks all the rest sectors with this offset. hf mf nested <card memory> <block number> <key A/B> <key (12 hex symbols)> [t]

* one sector attack. It attacks only one sector. It intends to use for testing if card can be broken via this attack.  
hf mf nested o <block number> <key A/B> <key (12 hex symbols)> <target block number> <target key A/B> [t]

Where:

* Card memory: 0 - MINI(320 bytes), 1 - 1K, 2 - 2K, 4 - 4K, <other> - 1K  
* block number must be between 0x00 and 0xFF  
* key type must be either 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* key must be 12 hex symbols (8 bytes) (for example: FFFFFFFFFFFF)  
* target block number must be between 0x00 and 0xFF  
* target key type must be either 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  
* t - transfer keys into emulator memory. Fills the emulator memory with extracted keys.  
Example of one sector attack:
```ruby
proxmark3> hf mf nested o 0 a ffffffffffff 4 a
--block no:00 key type:00 key:ff ff ff ff ff ff  etrans:0
--target block no:04 target key type:00

..uid:e68487f3 len=5 trgbl=4 trgkey=0
uid:e68487f3 len=1 trgbl=4 trgkey=0
uid:e68487f3 len=5 trgbl=4 trgkey=0
uid:e68487f3 len=1 trgbl=4 trgkey=0
uid:e68487f3 len=2 trgbl=4 trgkey=0
uid:e68487f3 len=5 trgbl=4 trgkey=0
uid:e68487f3 len=3 trgbl=4 trgkey=0
uid:e68487f3 len=2 trgbl=4 trgkey=0
------------------------------------------------------------------
Total keys count:1657647
cnt=0 key= 31 09 f5 fe af ff
cnt=1 key= ff ff ff ff ff ff
cnt=2 key= 8c 9b fa 2f dd ff
cnt=3 key= 0e a7 5a ff ff ff
cnt=4 key= 11 c4 ff 7e dd ff
cnt=5 key= ad 1a 73 7f dd ff
cnt=6 key= f1 59 f0 2c 63 c2
cnt=7 key= 63 74 f0 2c 2f 15
cnt=8 key= 88 60 f0 2c da e8
cnt=9 key= e0 b5 f0 2c 15 60
cnt=10 key= ec 0f f0 2d 2d 74
cnt=11 key= ec b9 f0 2b fb 6a
cnt=12 key= 2c b3 f0 2d 50 20
cnt=13 key= e1 ed f0 2b fb 4a
cnt=14 key= c7 e1 f0 2b f1 ea
cnt=15 key= ab 80 f0 2b c5 ec
Found valid key:ffffffffffff
```

Example of all sectors attack:  
```ruby
proxmark3> hf mf nested 1 0 a ffffffffffff
--block no:00 key type:00 key:ff ff ff ff ff ff  etrans:0
Block shift=0
Testing known keys. Sector count=16
nested...
Iterations count: 0
|---|----------------|---|----------------|---|
|sec|key A           |res|key B           |res|
|---|----------------|---|----------------|---|
|000|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|001|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|002|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|003|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|004|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|005|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|006|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|007|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|008|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|009|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|010|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|011|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|012|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|013|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|014|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|015|  ffffffffffff  | 1 |  ffffffffffff  | 1 |
|---|----------------|---|----------------|---|
```

#### hf mf sim  
It simulates MIFARE classic tag. Tag contents is stored into the emulator memory and can be read and written by the following commands.

`hf mf sim <UID 8 hex digits>`

* 4 byte UID if specified - replaces UID that is stored into the emulator memory. However, the replacement doesn't get loaded into the memory.  

#### hf mf eclr  
It fills the memory of emulator of a blank MIFARE 1K card with default (0xFFFFFFFFFFFF) keys.

#### hf mf eget  
It gets blocks from the card emulator dump.

`hf mf eget <block number>`

* block number must be between 0x00 and 0x63  
Example of use:
```ruby
proxmark3> hf mf eget 0

data[0]:e6 84 87 f3 16 88 04 00 46 8e 45 55 4d 70 41 04
data[1]:00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
data[2]:00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
```

#### hf mf eset  
It sets block in the card emulator dump.  

`hf mf eset <block number> <block data (32 hex symbols)>`

* block number must be between 0x00 and 0x63  
* block data - '000102030405060708090a0b0c0d0e0f'  
Example of use:

`proxmark3> hf mf eset 1 000102030405060708090a0b0c0d0e0f`
Just to make sure:
```ruby
proxmark3> hf mf eget 0

data[0]:e6 84 87 f3 16 88 04 00 46 8e 45 55 4d 70 41 04
data[1]:00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f
data[2]:00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
```

#### hf mf eload  

It loads dump to the card emulator memory.

`hf mf eload <file name>`

* file name without '.eml' extension.  

#### hf mf esave  
It saves dump from the card emulator memory.  

`hf mf esave <file name>`

* file name without '.eml' extension. If the file name is empty, the file name will be the first 7 bytes in hex from emulator memory. (UID location).  

#### hf mf ecfill  
It fills the memory of MIFARE card emulator. It uses keys from the emulator memory (to view the keys: 'hf mf ekeyprn')

`hf mf efill <key A/B>`

* key must be 'A' or 'B' (authentication command is 0x60 for 'A' and 0x61 for 'B')  

#### hf mf ekeyprn  
It prints keys from the emulator memory (MIFARE classic 1K).
```ruby
proxmark3> hf mf ekeyprn
|---|----------------|----------------|
|sec|key A           |key B           |
|---|----------------|----------------|
|000|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|001|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|002|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|003|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|004|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|005|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|006|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|007|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|008|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|009|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|010|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|011|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|012|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|013|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|014|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|015|  f0f0f0f0f0f0  |  f0f0f0f0f0f0  |
|---|----------------|----------------|
```