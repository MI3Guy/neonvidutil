using System;
using System.IO;

namespace WAVSharp {
	public class WAVBitDepthDetector {
		public WAVBitDepthDetector(Stream instream, Action callback) {
			reader = new WAVReader(instream);
			this.callback = callback;
		}
		
		WAVReader reader;
		Action callback;
		
		
		public int Check() {
			WAVDataChunk dataChunk = reader.ReadDataChunk();
			
			int numBits = 0;
			uint mask = ~(uint)0;
			
			WAVDataSample sample;
			while((sample = dataChunk.ReadSample()) != null && numBits < sizeof(uint)*8) {
				callback();
				
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

