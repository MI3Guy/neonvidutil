using System;
using System.IO;
using System.Text;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVFormatChunk {
		public WAVFormatChunk(BinaryReader reader) {
			cksize = reader.ReadUInt32();
			long prev = reader.BaseStream.Position;
			
			wFormatTag = (WAVConst.FormatTag)reader.ReadUInt16();
			nChannels = reader.ReadUInt16();
			nSamplesPerSec = reader.ReadUInt32();
			nAvgBytesPerSec = reader.ReadUInt32();
			nBlockAlign = reader.ReadUInt16();
			wBitsPerSample = reader.ReadUInt16();
			if(wFormatTag == WAVConst.FormatTag.EXTENSIBLE) {
				cbSize = reader.ReadUInt16();
				if(cbSize > 0) {
					wValidBitsPerSample = reader.ReadUInt16();
					dwChannelMask = reader.ReadUInt32();
					SubFormat = new Guid(reader.ReadBytes(16));
				}
			}
			reader.BaseStream.Position = prev + cksize;
		}
		
		public WAVFormatChunk() {
			
		}
		
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

