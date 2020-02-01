[Low frequency tag operations](https://github.com/Proxmark/proxmark3/wiki/LF-Tag-Operations#low-frequency-tag-operations)   
[Reading an unknown tag](https://github.com/Proxmark/proxmark3/wiki/LF-Tag-Operations#reading-an-unknown-tag)   
[Finding the tag's bitstream period](https://github.com/Proxmark/proxmark3/wiki/LF-Tag-Operations#finding-the-tags-bitstream-period)   
[Modulation type](https://github.com/Proxmark/proxmark3/wiki/LF-Tag-Operations#modulation-type)   
[If you want to try yourself](https://github.com/Proxmark/proxmark3/wiki/LF-Tag-Operations#if-you-want-to-try-yourself)    

***

### Low frequency tag operations
This page presents how the Proxmark3 can be used for working with simple low frequency tags. The commands are valid for the current GitHub version of the Proxmark (the askdemod command argument format was recently modified).

### Reading an unknown tag
```
- Update - 
Try the new 'lf search' command.
```

Below is a scan of an unknown physical access tag which I wanted to identify : a "thick" clamshell kind of tag, meaning with 99% certainty a LF tag.


The first and obvious operation when faced with an unknown LF tag is to excite it with either 125kHz or 134kHz in order to see how it will react :

`proxmark3> lf read `  

Note : use lf config to configure the read command to read the tag with additional settings, like 134kHz.

Then you can download the trace using data samples. By default, data samples only downloads the beginning of the trace. Use data samples 20000 (or any other value below 40000) to download a longer trace.

`proxmark3> data samples 20000 `   
`proxmark3> data plot `  

From the waveform, the tag does a simple ASK bitstream modulation : the tag modulates the signal amplitude to transmit its bitstream to the reader.

You can use the arrow keys to navigate the trace : at this point, you should have a pretty clear idea of whether the tag does expect a simple LF carrier or needs something more sophisticated.

In order to get a meaningful reading when setting the purple and yellow markers, you can use the scale command to set the sampling frequency :

`proxmark3> data scale 125  `  

### Finding the tag’s bitstream period
Once you are satisfied with the acquired trace, the next step is to determine whether the tag’s signal is send in a periodic way (i.e. repeated) : the proper and simple way to do this is to autocorrelate the signal and find the peak period : the proxmark client offers a simple autocorrelation feature to this end :

`proxmark3> data autocorr 2000 g`   

The plot will be updated :   
![](http://i.imgur.com/uKbha5n.png)

In the example above, it is obvious the tag has a 4096 samples period. One sample period is equal to the carrier period since the Proxmark3 samples at the same rate as the carrier frequency. 1 carrier clock period being 8µs at 125kHz, the complete word is therefore sent in 32ms in the example above.
```
- Update - 
data autocorr will now also output to the cli window the auto detected
correlation. Remove the 'g' from the command and it won't overwrite 
your tag's plot, but it will output what it found.
```

### Modulation type
The next step is to understand how each symbol is transmitted. Zooming in (keyboard DOWN ARROW), and looking at the trace — this is where a bit of practice helps — we can isolate a "short" period, which corresponds to 64 samples : if this is right, the whole message is therefore 4096/64=64 bits long, which makes sense.   
![](http://i.imgur.com/Iuowkrc.png)   

Now, let’s try to extract a meaningful bitstream from the tag. By using the askdemod command which is available in firmware 20090328 and later, we can turn the analog capture into a nicer looking bitstream which will be ready for further analysis : we will try with a positive bit encoding convention. What does this mean ? Depending on the tag manufacturer’s design, field modulation will either mean a logical "0" or a logical "1". The askdemod command therefore gives a choice to decode the bitstream one way or another.   

`proxmark3> data askdemod 1`  
```
- Update - 
now use the 'data dirthreshold <thres up> <thres down>' or 'data askedgedetect'
```
On the trace, the bitstream now looks like this :   
![](http://i.imgur.com/GvTcfy8.png)   
You can see on the trace that there are "long" and "short" zero and one modulations : typically, this indicates some sort of manchester encoding. We can use the Proxmark’s mandemod command to attempt a manchester demodulation of the bitstream. This command takes the clock period as its argument.

Since the July 2009 SVN versions, you do not need to give the clock rate as argument anymore, since the mandemod function can now autodetect the clock rate.   

`proxmark3> data mandemod 64  `   
```
- Update - 
now use the 'data rawdemod am'
```
and this is the Manchester demodulated answer:   
> Unsynchronized, resync...   
> (too many of those messages mean the stream is not Manchester encoded)   
> Unsynchronized, resync...   
> (too many of those messages mean the stream is not Manchester encoded)   
> Manchester decoded bitstream   
>  
> 0 0 0 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0   
0 0 1 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0   
0 0 1 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0   
0 0 1 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0   
0 0 1 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0   
0 0 1 1 0 0 0 0 0 0 1 1 1 1 0 0   
1 1 0 1 0 1 0 0 1 1 1 1 0 0 1 0   
0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1   
0 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0   
proxmark3>   

The two ’unsynchronized’ messages indicate the demodulator is trying to synchronize on the bitstream, which is nothing to worry about unless the output sends tons of such messages, which would indicate that the stream is definitely not manchester encoded.

From the binary dump above, we can indeed isolate a regular "111111111" message every 64 bits : most probably a header. A unitary tag message is therefore :

> 111111111   
> 0000000011000000111100110101001111001001001010011001010   

There are 55 remaining bits : not a "binary-friendly" value unless : 55 is 11*5 or 10(4+1) : it is possible that we actually have 11 nibbles with one extra parity bit. If we decode the stream this way by discarding every 5th bit, we end up with the following value :

> 0000 0 -> 0x0   
> 0001 1 -> 0x1   
> 0000 0 -> 0x0   
> 0111 1 -> 0x7   
> 0011 0 -> 0x3   
> 1010 0 -> 0xA   
> 1111 0 -> 0xF   
> 0100 1 -> 0x4   
> 0010 1 -> 0x2   
> 0011 0 -> 0x3   
> 0101 0 -> 0x5   

Which is 0x01073AF4235 : exactly the value which is printed on the badge ! — plus one extra nibble, probably parity too.

By doing more research on the Internet, we can verify that this type of stream format is compliant with the datasheet of the EM Microelectronic EM4100 family : we can therefore safely assume that the tags indeed contain an EM4100 or similar chip.

The next logical step in the Proxmark world would be to verify that the Proxmark, beyond acting as a reader, can actually simulate those tags, either by replaying a captured bitstream, or by taking a tag’s serial number but this is not part of this guide.

### If you want to try yourself...

Here is the [raw trace](https://github.com/Proxmark/proxmark3/tree/master/traces/EM4102-1.pm3), use the load command on the proxmark3 command line client to load it and work on it :

More traces are available on the [trace directory](https://github.com/Proxmark/proxmark3/tree/master/traces/) on the github repository (trace description [here](https://github.com/Proxmark/proxmark3/tree/master/traces/README.txt)).