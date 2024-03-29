using System;
using System.Runtime.InteropServices;

namespace LibSndFileSharp {
	internal static class LibSndFile {
		
		[Flags]
		public enum SndFileFormat : int
		{	/* Major formats. */
			SF_FORMAT_WAV			= 0x010000,		/* Microsoft WAV format (little endian default). */
			SF_FORMAT_AIFF			= 0x020000,		/* Apple/SGI AIFF format (big endian). */
			SF_FORMAT_AU			= 0x030000,		/* Sun/NeXT AU format (big endian). */
			SF_FORMAT_RAW			= 0x040000,		/* RAW PCM data. */
			SF_FORMAT_PAF			= 0x050000,		/* Ensoniq PARIS file format. */
			SF_FORMAT_SVX			= 0x060000,		/* Amiga IFF / SVX8 / SV16 format. */
			SF_FORMAT_NIST			= 0x070000,		/* Sphere NIST format. */
			SF_FORMAT_VOC			= 0x080000,		/* VOC files. */
			SF_FORMAT_IRCAM			= 0x0A0000,		/* Berkeley/IRCAM/CARL */
			SF_FORMAT_W64			= 0x0B0000,		/* Sonic Foundry's 64 bit RIFF/WAV */
			SF_FORMAT_MAT4			= 0x0C0000,		/* Matlab (tm) V4.2 / GNU Octave 2.0 */
			SF_FORMAT_MAT5			= 0x0D0000,		/* Matlab (tm) V5.0 / GNU Octave 2.1 */
			SF_FORMAT_PVF			= 0x0E0000,		/* Portable Voice Format */
			SF_FORMAT_XI			= 0x0F0000,		/* Fasttracker 2 Extended Instrument */
			SF_FORMAT_HTK			= 0x100000,		/* HMM Tool Kit format */
			SF_FORMAT_SDS			= 0x110000,		/* Midi Sample Dump Standard */
			SF_FORMAT_AVR			= 0x120000,		/* Audio Visual Research */
			SF_FORMAT_WAVEX			= 0x130000,		/* MS WAVE with WAVEFORMATEX */
			SF_FORMAT_SD2			= 0x160000,		/* Sound Designer 2 */
			SF_FORMAT_FLAC			= 0x170000,		/* FLAC lossless file format */
			SF_FORMAT_CAF			= 0x180000,		/* Core Audio File format */
			SF_FORMAT_WVE			= 0x190000,		/* Psion WVE format */
			SF_FORMAT_OGG			= 0x200000,		/* Xiph OGG container */
			SF_FORMAT_MPC2K			= 0x210000,		/* Akai MPC 2000 sampler */
			SF_FORMAT_RF64			= 0x220000,		/* RF64 WAV file */
		
			/* Subtypes from here on. */
		
			SF_FORMAT_PCM_S8		= 0x0001,		/* Signed 8 bit data */
			SF_FORMAT_PCM_16		= 0x0002,		/* Signed 16 bit data */
			SF_FORMAT_PCM_24		= 0x0003,		/* Signed 24 bit data */
			SF_FORMAT_PCM_32		= 0x0004,		/* Signed 32 bit data */
		
			SF_FORMAT_PCM_U8		= 0x0005,		/* Unsigned 8 bit data (WAV and RAW only) */
		
			SF_FORMAT_FLOAT			= 0x0006,		/* 32 bit float data */
			SF_FORMAT_DOUBLE		= 0x0007,		/* 64 bit float data */
		
			SF_FORMAT_ULAW			= 0x0010,		/* U-Law encoded. */
			SF_FORMAT_ALAW			= 0x0011,		/* A-Law encoded. */
			SF_FORMAT_IMA_ADPCM		= 0x0012,		/* IMA ADPCM. */
			SF_FORMAT_MS_ADPCM		= 0x0013,		/* Microsoft ADPCM. */
		
			SF_FORMAT_GSM610		= 0x0020,		/* GSM 6.10 encoding. */
			SF_FORMAT_VOX_ADPCM		= 0x0021,		/* OKI / Dialogix ADPCM */
		
			SF_FORMAT_G721_32		= 0x0030,		/* 32kbs G721 ADPCM encoding. */
			SF_FORMAT_G723_24		= 0x0031,		/* 24kbs G723 ADPCM encoding. */
			SF_FORMAT_G723_40		= 0x0032,		/* 40kbs G723 ADPCM encoding. */
		
			SF_FORMAT_DWVW_12		= 0x0040, 		/* 12 bit Delta Width Variable Word encoding. */
			SF_FORMAT_DWVW_16		= 0x0041, 		/* 16 bit Delta Width Variable Word encoding. */
			SF_FORMAT_DWVW_24		= 0x0042, 		/* 24 bit Delta Width Variable Word encoding. */
			SF_FORMAT_DWVW_N		= 0x0043, 		/* N bit Delta Width Variable Word encoding. */
		
			SF_FORMAT_DPCM_8		= 0x0050,		/* 8 bit differential PCM (XI only) */
			SF_FORMAT_DPCM_16		= 0x0051,		/* 16 bit differential PCM (XI only) */
		
			SF_FORMAT_VORBIS		= 0x0060,		/* Xiph Vorbis encoding. */
		
			/* Endian-ness options. */
		
			SF_ENDIAN_FILE			= 0x00000000,	/* Default file endian-ness. */
			SF_ENDIAN_LITTLE		= 0x10000000,	/* Force little endian-ness. */
			SF_ENDIAN_BIG			= 0x20000000,	/* Force big endian-ness. */
			SF_ENDIAN_CPU			= 0x30000000,	/* Force CPU endian-ness. */
		
			SF_FORMAT_SUBMASK		= 0x0000FFFF,
			SF_FORMAT_TYPEMASK		= 0x0FFF0000,
			SF_FORMAT_ENDMASK		= 0x30000000
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct SfInfo {
			public long frames;
			public int samplerate;
			public int channels;
			public int format;
			public int sections;
			public int seekable;
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct SfVirtualIO {
			public delegate long sf_vio_get_filelen(IntPtr user_data);
			public delegate long sf_vio_seek(long offset, int whence, IntPtr user_data);
			public delegate long sf_vio_read(IntPtr ptr, long count, IntPtr user_data);
			public delegate long sf_vio_write(IntPtr ptr, long count, IntPtr user_data);
			public delegate long sf_vio_tell(IntPtr user_data);
			
			public sf_vio_get_filelen get_filelen;
			public sf_vio_seek seek;
			public sf_vio_read read;
			public sf_vio_write write;
			public sf_vio_tell tell;
		}
		
		public const int SFM_READ = 0x10;
		public const int SFM_WRITE = 0x20;
		public const int SFM_RDWR = 0x30;
		
		[DllImport("libsndfile")]
		public static extern IntPtr sf_open_virtual(ref SfVirtualIO sfvirtual, int mode, ref SfInfo sfinfo, IntPtr user_data);
		
		[DllImport("libsndfile")]
		public static extern int sf_command(IntPtr sndfile, int cmd, IntPtr data, int datasize);
		
		[DllImport("libsndfile")]
		public static extern long sf_readf_int(IntPtr sndfile, IntPtr ptr, long frames);
		
		[DllImport("libsndfile")]
		public static extern int sf_close(IntPtr sndfile);
		
	}
}

