using System;
using System.IO;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVDataSample {
		public WAVDataSample(BinaryReader reader, int bytesPerSample, int numChannels) {
			BytesPerSample = bytesPerSample;
			Channels = numChannels;
			Data = reader.ReadBytes(BytesPerSample * Channels);
			if(Data.Length < BytesPerSample * Channels) {
				throw new EndOfStreamException();
			}
		}
		
		public byte[] Data {
			get;
			protected set;
		}
		
		public int BytesPerSample {
			get;
			protected set;
		}
		
		public int Channels {
			get;
			protected set;
		}
		
		public uint GetSampleForChannel(int n) {
			if(n >= Channels || n < 0) {
				throw new ArgumentException();
			}
			
			byte[] barr = new byte[4];
			int barroffset = barr.Length - BytesPerSample;
			int dataoffset = BytesPerSample*n;
			for(int i = 0; i < BytesPerSample; ++i) {
				barr[i + barroffset] = Data[i + dataoffset];
			}
			return BitConverter.ToUInt32(barr, 0);
		}
	}
}

