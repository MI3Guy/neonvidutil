using System;
using System.IO;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVFormatChunk {
		public WAVFormatChunk(BinaryReader reader) {
			uint cksize = reader.ReadUInt32();
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
	}
}

