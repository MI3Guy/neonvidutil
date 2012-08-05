using System;
using System.IO;

namespace WAVSharp {
	public class WAVDataSample {
		public WAVDataSample(BinaryReader reader, int bytesPerSample, int bitsPerSample, int numChannels) {
			BytesPerSample = bytesPerSample;
			Channels = numChannels;
			Data = reader.ReadBytes(BytesPerSample * Channels);
			if(Data.Length < BytesPerSample * Channels) {
				throw new EndOfStreamException();
			}
		}
		
		public WAVDataSample() {
			
		}
		
		public WAVDataSample(WAVDataSample other, int bits, byte[] mask) {
			BytesPerSample = (int)Math.Ceiling((double)bits / 8.0);
			BitsPerSample = bits;
			Channels = other.Channels;
			
			int end = BytesPerSample;
			if(other.BytesPerSample < BytesPerSample) {
				end = other.BytesPerSample;
			}
			
			Data = new byte[BytesPerSample * Channels];
			for(int i = 0; i < Channels; ++i) {
				for(int j = 1; j <= end; ++j) {
					Data[(BytesPerSample - j) + i*BytesPerSample] = (byte)(other.Data[(other.BytesPerSample - j) + i*other.BytesPerSample] & mask[BytesPerSample - j]);
				}
			}
		}
		
		public static byte[] FindMaskForBits(int bits) {
			int bytes = (int)Math.Ceiling((double)bits / 8.0);
			byte[] mask = new byte[bytes];
			int bitsRemaining = 8*bytes - bits;
			for(int i = bytes - 1; i >= 0; --i) {
				mask[i] = 0xFF;
				if(bitsRemaining > 0) {
					mask[i] >>= bitsRemaining;
					bitsRemaining -= 8;
				}
			}
			
			return mask;
		}
		
		public byte[] Data {
			get;
			set;
		}
		
		public int BytesPerSample {
			get;
			set;
		}
		
		public int BitsPerSample {
			get;
			set;
		}
		
		public int Channels {
			get;
			set;
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
		
		public int GetSampleForChannel2(int n) {
			if(n >= Channels || n < 0) {
				throw new ArgumentException();
			}
			
			int dataoffset = BytesPerSample*n;
			switch(BytesPerSample) {
				case 1:
					return (sbyte)Data[dataoffset];
				case 2:
					return BitConverter.ToInt16(Data, dataoffset);
				case 3:{
					byte[] sample = new byte[sizeof(int)];
					for(int i = 0; i < BytesPerSample; ++i) {
						sample[i] = Data[i + dataoffset];
					}
					bool positive = (sample[2] & 0x80) == 0;
					if(!positive) { // Convert negative 24 bit number to negative 32 bit number
						sample[3] = 0xFF;
					}
					return BitConverter.ToInt32(sample, 0);
					}
				case 4:
					return BitConverter.ToInt32(Data, dataoffset);
				default:
					throw new ApplicationException("Unsupported Bit Depth");
			}
		}
		
		public void WriteTo(BinaryWriter writer) {
			writer.Write(Data);
		}
	}
}

