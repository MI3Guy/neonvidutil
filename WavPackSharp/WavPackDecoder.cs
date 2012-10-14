using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WAVSharp;

namespace JVL.Audio.WavPackWrapper
{
    public class WavPackException : Exception { }

    public class OpenFailedException : WavPackException
    {
        public String Error { get; private set; }

        public OpenFailedException(String error)
        {
            Error = error;
        }

        public override String Message
        {
            get { return String.Format("Open error: {0}.", Error); }
        }
    }

    public class UnexpectedFormatException : Exception
    {
        public String Error { get; private set; }

        public UnexpectedFormatException(String error)
        {
            Error = error;
        }

        public override String Message
        {
            get { return String.Format("Unexpected format: {0}.", Error); }
        }
    }

    public class SeekFailedException : Exception
    {
        public UInt32 Sample { get; private set; }

        public SeekFailedException(UInt32 sample)
        {
            Sample = sample;
        }

        public override String Message
        {
            get { return String.Format("Seek to sample {0} failed.", Sample); }
        }
    }

    /// <summary>
    /// Wrapper for wavpackdll.dll, to read WavPack files (.wv and their optional .wvc) as wave information and data.
    /// <br></br>
    /// Written by Jean Van Laethem.
    /// <br></br>
    /// All exceptions possibly thrown derive from WavPackException.
    /// <br></br>
    /// <code>
    /// using (WavPack wavPack = new WavPack(@"E:\file.wv"))
    /// {
    ///     using (WavWriter writer = new WavWriter(new FileStream(@"E:\file.Wav", FileMode.Create), wavPack.WaveFormat))
    ///     {
    ///         Byte[] buffer = new Byte[0x10000];
    ///         for (Int32 bytesRead; (0 != (bytesRead = wavPack.Read(buffer))); )
    ///         {
    ///             writer.Write(buffer, 0, bytesRead);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </summary>
    public class WavPackDecoder : IDisposable
    {
        private                         IntPtr      m_WavPackContext;
        private             readonly    WAVFormatChunk  m_WaveFormat;
        private             readonly    Boolean     m_WavPackIsFloat;
        private             readonly    Int64       m_WaveBytesPerSample;
        private             const       Int32       WavPackBytesPerSample   = 4;
        private             readonly    Int64       m_WavPackSampleSize;
        private                         Byte[]      m_WavPackBuffer         = new Byte[0];
        private                         Boolean     m_SeekFailed;

        /// <summary>
        /// Return the WavPack library version. As of this writing this is "4.60.1".
        /// </summary>
        public    static              Version     LibraryVersion
        {
            get
            {
                Int32 version = (Int32)WavpackGetLibraryVersion();
                return new Version( (version >> 16) & 0xFF,
                                    (version >>  8) & 0xFF,
                                     version        & 0xFF);
            }
        }

        /// <summary>
        /// Return the mode of the WavPack file.
        /// </summary>
        public                        FileMode    Mode            { get { return WavpackGetMode(m_WavPackContext);                    }   }
        /// <summary>
        /// Return the total size of the WavPack file in bytes.
        /// </summary>
        public                        UInt32      Size            { get { return WavpackGetFileSize(m_WavPackContext);                }   }
        /// <summary>
        /// Get total number of samples contained in the WavPack file.
        /// </summary>
        public                        UInt32      NumSamples      { get { return WavpackGetNumSamples(m_WavPackContext);              }   }
        /// <summary>
        /// Return the wave header of the WavPack file.
        /// </summary>
        public                        WAVFormatChunk  WaveFormat      { get { return m_WaveFormat;                                        }   }
        /// <summary>
        /// Return the total size of the wave data in bytes.
        /// </summary>
        public                        UInt32      WaveDataSize    { get { return (UInt32)(NumSamples * WaveFormat.nBlockAlign);        }   }
        /// <summary>
        /// This function returns the version number of the WavPack program
        /// (or library) that created the open file. Currently, this can be 1 to 4.
        /// </summary>
        /// <returns></returns>
        public                        Version     Version         { get { return new Version(WavpackGetVersion(m_WavPackContext), 0); }   }

        /// <summary>
        /// Create a WavPack object and open fileName for reading and seeking.
        /// <br></br>
        /// Files with floating point data are read as 2 bytes per sample integer data.
        /// </summary>
        /// <param name="fileName"></param>
        /// </param>
        /// <exception cref="OpenFailedException">Thrown when the WavPack library fails to open the file "fileName" ;o)</exception>
        /// <exception cref="UnexpectedFormatException">Thrown when the file cannot be unpacked because its format is not supported.</exception>
        public WavPackDecoder(Stream file)
            : this(file, false)
        {

        }
		
		WavPackStreamReader reader;

        /// <summary>
        /// Create a WavPack object and open fileName for reading and seeking.
        /// <br></br>
        /// You can choose to read floating point files as 2 or 4 bytes per sample integer data.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="makeFloat4BytesPerSample">
        /// Set this flag to convert floating point data to 4 bytes per sample data.
        /// Clear this flag to convert floating point data to 2 bytes per sample data.
        /// </param>
        /// <exception cref="OpenFailedException">Thrown when the WavPack library fails to open the file "fileName" ;o)</exception>
        /// <exception cref="UnexpectedFormatException">Thrown when the file cannot be unpacked because its format is not supported.</exception>
        public WavPackDecoder(Stream file, Boolean makeFloat4BytesPerSample)
        {
			reader = new WavPackStreamReader() {
				read_bytes = new WavPackStreamReader.read_bytes_type(CallbackReadBytes),
				get_pos = new WavPackStreamReader.get_pos_type(CallbackGetPos),
				set_pos_abs = new WavPackStreamReader.set_pos_abs_type(CallbackSetPosAbs),
				set_pos_rel = new WavPackStreamReader.set_pos_rel_type(CallbackSetPosRel),
				push_back_byte = new WavPackStreamReader.push_back_byte_type(CallbackPushBackByte),
				can_seek = new WavPackStreamReader.can_seek_type(CallbackCanSeek),
				write_bytes = new WavPackStreamReader.write_bytes_type(CallbackWriteBytes)
			};
			
			pushedBackBytes = new Stack<byte>();
			readStream = file;
			
            StringBuilder
                error                                       = new StringBuilder(1024);
			
			/*
			 * 			public delegate int read_bytes_type(IntPtr id, IntPtr data, int bcount);
			public delegate uint get_pos_type(IntPtr id);
			public delegate int set_pos_abs_type(IntPtr id, uint pos);
			public delegate int set_pos_rel_type(IntPtr id, int delta, int mode);
			public delegate int push_back_byte_type(IntPtr id, int c);
			public delegate uint get_length_type(IntPtr id);
			public delegate int can_seek_type(IntPtr id);
			 * */
			
                m_WavPackContext                            = WavpackOpenFileInputEx(ref reader, IntPtr.Zero, IntPtr.Zero, error, Open.Normalize | Open.WVC | Open.Max2Ch, 0);
            if (IntPtr.Zero == m_WavPackContext)
            {
                throw new OpenFailedException(error.ToString());
            }
            try
            {
                // init wave format header
                m_WavPackIsFloat                            = (0 != (FileMode.Float & Mode));
                m_WaveBytesPerSample                        = m_WavPackIsFloat  ? makeFloat4BytesPerSample  ? 4
                                                                                                            : 2
                                                                                : WavpackGetBytesPerSample(m_WavPackContext);
                m_WaveFormat                                = new WAVFormatChunk(
						channels: (ushort)WavpackGetNumChannels   (m_WavPackContext),
						samplesPerSec: (uint)   WavpackGetSampleRate    (m_WavPackContext),
						bitsPerSample: (ushort)  (m_WaveBytesPerSample * 8),
						validBitsPerSample: (ushort) WavpackGetBitsPerSample(m_WavPackContext),
						channelMask: (uint)WavpackGetChannelMask(m_WavPackContext)
					);/*
                                                                    {
                                                                        wFormatTag			=			WAVConst.FormatTag.EXTENSIBLE,
                                                                        nChannels            = (ushort)WavpackGetNumChannels   (m_WavPackContext),
                                                                        nSamplesPerSec    = (uint)   WavpackGetSampleRate    (m_WavPackContext),
                                                                        wBitsPerSample       = (ushort)  (m_WaveBytesPerSample * 8),
					// TODO: Add additional information.
                                                                    };
                m_WaveFormat.nBlockAlign             = (ushort)(m_WaveFormat.nChannels * m_WaveBytesPerSample);
                m_WaveFormat.nAvgBytesPerSec  = m_WaveFormat.nSamplesPerSec * m_WaveFormat.nBlockAlign;
                m_WavPackSampleSize                         = m_WaveFormat.nChannels * WavPackBytesPerSample;*/

                // stop if format is unexpected
                if (0 > WavpackGetNumSamples(m_WavPackContext))
                {
                    throw new UnexpectedFormatException("Unknown number of samples.");
                }
            }
            catch (WavPackException)
            {
                Close();
                throw;
            }
        }
		
		private Stack<byte> pushedBackBytes;
		private Stream readStream;

        /// <summary>
        /// Close the current instance.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Seek to the specifed sample index. Note that files
        /// generated with version 4.0 or newer will seek almost immediately. Older files
        /// can take quite long if required to seek through unplayed portions of the file,
        /// but will create a seek map so that reverse seeks (or forward seeks to already
        /// scanned areas) will be very fast.
        /// <br></br>
        /// If a SeekFailedException is raised, the file should not be accessed again
        /// (other than to close it); this is a fatal error.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="SeekFailedException">
        /// Thrown when seeking fails ;o) The file should not be accessed again
        /// (other than to close it); this is a fatal error.".
        /// </exception>
        public void SeekSample(UInt32 sample)
        {
            if (!m_SeekFailed)
            {
                if (!WavpackSeekSample(m_WavPackContext, sample))
                {
                    m_SeekFailed = true;
                    throw new SeekFailedException(sample);
                }
            }
        }

        /// <summary>
        /// Read up to buffer.Length bytes from the current file position.
        /// The actual number of bytes read is returned.
        /// <br></br>
        /// If the number of bytes per sample is not 4, no more bytes than
        /// WaveFormat.AverageBytesPerSecond are read.
        /// <br></br>
        /// If all samples have been read then 0 will be returned.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>The number of bytes read.</returns>
        public Int32 Read(Byte[] buffer)
        {
            if (m_SeekFailed)
            {
                return 0;
            }

            // if wavpack unpacks as 4-byte integer
            if ((WavPackBytesPerSample == m_WaveBytesPerSample)
             && (!m_WavPackIsFloat))
            {
                // unpack directly in destination buffer
                UInt32  sampleCount         = (UInt32)  (buffer.Length  / m_WavPackSampleSize);
                        sampleCount         = WavpackUnpackSamples(m_WavPackContext, buffer, sampleCount);
                return                        (Int32)   (sampleCount    * m_WavPackSampleSize);
            }
            else
            {
                // unpack 4-byte samples in temp buffer and copy/convert to m_BytesPerSample samples in destination buffer
                // limit wave buffer size to 1 sec
                Int32   maxWaveBufferLength = Math.Min((int)m_WaveFormat.nAvgBytesPerSec, buffer.Length);
                Int32   wavPackBufferLength = (Int32)(maxWaveBufferLength * WavPackBytesPerSample / m_WaveBytesPerSample);
                // enlarge temp buffer if needed
                if (wavPackBufferLength > m_WavPackBuffer.Length)
                {
                        m_WavPackBuffer     = new Byte[wavPackBufferLength];
                }
                UInt32  sampleCount         = (UInt32)  (wavPackBufferLength    / m_WavPackSampleSize);
                        sampleCount         = WavpackUnpackSamples(m_WavPackContext, m_WavPackBuffer, sampleCount);
                Int32   count               = (Int32)   (sampleCount            * m_WavPackSampleSize);
                Int32   dst                 = 0;
                if (m_WavPackIsFloat)
                {
                    switch (m_WaveBytesPerSample)
                    {
                        case 2:
                            {
                                for (Int32 src = 0; src < count; src += WavPackBytesPerSample)
                                {
                                    Single  single          = BitConverter.ToSingle(m_WavPackBuffer, src);
                                    Int16   int16           =   (single >=  1.0)    ? Int16.MaxValue :
                                                                (single <= -1.0)    ? Int16.MinValue
                                                                                    : (Int16)Math.Floor(single * Int16.MinValue);
                                    Byte[]  bytes           = BitConverter.GetBytes(int16);
                                            buffer[dst++]   = bytes[0];
                                            buffer[dst++]   = bytes[1];
                                }
                                break;
                            }
                        case 4:
                            {
                                for (Int32 src = 0; src < count; src += WavPackBytesPerSample)
                                {
                                    Single  single          = BitConverter.ToSingle(m_WavPackBuffer, src);
                                    Int32   int32           =   (single >=  1.0)    ? Int32.MaxValue :
                                                                (single <= -1.0)    ? Int32.MinValue
                                                                                    : (Int32)Math.Floor(single * Int32.MinValue);
                                    Byte[]  bytes           = BitConverter.GetBytes(int32);
                                            buffer[dst++]   = bytes[0];
                                            buffer[dst++]   = bytes[1];
                                            buffer[dst++]   = bytes[2];
                                            buffer[dst++]   = bytes[3];
                                }
                                break;
                            }
                        default:
                            {
                                throw new UnexpectedFormatException(String.Format("Bytes per sample is {0}.", m_WaveBytesPerSample));
                            }
                    }
                }
                else
                {
                    switch (m_WaveBytesPerSample)
                    {
                        case 1:
                            {
                                for (Int32 src = 0; src < count; src += WavPackBytesPerSample)
                                {
                                    buffer[dst++]   = (Byte)(m_WavPackBuffer[src] + 128);
                                }
                                break;
                            }
                        case 2:
                            {
                                for (Int32 src = 0; src < count; src += WavPackBytesPerSample)
                                {
                                    buffer[dst++]   = m_WavPackBuffer[src];
                                    buffer[dst++]   = m_WavPackBuffer[src + 1];
                                }
                                break;
                            }
                        case 3:
                            {
                                for (Int32 src = 0; src < count; src += WavPackBytesPerSample)
                                {
                                    buffer[dst++]   = m_WavPackBuffer[src];
                                    buffer[dst++]   = m_WavPackBuffer[src + 1];
                                    buffer[dst++]   = m_WavPackBuffer[src + 2];
                                }
                                break;
                            }
                        default:
                            {
                                throw new UnexpectedFormatException(String.Format("Bytes per sample is {0}.", m_WaveBytesPerSample));
                            }
                    }
                }
                return dst;
            }
        }

        [Flags]
        public enum Open
        {
            /// <summary>
            /// Attempt to open and read a corresponding "correction" file along with the
            /// standard WavPack file. No error is generated if this fails (although it is
            /// possible to find out which decoding mode is actually being used). NOTE THAT
            /// IF THIS FLAG IS NOT SET THEN LOSSY DECODING WILL OCCUR EVEN WHEN A CORRECTION
            /// FILE IS AVAILABLE, THEREFORE THIS FLAG SHOULD NORMALLY BE SET!
            /// </summary>
            WVC         = 0x1,
            /// <summary>
            /// Attempt to read any ID3v1 or APEv2 tags appended to the end of the file. This
            /// obviously requires a seekable file to succeed.
            /// </summary>
            Tags        = 0x2,
            /// <summary>
            /// Normally all the information required to decode the file will be available from
            /// native WavPack information. However, if the purpose is to restore the actual
            /// .wav file verbatum (or the RIFF header is needed for some other reason) then
            /// this flag should be set. After opening the file, WavpackGetWrapperData() can be
            /// used to obtain the actual RIFF header (which the caller must parse if desired).
            /// Note that some WavPack files might not contain RIFF headers.
            /// </summary>
            Wrapper     = 0x4,
            /// <summary>
            /// This allows multichannel WavPack files to be opened with only one stream, which
            /// usually incorporates the front left and front right channels. This is provided
            /// to allow decoders that can only handle 2 channels to at least provide
            /// "something" when playing multichannel. It would be nice if this could downmix
            /// the multichannel audio to stereo instead of just using two channels, but that
            /// exercise is left for the student.  :)
            /// </summary>
            Max2Ch      = 0x8,
            /// <summary>
            /// Most floating point audio data is normalized to the range of +/-1.0
            ///(especially the floating point data in Microsoft .wav files) and this is what
            ///WavPack normally stores. However, WavPack is a lossless compressor, which means
            ///that is should (and does) work with floating point data that is normalized to
            ///some other range. However, if an application simply wants to play the audio,
            ///then it probably wants the data normalized to the same range regardless of the
            ///source. This flag is provided to accomplish that, and when set simply tells the
            ///decoder to provide floating point data normalized to +/-1.0 even if the source
            ///had some other range. The "norm_offset" parameter can be used to select a
            ///different range if that is desired.
            ///
            /// Keep in mind that floating point audio (unlike integer audio) is not required
            /// to stay within its normalized limits. In fact, it can be argued that this is
            /// one of the advantages of floating point audio (i.e. no danger of clipping)!
            /// However, when this is decoded for playback (which, of course, must eventually
            /// involve a conversion back to the integer domain) it is important to consider
            /// this possibility and (at a minimum) perform hard clipping.</summary>
            Normalize   = 0x10,
            /// <summary>
            /// This is essentially a "raw" or "blind" mode where the library will simply
            /// decode any blocks fed it through the reader callback (or file), regardless of
            /// where those blocks came from in a stream. The only requirement is that complete
            /// WavPack blocks are fed to the decoder (and this will require multiple blocks in
            /// multichannel mode) and that complete blocks are decoded (even if all samples
            /// are not actually required). All the blocks must contain the same number of
            /// channels and bit resolution, and the correction data must be either present or
            /// not. All other parameters may change from block to block (like lossy/lossless).
            /// Obviously, in this mode any seeking must be performed by the application (and
            /// again, decoding must start at the beginning of the block containing the seek
            /// sample).
            /// "streaming" mode blindly unpacks blocks w/o regard to header file position info</summary>
            Streaming   = 0x20,
            /// <summary>
            /// Open the file in read/write mode to allow editing of any APEv2 tags present, or
            /// appending of a new APEv2 tag. Of course the file must have write permission.</summary>
            EditTags    = 0x40,
        }

        [Flags]
        public enum FileMode
        {
            /// <summary>A .wvc file has been found and will be used for lossless decoding.</summary>
            WVC         = 0x1,
            /// <summary>The file decoding is lossless (either pure or hybrid).</summary>
            LossLess    = 0x2,
            /// <summary>The file is in hybrid mode (may be either lossy or lossless).</summary>
            Hybrid      = 0x4,
            /// <summary>The audio data is 32-bit ieee floating point.</summary>
            Float       = 0x8,
            /// <summary>The file conatins a valid ID3v1 or APEv2 tag (OPEN_TAGS must be set above to get this status).</summary>
            ValidTag    = 0x10,
            /// <summary>The file was originally created in "high" mode (this is really only useful for reporting to the user)</summary>
            High        = 0x20,
            /// <summary>The file was originally created in "fast" mode (this is really only useful for reporting to the user)</summary>
            Fast        = 0x40,
            /// <summary>
            /// The file was originally created with the  "extra" mode (this is really only
            /// useful for reporting to the user). The MODE_XMODE below can sometimes allow
            /// determination of the exact extra mode level.
            /// </summary>
            Extra       = 0x80,
            /// <summary>
            /// The file contains a valid APEv2 tag (OPEN_TAGS must be set in the "open" call
            /// for this to be true). Note that only APEv2 tags can be edited by the library.
            /// If a file that has an ID3v1 tag needs to be edited then it must either be done
            /// with another library or it must be converted (field by field) into a APEv2 tag
            /// (see the wvgain.c program for an example of this).
            /// </summary>
            APETag      = 0x100,
            /// <summary>The file was created as a "self-extracting" executable (this is really only useful for reporting to the user).</summary>
            SFX         = 0x200,
            /// <summary>The file was created in the "very high" mode (or in the "high" mode prior to 4.40).</summary>
            VeryHigh    = 0x400,
            /// <summary>The file contains an MD5 checksum.</summary>
            MD5         = 0x800,
            /// <summary>
            /// If the MODE_EXTRA bit above is set, this 3-bit field can sometimes allow the
            /// determination of the exact extra mode parameter specified by the user if the
            /// file was encoded with version 4.50 or later. If these three bits are zero
            /// then the extra mode level is unknown, otherwise is represents the extra mode
            /// level from 1-6.
            /// </summary>
            XMode       = 0x7000,
            /// <summary>The hybrid file was encoded with the dynamic noise shaping feature which was introduced in the 4.50 version of WavPack.</summary>
            DNS         = 0x8000,
        }
		
		[StructLayout(LayoutKind.Sequential)]
		private struct WavPackStreamReader {
			public delegate int read_bytes_type(IntPtr id, IntPtr data, int bcount);
			public delegate uint get_pos_type(IntPtr id);
			public delegate int set_pos_abs_type(IntPtr id, uint pos);
			public delegate int set_pos_rel_type(IntPtr id, int delta, int mode);
			public delegate int push_back_byte_type(IntPtr id, int c);
			public delegate uint get_length_type(IntPtr id);
			public delegate int can_seek_type(IntPtr id);
			public delegate int write_bytes_type(IntPtr id, IntPtr data, int bcount);
			
			public read_bytes_type read_bytes;
			public get_pos_type get_pos;
			public set_pos_abs_type set_pos_abs;
			public set_pos_rel_type set_pos_rel;
			public push_back_byte_type push_back_byte;
			public get_length_type get_length;
			public can_seek_type can_seek;
			public write_bytes_type write_bytes;
		}
		
		private int CallbackReadBytes(IntPtr id, IntPtr data, int bcount) {
			byte[] databuff = new byte[bcount];
			if(pushedBackBytes.Count > 0) {
				int i;
				for(i = 0; i < bcount; ++i)
				{
					databuff[i] = pushedBackBytes.Pop();
				}
				Marshal.Copy(databuff, 0, data, i);
				return i;
			}
			else {
				int length = readStream.Read(databuff, 0, bcount);
				Marshal.Copy(databuff, 0, data, length);
				return length;
			}
		}
		
		private uint CallbackGetPos(IntPtr id) {
			try {
				return (uint)readStream.Position;
			}
			catch {
				return 0;
			}
		}
		
		private int CallbackSetPosAbs(IntPtr id, uint pos) {
			try {
				readStream.Position = pos;
				return 0;
			}
			catch {
				return -1;
			}
		}
		
		private int CallbackSetPosRel(IntPtr id, int delta, int mode) {
			SeekOrigin origin;
			switch(mode) {
				case 0: // SEEK_SET:
					origin = SeekOrigin.Begin;
					break;
					
				case 1:
					origin = SeekOrigin.Current;
					break;
					
				case 2:
					origin = SeekOrigin.End;
					break;
						
				default:
					return -1;
			}
			
			try {
				readStream.Seek(delta, origin);
				return 0;
			}
			catch {
				return -1;
			}
		}
		
		private int CallbackPushBackByte(IntPtr id, int c) {
			byte realc;
			try {
				realc = (byte)c;
			}
			catch {
				return -1;
			}
			
			pushedBackBytes.Push(realc);
			
			return realc;
		}
		
		private uint CallbackGetLength(IntPtr id, int c) {
			try {
				return (uint)readStream.Length;
			}
			catch {
				return 0;
			}
		}
		
		private int CallbackCanSeek(IntPtr id) {
			return readStream.CanSeek ? 1 : 0;
		}
		
		private int CallbackWriteBytes(IntPtr id, IntPtr data, int bcount) {
			byte[] databuff = new byte[bcount];
			Marshal.Copy(data, databuff, 0, bcount);
			readStream.Write(databuff, 0, bcount);
			return bcount;
		}
		
		
		[DllImport("wavpack")]
		private static extern IntPtr WavpackOpenFileInputEx(ref WavPackStreamReader reader, IntPtr wv_id, IntPtr wvc_id, StringBuilder error, Open flags, int norm_offset);
		
		[DllImport("wavpack")]
		private static extern int WavpackGetChannelMask(IntPtr wpc);
		
        [DllImport("wavpack")]   private static extern IntPtr    WavpackCloseFile            (IntPtr wpc);
        [DllImport("wavpack")]   private static extern Int32     WavpackGetBitsPerSample     (IntPtr wpc);
        [DllImport("wavpack")]   private static extern Int32     WavpackGetBytesPerSample    (IntPtr wpc);
        [DllImport("wavpack")]   private static extern FileMode  WavpackGetMode              (IntPtr wpc);
        [DllImport("wavpack")]   private static extern UInt32    WavpackGetLibraryVersion    ();
        [DllImport("wavpack")]   private static extern Int32     WavpackGetNumChannels       (IntPtr wpc);
        [DllImport("wavpack")]   private static extern UInt32    WavpackGetNumSamples        (IntPtr wpc);
        [DllImport("wavpack")]   private static extern UInt32    WavpackGetSampleRate        (IntPtr wpc);
        [DllImport("wavpack")]   private static extern Int32     WavpackGetVersion           (IntPtr wpc);
        [DllImport("wavpack")]   private static extern UInt32    WavpackGetFileSize          (IntPtr wpc);
        [DllImport("wavpack")]   private static extern IntPtr    WavpackOpenFileInput        (String infilename, StringBuilder error, Open flags, Int32 norm_offset);
        [DllImport("wavpack")]   private static extern Boolean   WavpackSeekSample           (IntPtr wpc, UInt32 sample);
        [DllImport("wavpack")]   private static extern UInt32    WavpackUnpackSamples        (IntPtr wpc, [In, Out] Byte[] buffer, UInt32 samples);

        #region IDisposable Members

        ~WavPackDecoder()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IntPtr.Zero != m_WavPackContext)
            {
                m_WavPackContext = WavpackCloseFile(m_WavPackContext);
            }
        }

        #endregion
    }
}
