using System;
using System.Runtime.InteropServices;

namespace FLACSharp {
	internal static class FLACSharpAPI {
		internal enum FLAC__StreamEncoderReadStatus {
			CONTINUE,
			END_OF_STREAM,
			ABORT,
			UNSUPPORTED
		}
		
		internal enum FLAC__StreamEncoderWriteStatus {
			OK,
			FATAL_ERROR
		}
		
		internal enum FLAC__StreamEncoderSeekStatus {
			OK,
			ERROR,
			UNSUPPORTED
		}
		
		internal enum FLAC__StreamEncoderTellStatus {
			OK,
			ERROR,
			UNSUPPORTED
		}
		
		internal enum FLAC__StreamEncoderInitStatus {
			OK,
			ENCODER_ERROR,
			UNSUPPORTED_CONTAINER,
			INVALID_CALLBACKS,
			INVALID_NUMBER_OF_CHANNELS,
			INVALID_BITS_PER_SAMPLE,
			INVALID_SAMPLE_RATE,
			INVALID_BLOCK_SIZE,
			INVALID_MAX_LPC_ORDER,
			INVALID_QLP_COEFF_PRECISION,
			BLOCK_SIZE_TOO_SMALL_FOR_LPC_ORDER,
			NOT_STREAMABLE,
			INVALID_METADATA
		}
		
		internal enum FLAC__StreamEncoderState {
			OK,
			UNINITIALIZED,
			OGG_ERROR,
			VERIFY_DECODER_ERROR,
			VERIFY_MISMATCH_IN_AUDIO_DATA,
			IO_ERROR,
			FRAMING_ERROR,
			MEMORY_ALLOCATION_ERROR
		}
		
		internal delegate FLAC__StreamEncoderReadStatus  FLAC__StreamEncoderReadCallback    (IntPtr encoder, IntPtr buffer, uint bytes, IntPtr client_data);
		internal delegate FLAC__StreamEncoderWriteStatus FLAC__StreamEncoderWriteCallback   (IntPtr encoder, IntPtr buffer, uint bytes, uint samples, uint current_frame, IntPtr client_data);
		internal delegate FLAC__StreamEncoderSeekStatus  FLAC__StreamEncoderSeekCallback    (IntPtr encoder, long absolute_byte_offset, IntPtr client_data);
		internal delegate FLAC__StreamEncoderTellStatus  FLAC__StreamEncoderTellCallback    (IntPtr encoder, out long absolute_byte_offset, IntPtr client_data);
		internal delegate void                           FLAC__StreamEncoderMetadataCallback(IntPtr encoder, IntPtr metadata, IntPtr client_data);
		
		[DllImport("libFLAC")]
		internal static extern IntPtr FLAC__stream_encoder_new();
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_set_verify(IntPtr encoder, bool value);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_set_compression_level(IntPtr encoder, uint value);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_set_channels(IntPtr encoder, uint value);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_set_bits_per_sample(IntPtr encoder, uint value);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_set_sample_rate(IntPtr encoder, uint value);
		
		[DllImport("libFLAC")]
		internal static extern FLAC__StreamEncoderInitStatus FLAC__stream_encoder_init_stream(IntPtr encoder,
		                                                                                           FLAC__StreamEncoderWriteCallback write_callback,
		                                                                                           FLAC__StreamEncoderSeekCallback seek_callback,
		                                                                                           FLAC__StreamEncoderTellCallback tell_callback,
		                                                                                           FLAC__StreamEncoderMetadataCallback metadata_callback,
		                                                                                           IntPtr client_data);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_process_interleaved(IntPtr encoder, IntPtr buffer, uint samples);
		
		[DllImport("libFLAC")]
		internal static extern bool FLAC__stream_encoder_finish(IntPtr encoder);
		
		[DllImport("libFLAC")]
		internal static extern void FLAC__stream_encoder_delete(IntPtr encoder);
		
		[DllImport("libFLAC")]
		internal static extern FLAC__StreamEncoderState FLAC__stream_encoder_get_state(IntPtr encoder);
		
		/*
		internal static string FLAC__StreamEncoderStateString {
			get {
				return GetStatus("FLAC__StreamEncoderStateString");
			}
		}
		
		private static class Dlfcn {
			[DllImport("dl")]
			public static extern IntPtr dlopen(string filename, int flag);
			
			[DllImport("dl")]
			public static extern string dlerror();
			
			[DllImport("dl")]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);
			
			[DllImport("dl")]
			public static extern int dlclose(IntPtr handle);
		}
		
		private unsafe static string GetStatus(string name, int index) {
			IntPtr handle = Dlfcn.dlopen("libFLAC.so", 1);
			IntPtr ptr = Dlfcn.dlsym(handle, name);
			string err = Dlfcn.dlerror();
			if(err != null) {
				return null;
			}
			//string str = Marshal.PtrToStringAnsi(ptr);
			
			char** 
			
			Dlfcn.dlclose(handle);
			return str;
		}*/
	}
}

