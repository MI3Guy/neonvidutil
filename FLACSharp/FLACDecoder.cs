using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using WAVSharp;

namespace FLACSharp {
	public class FLACDecoder {
		public FLACDecoder(Stream inStream, Stream outStream, Action callback) {
			this.inStream = inStream;
			this.outStream = outStream;
			this.callback = callback;
		}
		
		private Stream inStream;
		private Stream outStream;
		private Action callback;
		private bool hasWrittenHeader;
		
		public void Process() {
			IntPtr decoder = FLAC__stream_decoder_new();
			
			if(decoder == IntPtr.Zero) {
				throw new OutOfMemoryException();
			}
			
			FLAC__stream_decoder_set_md5_checking(decoder, true);
			
			StreamDecoderReadCallback readCallback = new StreamDecoderReadCallback(ReadCallback);
			StreamDecoderWriteCallback writeCallback = new StreamDecoderWriteCallback(WriteCallback);
			StreamDecoderMetadataCallback metadataCallback = null;
			StreamDecoderErrorCallback errorCallback = new StreamDecoderErrorCallback(ErrorCallback);
			
			StreamDecoderSeekCallback seekCallback = null;
			StreamDecoderTellCallback tellCallback = null;
			StreamDecoderLengthCallback lengthCallback = null;
			StreamDecoderEofCallback eofCallback = null;
			
			if(inStream.CanSeek) {
				seekCallback = new StreamDecoderSeekCallback(SeekCallback);
				tellCallback = new StreamDecoderTellCallback(TellCallback);
				lengthCallback = new StreamDecoderLengthCallback(LengthCallback);
				eofCallback = new StreamDecoderEofCallback(EofCallback);
			}
			
			if(FLAC__stream_decoder_init_stream(decoder, readCallback,
			                                    seekCallback, tellCallback, lengthCallback, eofCallback,
			                                    writeCallback, metadataCallback, errorCallback, IntPtr.Zero) != StreamDecoderInitStatus.OK) {
				throw new ApplicationException("An error occurred while initializing the FLAC decoder.");
			}
			
			if(!FLAC__stream_decoder_process_until_end_of_stream(decoder)) {
				throw new ApplicationException("An error occurred while processing the stream.");	
			}
			
			if(!FLAC__stream_decoder_finish(decoder)) {
				// TODO: Handle MD5 hash check failed.
			}
			
			FLAC__stream_decoder_delete(decoder);
		}
		
		private StreamDecoderReadStatus ReadCallback(IntPtr decoder, IntPtr buffer, ref IntPtr bytes, IntPtr clientData) {
			int realBytes = (int)bytes;
			if(realBytes > 0) {
				byte[] buff = new byte[realBytes];
				
				realBytes = inStream.Read(buff, 0, realBytes);
				bytes = (IntPtr)realBytes;
				Marshal.Copy(buff, 0, buffer, realBytes);
				
				callback();
				if(realBytes == 0) {
					return StreamDecoderReadStatus.EndOfStream;
				}
				else {
					return StreamDecoderReadStatus.Continue;
				}
			}
			else {
				return StreamDecoderReadStatus.Abort;
			}
		}
		
		private StreamDecoderSeekStatus SeekCallback(IntPtr decoder, ulong absoluteByteOffset, IntPtr clientData) {
			if(inStream.CanSeek) {
				inStream.Position = (long)absoluteByteOffset;
				return StreamDecoderSeekStatus.OK;
			}
			else {
				return StreamDecoderSeekStatus.Unsupported;
			}
		}
		
		private StreamDecoderTellStatus TellCallback(IntPtr decoder, ref ulong absoluteByteOffset, IntPtr clientData) {
			if(inStream.CanSeek) {
				absoluteByteOffset = (ulong)inStream.Position;
				return StreamDecoderTellStatus.OK;
			}
			else {
				return StreamDecoderTellStatus.Unsupported;
			}
		}
		
		private StreamDecoderLengthStatus LengthCallback(IntPtr decoder, ref ulong streamLength, IntPtr clientData) {
			try {
				streamLength = (ulong)inStream.Length;
				return StreamDecoderLengthStatus.OK;
			}
			catch(NotSupportedException) {
				return StreamDecoderLengthStatus.Unsupported;
			}
		}
		
		private bool EofCallback(IntPtr decoder, IntPtr clientData) {
			return false;
		}
		
		private StreamDecoderWriteStatus WriteCallback(IntPtr decoder, ref FlacFrame frame, IntPtr buffer, IntPtr clientData) {
			int bitDepth = frame.Header.BitsPerSample;
			int numChannels = frame.Header.Channels;
			int samplesPerChannel = frame.Header.BlockSize;
			
			if(!hasWrittenHeader) {
				hasWrittenHeader = true;
				new WAVWriter(
					outStream,
					new WAVFormatChunk(FileLength: 0, channels: unchecked((ushort)numChannels), samplesPerSec: (uint)frame.Header.SampleRate, bitsPerSample: (ushort)bitDepth),
					0);
			}
			
			int[] samples = new int[numChannels * samplesPerChannel];
			IntPtr[] channelArrays = new IntPtr[numChannels];
			Marshal.Copy(buffer, channelArrays, 0, numChannels);
			
			for(int i = 0; i < numChannels; ++i) {
				Marshal.Copy(channelArrays[i], samples, i * samplesPerChannel, samplesPerChannel);
			}
			
			for(int i = 0; i < samplesPerChannel; ++i) {
				for(int j = 0; j < numChannels; ++j) {
					byte[] bytes = BitConverter.GetBytes(samples[i + j * samplesPerChannel]);
					
					int bytesPerSample = bitDepth / 8;
					switch(bytesPerSample) {
						case 2:
						case 3:
							outStream.Write(bytes, 0, bytesPerSample);
							break;
							
						default:
							throw new NotSupportedException(string.Format("FLACSharp does not suppot {0}-bit files.", bitDepth));
					}
				}
			}
			
			return StreamDecoderWriteStatus.Continue;
		}
			
		private void ErrorCallback(IntPtr decoder, StreamDecoderErrorStatus status, IntPtr clientData) {
			Console.WriteLine(status);
		}
		
		[StructLayout(LayoutKind.Sequential)]
		struct FlacFrame
        {
		    private const int FlacMaxChannels = 8;	
            public FrameHeader Header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = FlacMaxChannels)]
            public FlacSubFrame[] Subframes;
            public FrameFooter Footer;
        }
		
		[StructLayout(LayoutKind.Sequential)]
        struct FrameHeader
        {
            public int BlockSize;
            public int SampleRate;
            public int Channels;
            public int ChannelAssignment;
            public int BitsPerSample;
            public FrameNumberType NumberType;
            public long FrameOrSampleNumber;
            public byte Crc;
        }
		
		[StructLayout(LayoutKind.Sequential)]
        struct FlacSubFrame
        {
            public SubframeType Type;
            public IntPtr Data;
            public int WastedBits;
        }
		
		[StructLayout(LayoutKind.Sequential)]
        struct FrameFooter
        {
            public ushort Crc;
        }

        enum FrameNumberType
        {
            Frame,
            Sample
        }

        enum SubframeType
        {
            Constant,
            Verbatim,
            Fixed,
            LPC
        }
		
		enum StreamDecoderInitStatus {
			OK,
			UnsupportedContainer,
			InvalidCallbacks,
			MemoryAllocationError,
			ErrorOpeningFile,
			AlreadyInitialized
		}
		
		enum StreamDecoderReadStatus {
			Continue,
			EndOfStream,
			Abort
		}
		
		enum StreamDecoderSeekStatus {
			OK,
			Error,
			Unsupported
		}
		
		enum StreamDecoderTellStatus {
			OK,
			Error,
			Unsupported
		}
		
		enum StreamDecoderLengthStatus {
			OK,
			Error,
			Unsupported
		}
		
		enum StreamDecoderWriteStatus {
			Continue,
			Abort
		}
		
		enum StreamDecoderErrorStatus {
			LostSync,
			BadHeader,
			FrameCRCMismatch,
			UnparseableStream
		}
		
		// IntPtr bytes = size_t
		delegate StreamDecoderReadStatus StreamDecoderReadCallback(IntPtr decoder, IntPtr buffer, ref IntPtr bytes, IntPtr clientData);
		delegate StreamDecoderSeekStatus StreamDecoderSeekCallback(IntPtr decoder, ulong absoluteByteOffset, IntPtr clientData);
		delegate StreamDecoderTellStatus StreamDecoderTellCallback(IntPtr decoder, ref ulong absoluteByteOffset, IntPtr clientData);
		delegate StreamDecoderLengthStatus StreamDecoderLengthCallback(IntPtr decoder, ref ulong streamLength, IntPtr clientData);
		delegate bool StreamDecoderEofCallback(IntPtr decoder, IntPtr clientData);
		delegate StreamDecoderWriteStatus StreamDecoderWriteCallback(IntPtr decoder, ref FlacFrame frame, IntPtr buffer, IntPtr clientData);
		delegate void StreamDecoderMetadataCallback(IntPtr decoder, IntPtr metadata, IntPtr clientData);
		delegate void StreamDecoderErrorCallback(IntPtr decoder, StreamDecoderErrorStatus status, IntPtr clientData);
		
		
		[DllImport("libFLAC")]
		private static extern IntPtr FLAC__stream_decoder_new();
		
		[DllImport("libFLAC")]
		private static extern void FLAC__stream_decoder_delete(IntPtr decoder);
		
		[DllImport("libFLAC")]
		private static extern bool FLAC__stream_decoder_set_md5_checking(IntPtr decoder, bool value);
		
		[DllImport("libFLAC")]
		private static extern StreamDecoderInitStatus FLAC__stream_decoder_init_stream(IntPtr decoder,
		                                                                               StreamDecoderReadCallback read_callback,
		                                                                               StreamDecoderSeekCallback seek_callback,
		                                                                               StreamDecoderTellCallback tell_callback,
		                                                                               StreamDecoderLengthCallback length_callback,
		                                                                               StreamDecoderEofCallback eof_callback,
		                                                                               StreamDecoderWriteCallback write_callback,
		                                                                               StreamDecoderMetadataCallback metadata_callback,
		                                                                               StreamDecoderErrorCallback error_callback,
		                                                                               IntPtr clientData);
		
		[DllImport("libFLAC")]
		private static extern bool FLAC__stream_decoder_process_until_end_of_stream(IntPtr decoder);
		
		[DllImport("libFLAC")]
		private static extern bool FLAC__stream_decoder_finish(IntPtr decoder);
				
		[DllImport("libFLAC")]
		private static extern ulong FLAC__stream_decoder_get_total_samples(IntPtr decoder);
		
	}
}

