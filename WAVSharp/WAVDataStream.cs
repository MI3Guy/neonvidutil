using System;
using System.IO;

namespace WAVSharp {
	public class WAVDataStream : Stream {
		public WAVDataStream(WAVDataChunk dataChunk, WAVFormatChunk chunk) {
			this.dataChunk = dataChunk;
			sampleSize = chunk.wBitsPerSample / 8 * chunk.nChannels;
			buff = new byte[sampleSize];
			buffCount = 0;
		}
		
		private WAVDataChunk dataChunk;
		private int sampleSize;
		byte[] buff;
		int buffCount;
		
		public override bool CanRead {
			get {
				return true;
			}
		}
		
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		public override bool CanWrite {
			get {
				return false;
			}
		}
		
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}
		
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		public override void Flush() {
			
		}
		
		public override int Read(byte[] buffer, int offset, int count) {
			int total = 0;
			try {
				if(buffCount > 0) {
					if(buffCount > count) {
						Array.Copy(buff, 0, buffer, offset, count);
						byte[] tmp = new byte[buffCount - count];
						Array.Copy(buff, count, tmp, 0, tmp.Length);
						Array.Copy(tmp, buff, tmp.Length);
						buffCount = tmp.Length;
						total += count;
						
						offset += count;
						count = 0;
					}
					else {
						Array.Copy(buff, 0, buffer, offset, buffCount);
						total += buffCount;
						buffCount = 0;
					}
				}
				if(count > 0) {
					int numWritten = 0;
					for(int i = offset; i < count - sampleSize; i += sampleSize) {
						WAVDataSample sample = dataChunk.ReadSample();
						if(sample == null) return total;
						
						Array.Copy(sample.Data, 0, buffer, i, sampleSize);
						numWritten += sampleSize;
						total += sampleSize;
					}
					
					if(numWritten < count) {
						WAVDataSample sample = dataChunk.ReadSample();
						if(sample == null) return total;
						
						Array.Copy(sample.Data, 0, buffer, offset + numWritten, count - numWritten);
						Array.Copy(sample.Data, count - numWritten, buff, 0, sampleSize - (count - numWritten));
						buffCount = sampleSize - (count - numWritten);
						total += count - numWritten;
					}
				}
			}
			catch(EndOfStreamException) {
				// Number of bytes written already is in total. No need to do anything.
			}
			
			return total;
		}
		
		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}
		
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}
		
		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotSupportedException();
		}
		
	}
}

