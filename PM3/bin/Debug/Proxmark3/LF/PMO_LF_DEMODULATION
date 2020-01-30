LF Modulations: FSK, ASK, ASK/Manchester, ASK/Biphase, ASK/Diphase, NRZ, PSK1, PSK2.  
What do they mean?
How does it work?

Learn for yourself using the proxmark3's new graphing functions.

This walk through will take a simple HID tag and examine the FSK2a waveform in the new graph.

**First, place your tag on the antenna and issue a** `lf search` **command:**
```
proxmark3> lf search
NOTE: some demods output possible binary
  if it finds something that looks like a tag
False Positives ARE possible

Checking for known tags:

HID Prox TAG ID: 2006020002 (1) - Format Len: 26bit - FC: 1 - Card: 1

Valid HID Prox ID Found!

Valid T55xx Chip Found
Try lf t55xx ... commands
```
NOTE: a lot is happening in the `lf search` but that is a topic for another time.  For now we are content to know it found the tag we want to see.

**Next let us see the full raw binary for the tag we just read, so issue a** `data printdemod` **to output the demodulated tag buffer from the previous demodulation command (lf search)**
```
proxmark3> data printdemod
DemodBuffer:
0001110101010101
0101100101010101
0101010101101001
0101010101011001
0101010101010101
0101010101011001
```
NOTE: How the raw binary ties to the FC 1 and Card# 1 of our tag is dependent on the format definition of this particular tag, and is a topic for another time.  (or see the forum as there is much documented there)

**Then do a** `data plot` **to open the graph**
```
proxmark3> data plot
```
**Move around in the graph with the arrow keys until you find the blue lines** (where lf search got it's data within the repeating waveform) 

NOTE: for more details on the graph press h while on the graph then look back at the normal pm3 window, additional details are a topic for another time)

**Results:**

![hid graph](http://i.imgur.com/EY6YHxM.png)

Now the binary we got from the `data printdemod` matches the binary in blue on the graph.

**Looking at the graph and the how waves translate to 1s and 0s we notice that for this "FSK2a modulation" wider spaced waves (often taller), translate into a 1 bit, while narrower spaced waves (often shorter) translate into a 0 bit.  **
![graph zoomed in](http://i.imgur.com/vD61J9q.png)
**This is the frequency changing, thus FSK = Frequency Shifting Key.**
Note: "FSK2 modulation" (no a) would be the opposite, wider spaced waves = 0, while narrower = 1

That along with the note at the bottom of the screen that tells you that the gridX is 50 (or data rate = RF/50 or 50 samples at 125khz equals one binary bit, but this too is another subject) we learn all we need to know to be able to manually demodulate this type of tag from the graph in the future. (granted we have automatic tools for this particular tag type)

Now try other modulations, let the graph be your guide!