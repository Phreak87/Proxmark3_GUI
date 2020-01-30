This page will walk new users through the process of analysing EM410X cards/tags.

Open proxmark:
<pre>
cd proxmark3
proxmark3 <com port | tty >
</pre>
Loading a previous trace:
<pre>
proxmark3> data load traces/EM4102-1.pm3 
loaded 16000 samples    
</pre> 
To display the wave form use the following command
<pre>proxmark3> data plot    </pre>
EM410X cards use ASK modulation, so use the askdemod command to demodulate to low(0) or high(1) signals, depending on which one you use you either end up with a pattern or an inverted pattern of bits:

```
- Update -
use instead the 'data rawdemod am' command
and you are done...
```

<pre>proxmark3> data askdemod 1</pre>
Lastly EM4100 tags additionally use Manchester modulation, use the following command to demodulate the Manchester Encoding:
<pre>proxmark3> data mandemod 64
Warning: Manchester decode error for pulse width detection.          
(too many of those messages mean either the stream is not Manchester encoded, or clock is wrong)          
Unsynchronized, resync...          
(too many of those messages mean the stream is not Manchester encoded)          
Manchester decoded bitstream          
0 0 1 0 1 1 1 1 0 1 0 1 1 1 1 0          
1 1 1 1 1 1 0 0 0 1 1 1 0 0 1 1          
1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 1          
1 0 0 0 0 0 1 0 0 0 1 0 1 1 1 1          
0 0 1 0 1 1 1 1 0 1 0 1 1 1 1 0          
1 1 1 1 1 1 0 0 0 1 1 1 0 0 1 1          
1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 1          
1 0 0 0 0 0 1 0 0 0 1 0 1 1 1 1          
0 0 1 0 1 1 1 1 0 1 0 1 1 1 1 0          
1 1 1 1 1 1 0 0 0 1 1 1 0 0 1 1          
1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 1          
1 0 0 0 0 0 1 0 0 0 1 0 1 1 1 1          
0 0 1 0 1 1 1 1 0 1 0 1 1 1 1 0          
1 1 1 1 1 1 0 0 0 1 1 1 0 0 1 1          
1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 1  </pre>
You should see the following repetitive pattern, starting from the Header sequence (nine 1's):
<pre>1111111110000000011000001000101111001011110101111011111100011100</pre>
Remove the Header bits (9x 1's) and note every 4th bit is a Parity bit
<pre>1 1 1 1 1 1 1 1 1 
0 0 0 0| 0|
0 0 0 1| 1|
0 0 0 0| 0|
1 0 0 0| 1|
0 1 1 1| 1|
0 0 1 0| 1|
1 1 1 0| 1|
0 1 1 1| 1|
0 1 1 1| 1|
1 1 0 0| 0|
column parity & stop bit
1 1 1 0| 0
</pre>
Translating the 4-bit codes should result in the following id:
 0x010872E77C
