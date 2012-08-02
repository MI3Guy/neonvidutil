using System;
using System.IO;
using System.Text;

namespace WAVSharp {
	public class WAVDataChunk {
		public WAVDataChunk(BinaryReader reader, WAVFormatChunk formatChunk) {
			this.reader = reader;
			this.formatChunk = formatChunk;
			
			cksize = reader.ReadUInt32();
			
			bytesPerSample = formatChunk.wBitsPerSample / 8;
			numChannels = formatChunk.nChannels;
			
			useStartPos = reader.BaseStream is FileStream && reader.BaseStream.Length <= uint.MaxValue && cksize != 0;
			if(useStartPos) startPos = reader.BaseStream.Position;
		}
		
		public WAVDataChunk() {
			
		}
		
		public uint cksize;
		
		private BinaryReader reader;
		private WAVFormatChunk formatChunk;
		private int bytesPerSample;
		private int numChannels;
		
		private bool useStartPos;
		private long startPos;
		
		public WAVDataSample ReadSample() {
			try {
				WAVDataSample sample = new WAVDataSample(reader, bytesPerSample, (formatChunk.cbSize > 0) ? formatChunk.wValidBitsPerSample : formatChunk.wBitsPerSample, numChannels);
				if(useStartPos && reader.BaseStream.Position > startPos + cksize) {
					return null;
				}
				return sample;
			}
			catch(EndOfStreamException) {
				return null;
			}
		}
		
		public WAVDataStream GetPCMStream() {
			return new WAVDataStream(this, formatChunk);
		}
		
		public void WriteTo(BinaryWriter writer) {
			writer.Write(Encoding.ASCII.GetBytes(WAVConst.ChunkIdData));
			writer.Write(cksize);
		}
		
		public long CalcLength() {
			if(!useStartPos) {
				return cksize;
			}
			else {
				return reader.BaseStream.Length - startPos;
			}
		}
	}
}

