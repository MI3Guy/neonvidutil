using System;
using System.IO;
using System.Text;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVData {
		public WAVData(Stream stream) {
			this.stream = stream;
			reader = new BinaryReader(stream);
			
			wavRiff = new WAVRIFF(reader);
			
			if(wavRiff.ckID != "RIFF" || wavRiff.WAVEID != "WAVE") {
				throw new ApplicationException("Not a valid WAV file");
			}
		}
		
		public DataChunk ReadDataChunk() {
			while(reader.BaseStream.Position < reader.BaseStream.Length) {
				string ckID = Encoding.ASCII.GetString(reader.ReadBytes(4));
				switch(ckID) {
					case ChunkIdFormat:
						formatChunk = new FormatChunk(reader);
						
						if(formatChunk.wFormatTag != FormatTag.PCM && (formatChunk.wFormatTag != FormatTag.EXTENSIBLE && formatChunk.SubFormat != FormatSubtypePCM)) {
							throw new ApplicationException("WAV does not contain PCM data.");
						}
						break;
						
					case ChunkIdData:
						return new DataChunk(reader, formatChunk);
						
					default:
						uint size = reader.ReadUInt32();
						reader.BaseStream.Position += size;
						break;
				}
			}
			return null;
		}
		
		private Stream stream;
		private BinaryReader reader;
		
		WAVRIFF wavRiff;
		FormatChunk formatChunk;
		
		
		
		public enum FormatTag : ushort {
			PCM			= 0x0001,
			IEEE_FLOAT	= 0x0003,
			ALAW		= 0x0006,
			MULAW		= 0x0007,
			EXTENSIBLE	= 0xFFFE
		}
			
		private enum Speaker {
			None 				=	0x0,
			FrontLeft			=	0x1,
			FrontRight			=	0x2,
			FrontCenter			=	0x4,
			LowFrequency		=	0x8,
			BackLeft			=	0x10,
			BackRight			=	0x20,
			FrontLeftOfCenter	=	0x40,
			FrontRightOfCenter	=	0x80,
			BackCenter			=	0x100,
			SideLeft			=	0x200,
			SideRight			=	0x400,
			TopCenter			=	0x800,
			TopFrontLeft		=	0x1000,
			TopFrontCenter		=	0x2000,
			TopFrontRight		=	0x4000,
			TopBackLeft			=	0x8000,
			TopBackCenter		=	0x10000,
			TopBackRight		=	0x20000,
			
			Mono				=	FrontCenter,
			Stereo				=	FrontLeft | FrontRight,
			Quad				=	FrontLeft | FrontRight | BackLeft | BackRight,
			Surround			=	FrontLeft | FrontRight | FrontCenter | BackCenter,
			FivePointOne		=	FrontLeft | FrontRight | FrontCenter | LowFrequency | BackLeft | BackRight,
			SevenPointOne		=	FrontLeft | FrontRight | FrontCenter | LowFrequency | BackLeft | BackRight | FrontLeftOfCenter | FrontRightOfCenter
		}
		
		private static readonly Guid FormatSubtypePCM = new Guid("00000001-0000-0010-8000-00aa00389b71");
		
		private const string ChunkIdRiff = "RIFF";
		private const string ChunkIdWave = "WAVE";
		private const string ChunkIdFormat = "fmt ";
		private const string ChunkIdData = "data";
		
		public class WAVRIFF {
			public WAVRIFF(BinaryReader reader) {
				ckID = Encoding.ASCII.GetString(reader.ReadBytes(4));
				size = reader.ReadUInt32();
				WAVEID = Encoding.ASCII.GetString(reader.ReadBytes(4));
			}
			public string ckID;
			public uint size;
			public string WAVEID;
		}
		
		public class FormatChunk {
			public FormatChunk(BinaryReader reader) {
				uint cksize = reader.ReadUInt32();
				long prev = reader.BaseStream.Position;
				
				wFormatTag = (FormatTag)reader.ReadUInt16();
				nChannels = reader.ReadUInt16();
				nSamplesPerSec = reader.ReadUInt32();
				nAvgBytesPerSec = reader.ReadUInt32();
				nBlockAlign = reader.ReadUInt16();
				wBitsPerSample = reader.ReadUInt16();
				if(wFormatTag == WAVData.FormatTag.EXTENSIBLE) {
					cbSize = reader.ReadUInt16();
					if(cbSize > 0) {
						wValidBitsPerSample = reader.ReadUInt16();
						dwChannelMask = reader.ReadUInt32();
						SubFormat = new Guid(reader.ReadBytes(16));
					}
				}
				reader.BaseStream.Position = prev + cksize;
			}
			public FormatTag wFormatTag;
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
		
		
		public class DataChunk {
			public DataChunk(BinaryReader reader, FormatChunk formatChunk) {
				this.reader = reader;
				this.formatChunk = formatChunk;
				
				uint cksize = reader.ReadUInt32();
				
				bytesPerSample = formatChunk.wBitsPerSample / 8;
				numChannels = formatChunk.nChannels;
			}
			
			private BinaryReader reader;
			private FormatChunk formatChunk;
			private uint bytesPerSample;
			private ulong numChannels;
			
			public byte[] ReadSample() {
				
			}
		}
		
		 
	}
}

