#### How can I read a card contents?  
- `hf mf rdbl 0 a ffffffffffff`
  - `0`: block number
  - `a`: key type
  - `ffffffffffff`: key
- `hf mf rdsc 0 a ffffffffffff`
  - `0`: sector number
  - `a`: key type
  - `ffffffffffff`: key  

#### How can I write a block into a card?  
* 'hf mf wrbl 0 a ffffffffffff 000102030405060708090a0b0c0d0e0f', where 0 - block number, a - key type, ffffffffffff - key,  
000102030405060708090a0b0c0d0e0f - block data.  

#### How can I break a card?  
* 'hf mf mifare'  
 if it doesn't found a key: 'hf mf mifare XXXXXXXX' , where XXXXXXXX - Nt from previous run  
 'hf mf nested 1 0 a FFFFFFFFFFFF', where 1 - card type MIFARE CLASSIC 1k, FFFFFFFFFFFF - key that found at previous step.  

#### How to save emulator dump from a card  
* 'hf mf mifare'  
* if it doesn't found a key: 'hf mf mifare XXXXXXXX' , where XXXXXXXX - Nt from previous run  
* 'hf mf nested 1 0 a FFFFFFFFFFFF t', where 1 - card type MIFARE CLASSIC 1k, FFFFFFFFFFFF - key that found at previous step.  
* 'hf mf efill a FFFFFFFFFFFF'  
* 'hf mf esave filename'  

#### How to emulate a card  
* 'hf mf mifare'  
* if it doesn't found a key: 'hf mf mifare XXXXXXXX' , where XXXXXXXX - Nt from previous run  
* 'hf mf nested 1 0 a FFFFFFFFFFFF t', where 1 - card type MIFARE CLASSIC 1k, FFFFFFFFFFFF - key that found at previous step.  
* 'hf mf efill a FFFFFFFFFFFF'  
* 'hf mf sim'  

#### How to emulate a new card  
* 'hf mf eclr'  
* 'hf mf sim'  

#### How to emulate a card with help of dump from file  
* 'hf mf eload filename', where filename - dump's file name (<filename>.eml)  
* 'hf mf sim'  

#### How to have look at the emulator memory  
* 'hf mf eget 00', where 00 - block number from 0 to 0x63. Each block contains 16 bytes of memory.  

#### How to make changes into the emulator memory  
* 'hf mf eset 01 000102030405060708090a0b0c0d0e0f',  
where:  
* 00 - block number from 0 to 0x63. Each block contains 16 bytes of memory.  
* 000102030405060708090a0b0c0d0e0f - block data.  