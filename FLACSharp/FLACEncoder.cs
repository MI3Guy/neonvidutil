using System;
using System.IO;
using System.Runtime.InteropServices;
using WAVSharp;

namespace FLACSharp {
	public class FLACEncoder {
		public FLACEncoder(WAVDataChunk inData, Stream outstream, FLACInfo info) {
			this.inData = inData;
			this.outstream = outstream;
			this.info = info;
			
			encoder = FLACSharpAPI.FLAC__stream_encoder_new();
			if(encoder == IntPtr.Zero) {
				throw new ApplicationException("Error allocating FLAC encoder.");
			}
			
			bool ok = true;
			
			ok &= FLACSharpAPI.FLAC__stream_encoder_set_verify(encoder, true);
			ok &= FLACSharpAPI.FLAC__stream_encoder_set_compression_level(encoder, 8);
			ok &= FLACSharpAPI.FLAC__stream_encoder_set_channels(encoder, info.channels);
			ok &= FLACSharpAPI.FLAC__stream_encoder_set_bits_per_sample(encoder, info.bits_per_sample);
			ok &= FLACSharpAPI.FLAC__stream_encoder_set_sample_rate(encoder, info.sample_rate);
			
			if(!ok) {
				throw new ArgumentException("Invalid FLACInfo settings.");
			}
			
			samples = info.sample_rate;
			
			WriteCallback = new FLACSharpAPI.FLAC__StreamEncoderWriteCallback(Write);
			SeekCallback = new FLACSharpAPI.FLAC__StreamEncoderSeekCallback(Seek);
			TellCallback = new FLACSharpAPI.FLAC__StreamEncoderTellCallback(Tell);
		}
		
		WAVDataChunk inData;
		Stream outstream;
		FLACInfo info;
		IntPtr encoder;
		uint samples;
		FLACSharpAPI.FLAC__StreamEncoderWriteCallback WriteCallback;
		FLACSharpAPI.FLAC__StreamEncoderSeekCallback  SeekCallback;
		FLACSharpAPI.FLAC__StreamEncoderTellCallback  TellCallback;
			
			
		public unsafe void Encode() {
			
			if(outstream.CanSeek) {
				if(FLACSharpAPI.FLAC__stream_encoder_init_stream(encoder, WriteCallback, SeekCallback, TellCallback, null, IntPtr.Zero) != FLACSharpAPI.FLAC__StreamEncoderInitStatus.OK) {
					throw new ApplicationException("An error occurred while initializing the output stream.");
				}
			}
			else {
				if(FLACSharpAPI.FLAC__stream_encoder_init_stream(encoder, WriteCallback, null, null, null, IntPtr.Zero) != FLACSharpAPI.FLAC__StreamEncoderInitStatus.OK) {
					throw new ApplicationException("An error occurred while initializing the output stream.");
				}
			}
			
			int[] pcm = new int[info.channels * samples];
			uint sampleCounter = 0;
			
			WAVDataSample sample = inData.ReadSample();
			while(sample != null) {
				for(int i = 0; i < info.channels; ++i) {
					pcm[sampleCounter * info.channels + i] = sample.GetSampleForChannel2(i);
				}
				
				++sampleCounter;
				sample = inData.ReadSample();
				if(sampleCounter == samples || sample == null) {
					fixed(int* pcmbuff = pcm) {
						IntPtr ptr = new IntPtr(pcmbuff);
						if(!FLACSharpAPI.FLAC__stream_encoder_process_interleaved(encoder, ptr, sampleCounter)) {
							
							using(StreamWriter log = File.CreateText("log.txt")) {
								foreach(int pcmVal in pcm) {
									log.WriteLine("{0:X} {1} {2}", pcmVal, pcmVal > 0x7FFFFF, pcmVal < -(0x7FFFFF+1));
								}
							}
							
							throw new ApplicationException(string.Format("An error occurred during the encode.\nError code: {0}", FLACSharpAPI.FLAC__stream_encoder_get_state(encoder)));
						}
					}
					sampleCounter = 0;
				}
				
			}
			
			
			
			if(!FLACSharpAPI.FLAC__stream_encoder_finish(encoder)) {
				throw new ApplicationException("An error occurred while finishing the encode.");
			}
			
			FLACSharpAPI.FLAC__stream_encoder_delete(encoder);
			encoder = IntPtr.Zero;
		}
		
		private FLACSharpAPI.FLAC__StreamEncoderWriteStatus Write(IntPtr encoder, IntPtr buffer, uint bytes, uint samples, uint current_frame, IntPtr client_data) {
			byte[] buff2 = new byte[bytes];
			Marshal.Copy(buffer, buff2, 0, (int)bytes);
			outstream.Write(buff2, 0, (int)bytes);
			return FLACSharpAPI.FLAC__StreamEncoderWriteStatus.OK;
		}
		
		private FLACSharpAPI.FLAC__StreamEncoderSeekStatus Seek(IntPtr encoder, long absolute_byte_offset, IntPtr client_data) {
			outstream.Position = absolute_byte_offset;
			return FLACSharpAPI.FLAC__StreamEncoderSeekStatus.OK;
		}
		
		private FLACSharpAPI.FLAC__StreamEncoderTellStatus Tell(IntPtr encoder, out long absolute_byte_offset, IntPtr client_data) {
			absolute_byte_offset = outstream.Position;
			return FLACSharpAPI.FLAC__StreamEncoderTellStatus.OK;
		}
		
	}
}
