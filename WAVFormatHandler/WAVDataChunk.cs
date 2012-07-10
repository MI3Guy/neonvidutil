using System;
using System.IO;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVDataChunk {
		public WAVDataChunk(BinaryReader reader, WAVFormatChunk formatChunk) {
			this.reader = reader;
			this.formatChunk = formatChunk;
			
			uint cksize = reader.ReadUInt32();
			
			bytesPerSample = formatChunk.wBitsPerSample / 8;
			numChannels = formatChunk.nChannels;
		}
		
		private BinaryReader reader;
		private WAVFormatChunk formatChunk;
		private int bytesPerSample;
		private int numChannels;
		
		public WAVDataSample ReadSample() {
			return new WAVDataSample(reader, bytesPerSample, numChannels);
		}
	}
}

