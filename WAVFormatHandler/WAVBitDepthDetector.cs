using System;
using System.IO;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVBitDepthDetector {
		public WAVBitDepthDetector(Stream instream) {
			reader = new WAVReader(instream);
			this.instream = instream;
		}
		
		Stream instream;
		WAVReader reader;
		
		
		public int Check() {
			WAVDataChunk dataChunk = reader.ReadDataChunk();
			
			int numBits = 0;
			uint mask = ~(uint)0;
			int numWritten = 0;
			
			WAVDataSample sample;
			while((sample = dataChunk.ReadSample()) != null && numBits < sizeof(uint)*8) {
				int numTotal = (int)((double)instream.Position / instream.Length * 50.0);
				if(numTotal > numWritten) {
					Console.Write(new string('-', numTotal - numWritten));
					numWritten = numTotal;
				}
				
				for(int channel = 0; channel < sample.Channels; ++channel) {
					uint data = sample.GetSampleForChannel(channel);
					
					while((data & mask) != 0 && mask != 0) {
						mask >>= 1;
						numBits += 1;
					}
				}
			}
			
			return numBits;
		}
	}
}

