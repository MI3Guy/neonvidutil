using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Wav2Flac
{
    public class WavReader : IDisposable
    {
        #region Fields
        private Stream input;
        private WaveFormat format;

        private int uRiffHeader;
        private int uRiffHeaderSize;

        private int uWaveHeader;

        private int uFmtHeader;
        private int uFmtHeaderSize;

        private int uDataHeader;
        private int nTotalAudioBytes;
        #endregion
		
		#region Constants
		private const ushort WAVE_FORMAT_PCM = 0x0001;
		private const ushort WAVE_FORMAT_EXTENSIBLE = 0xFFFE;
		
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
		
		private readonly Guid FormatSubtypePCM = new Guid("00000001-0000-0010-8000-00aa00389b71");
		
		#endregion
		
        #region Properties
        public Stream InputStream
        {
            get { return input; }
        }

        public int Channels
        {
            get { return format.nChannels; }
        }

        public int SampleRate
        {
            get { return format.nSamplesPerSec; }
        }

        public int BitDepth
        {
            get { return format.wBitsPerSample; }
        }

        public int Bitrate
        {
            get { return BitDepth * SampleRate * Channels; }
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(nTotalAudioBytes * 8 / Bitrate); }
        }
        #endregion

        #region Methods
        public WavReader(string input)
            : this(File.OpenRead(input))
        {
        }

        public WavReader(Stream input)
        {
            BinaryReader reader = new BinaryReader(input);

            // Ensure there are enough bytes to read
            //if (input.Length < Marshal.SizeOf(typeof(WaveFormat)))
            //    throw new ApplicationException("Input stream is too short (< wave header size)!");

            this.input = input;

            // Ensure this is a correct WAVE file to avoid unnecessary reading, processing, etc.
            uRiffHeader = reader.ReadInt32();
            uRiffHeaderSize = reader.ReadInt32();
            uWaveHeader = reader.ReadInt32();

            if (uRiffHeader != 0x46464952 /* RIFF */ ||
                uWaveHeader != 0x45564157 /* WAVE */)
                throw new ApplicationException("Invalid WAVE header!");

            // Read all WAVE chunks
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int type = reader.ReadInt32();
                int size = reader.ReadInt32();

                long last = reader.BaseStream.Position;

                switch (type)
                {
                    case 0x61746164: /* data */
                        uDataHeader = type;
                        nTotalAudioBytes = size;
                        break;

                    case 0x20746d66: /* fmt  */
                        uFmtHeader = type;
                        uFmtHeaderSize = size;

                        format.wFormatTag = reader.ReadUInt16();
                        format.nChannels = reader.ReadInt16();
                        format.nSamplesPerSec = reader.ReadInt32();
                        format.nAvgBytesPerSec = reader.ReadInt32();
                        format.nBlockAlign = reader.ReadInt16();
                        format.wBitsPerSample = reader.ReadInt16();
                        format.cbSize = reader.ReadInt16();
						if(format.wFormatTag == WAVE_FORMAT_EXTENSIBLE) {
							format.wValidBitsPerSample = reader.ReadUInt16();
							format.dwChannelMask = reader.ReadUInt32();
							format.SubFormat = new Guid(reader.ReadBytes(16));
						}
                        break;
                }

                if (uDataHeader == 0) // Do not skip the 'data' chunk size
                    reader.BaseStream.Position = last + size;
                else
                    break;
            }
			
            // Ensure that samples are integers (e.g. not floating-point numbers)
            if (format.wFormatTag != WAVE_FORMAT_PCM && (format.wFormatTag != WAVE_FORMAT_EXTENSIBLE && format.SubFormat == FormatSubtypePCM)) // 1 = PCM 2 = Float
                throw new ApplicationException("Format tag " + format.wFormatTag + " is not supported!");
			
			// TODO: Check channel mapping
			
			
            // Ensure that samples are 16 or 24-bit
            if (format.wBitsPerSample != 16 && format.wBitsPerSample != 24)
                throw new ApplicationException(format.wBitsPerSample + " bits per sample is not supported by FLAC!");
        }

        public void Dispose()
        {
            if (this.input != null)
                this.input.Dispose();

            this.input = null;
        }
        #endregion
    }

    #region Native
    struct WaveFormat
    {
        public ushort wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
		public ushort wValidBitsPerSample;
		public uint dwChannelMask;
		public Guid SubFormat;
    }
    #endregion
}
