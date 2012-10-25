using System;
using System.IO;
using System.Runtime.InteropServices;
using WAVSharp;

namespace FLACSharp {
	public class FLACEncoder {
		public FLACEncoder(WAVDataChunk inData, Stream outstream, FLACInfo info, Action callback = null, Action<string> output = null) {
			this.inData = inData;
			this.outstream = outstream;
			this.info = info;
			this.output = output;
			
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
			_callback = callback;
			
			WriteCallback = new FLACSharpAPI.FLAC__StreamEncoderWriteCallback(Write);
			SeekCallback = new FLACSharpAPI.FLAC__StreamEncoderSeekCallback(Seek);
			TellCallback = new FLACSharpAPI.FLAC__StreamEncoderTellCallback(Tell);
		}
		
		WAVDataChunk inData;
		Stream outstream;
		FLACInfo info;
		IntPtr encoder;
		uint samples;
		Action _callback;
		Action<string> output;
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
							throw new ApplicationException(string.Format("An error occurred during the encode.\nError code: {0}", FLACSharpAPI.FLAC__stream_encoder_get_state(encoder)));
						}
						_callback();
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
		
		private void CheckChannelMapping(FLACInfo info) {
			const string WarningMessage = "Warning: Channel Mapping not supported by FLAC standard.";
			switch(info.channels) {
				case 1:
					if(info.channel_mapping != (uint)WAVConst.Speaker.FrontCenter) {
						output(WarningMessage + " Probably OK for mono tracks.");
					}
					break;
					
				case 2:
					if(info.channel_mapping != (uint)(WAVConst.Speaker.FrontLeft | WAVConst.Speaker.FrontRight)) {
						output(WarningMessage);
					}
					break;
					
				case 3:
					if(info.channel_mapping != (uint)(WAVConst.Speaker.FrontLeft | WAVConst.Speaker.FrontRight | WAVConst.Speaker.FrontCenter)) {
						output(WarningMessage);
					}
					break;
					
				case 4:
					if(info.channel_mapping != (uint)(WAVConst.Speaker.FrontLeft | WAVConst.Speaker.FrontRight
				                            | WAVConst.Speaker.BackLeft | WAVConst.Speaker.BackRight)) {
						output(WarningMessage);
					}
					break;
					
				case 5:
					if(info.channel_mapping != (uint)(WAVConst.Speaker.FrontLeft | WAVConst.Speaker.FrontRight | WAVConst.Speaker.FrontCenter
				                            | WAVConst.Speaker.BackLeft | WAVConst.Speaker.BackRight)) {
						output(WarningMessage);
					}
					break;
					
				case 6:
					if(info.channel_mapping != (uint)WAVConst.Speaker.FivePointOne) {
						output(WarningMessage);
					}
					break;
					
				case 7:
					output(WarningMessage);
					break;
					
				case 8:
					if(info.channel_mapping == (uint)WAVConst.Speaker.SevenPointOneReal) {
						output(WarningMessage + " Input configuration is the most commonly used channel mapping for 8 channel FLAC.");
					}
					else {
						output(WarningMessage);
					}
					break;
					
				default:
					output("Number of channels is not supported by flac.");
					break;
			}
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

