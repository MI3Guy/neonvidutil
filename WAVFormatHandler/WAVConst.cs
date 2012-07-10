using System;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public static class WAVConst {
		public enum FormatTag : ushort {
			PCM			= 0x0001,
			IEEE_FLOAT	= 0x0003,
			ALAW		= 0x0006,
			MULAW		= 0x0007,
			EXTENSIBLE	= 0xFFFE
		}
			
		public enum Speaker {
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
		
		public static readonly Guid FormatSubtypePCM = new Guid("00000001-0000-0010-8000-00aa00389b71");
		
		public const string ChunkIdRiff = "RIFF";
		public const string ChunkIdWave = "WAVE";
		public const string ChunkIdFormat = "fmt ";
		public const string ChunkIdData = "data";
	}
}

