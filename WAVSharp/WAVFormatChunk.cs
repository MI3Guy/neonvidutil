using System;
using System.IO;
using System.Text;

namespace WAVSharp {
	public class WAVFormatChunk {
		public WAVFormatChunk(BinaryReader reader) {
			cksize = reader.ReadUInt32();
			
			uint numRead = expectedSizeSimple;
			
			wFormatTag = (WAVConst.FormatTag)reader.ReadUInt16();
			nChannels = reader.ReadUInt16();
			nSamplesPerSec = reader.ReadUInt32();
			nAvgBytesPerSec = reader.ReadUInt32();
			nBlockAlign = reader.ReadUInt16();
			wBitsPerSample = reader.ReadUInt16();
			if(wFormatTag == WAVConst.FormatTag.EXTENSIBLE) {
				cbSize = reader.ReadUInt16();
				numRead = expectedSizeMore;
				if(cbSize > 0) {
					wValidBitsPerSample = reader.ReadUInt16();
					dwChannelMask = reader.ReadUInt32();
					SubFormat = new Guid(reader.ReadBytes(16));
					numRead = expectedSizeFull;
				}
			}
			
			if(cksize > numRead) {
				reader.ReadBytesIgnoreUInt(cksize - numRead);
			}
			else if(numRead > cksize) {
				throw new ApplicationException("More bytes were read than were in the header.");
			}
			
		}
		const uint expectedSizeSimple = 2 + 2 + 4 + 4 + 2 + 2;
		const uint expectedSizeMore = expectedSizeSimple + 2;
		const uint expectedSizeFull = expectedSizeMore + 2 + 4 + 16;
		
		public WAVFormatChunk() {
			
		}
		
		public WAVFormatChunk(WAVFormatChunk other, int bits) {
			ushort BytesPerSample = (ushort)Math.Ceiling((double)bits / 8.0);
			ushort bitsPerSample = (ushort)(8 * BytesPerSample);
			
			cksize = WAVConst.FormatChunkSizeExtensible;
			wFormatTag = WAVConst.FormatTag.EXTENSIBLE;
			nChannels = other.nChannels;
			nSamplesPerSec = other.nSamplesPerSec;
			nAvgBytesPerSec = (uint)((ulong)(other.nAvgBytesPerSec - other.cksize) * bitsPerSample / other.wBitsPerSample) + cksize;
			nBlockAlign = (ushort)(BytesPerSample * nChannels);
			wBitsPerSample = bitsPerSample;
			cbSize = WAVConst.FormatChunkExtensibleExtSize;
			wValidBitsPerSample = (ushort)bits;
			dwChannelMask = other.dwChannelMask;
			SubFormat = WAVConst.FormatSubtypePCM;
		}
		
		public long FileLength;
		public uint cksize;
		public WAVConst.FormatTag wFormatTag;
		public ushort nChannels;
		public uint nSamplesPerSec;
		public uint nAvgBytesPerSec;
		public ushort nBlockAlign;
		public ushort wBitsPerSample;
		public ushort cbSize;
		public ushort wValidBitsPerSample;
		public uint dwChannelMask;
		public Guid SubFormat;
		
		
		public void WriteTo(BinaryWriter writer) {
			writer.Write(Encoding.ASCII.GetBytes(WAVConst.ChunkIdFormat));
			writer.Write(cksize);
			writer.Write((ushort)wFormatTag);
			writer.Write(nChannels);
			writer.Write(nSamplesPerSec);
			writer.Write(nAvgBytesPerSec);
			writer.Write(nBlockAlign);
			writer.Write(wBitsPerSample);
			if(wFormatTag == WAVConst.FormatTag.EXTENSIBLE) {
				writer.Write(cbSize);
				if(cbSize > 0) {
					writer.Write(wValidBitsPerSample);
					writer.Write(dwChannelMask);
					writer.Write(SubFormat.ToByteArray());
				}
			}
		}
	}
}

