using System;
using System.IO;
using System.Runtime.InteropServices;
using WAVSharp;

namespace WavPackSharp {
	public sealed class WavPackEncoder {
		
		private delegate int WavpackBlockOutput(IntPtr id, IntPtr data, int bcount);
		
		[StructLayout(LayoutKind.Sequential)]
		private struct WavpackConfig {
			public float bitrate;
			public float shaping_weight;
			public int bits_per_sample;
			public int bytes_per_sample;
			public int qmode;
			public WavpackConfigFlags flags;
			public int xmode;
			public int num_channels;
			public int float_norm_exp;
			public int block_samples;
			public int extra_flags;
			public int sample_rate;
			public int channel_mask;
			public byte md5_checksum00;
			public byte md5_checksum01;
			public byte md5_checksum02;
			public byte md5_checksum03;
			public byte md5_checksum04;
			public byte md5_checksum05;
			public byte md5_checksum06;
			public byte md5_checksum07;
			public byte md5_checksum08;
			public byte md5_checksum09;
			public byte md5_checksum10;
			public byte md5_checksum11;
			public byte md5_checksum12;
			public byte md5_checksum13;
			public byte md5_checksum14;
			public byte md5_checksum15;
			public byte md5_read;
			public int num_tag_strings;
			public IntPtr tag_strings;
		}
		
		[Flags]
		private enum WavpackConfigFlags : uint {
			Hybrid			= 0x00000008,
			JointStereo		= 0x00000010,
			HybridShape		= 0x00000040,
			Fast			= 0x00000200,
			High			= 0x00000800,
			VeryHigh		= 0x00001000,
			BitrateKbps		= 0x00002000,
			ShapeOverride	= 0x00008000,
			JointOverride	= 0x00010000,
			DynamicShaping	= 0x00020000,
			CreateExe		= 0x00040000,
			CreateWvc		= 0x00080000,
			OptimizeWvc		= 0x00100000,
			CalcNoize		= 0x00800000,
			ExtraMode		= 0x02000000,
			SkipWvx			= 0x04000000,
			MD5Checksum		= 0x08000000,
			MergeBlocks		= 0x10000000,
			PairUndefChans	= 0x20000000,
			OptimizeMono	= 0x80000000
		}
		
		[DllImport("wavpack")]
		private static extern IntPtr WavpackOpenFileOutput(WavpackBlockOutput wpc, IntPtr wv_id, IntPtr wvc_id);
		
		[DllImport("wavpack")]
		private static extern int WavpackSetConfiguration(IntPtr wpc, ref WavpackConfig config, uint total_samples);
		
		[DllImport("wavpack")]
		private static extern int WavpackPackInit(IntPtr wpc);
		
		[DllImport("wavpack")]
		private static unsafe extern int WavpackPackSamples(IntPtr wpc, IntPtr sample_buffer, uint sample_count);
		
		[DllImport("wavpack")]
		private static extern int WavpackFlushSamples(IntPtr wpc);
		
		[DllImport("wavpack")]
		private static extern string WavpackGetErrorMessage(IntPtr wpc);
		
		[DllImport("wavpack")]
		private static extern void WavpackUpdateNumSamples(IntPtr wpc, IntPtr first_block);
		
		// WavpackStoreMD5Sum
		
		[DllImport("wavpack")]
		private static extern IntPtr WavpackCloseFile(IntPtr wpc);
		
		
		private WavPackEncoder() {
			
		}
		
		
		
		private Stream outStream;
		public int firstBlockSize;
		
		private int StreamWrite(IntPtr id, IntPtr data, int bcount) {
			byte[] buff = new byte[bcount];
			Marshal.Copy(data, buff, 0, bcount);
			outStream.Write(buff, 0, bcount);
			if(firstBlockSize == 0)
			{
				firstBlockSize = bcount;
			}
			return 1;
		}
		
		public static unsafe void Encode(WAVReader inData, Stream outstream, Action callback) {
			WavPackEncoder wv = new WavPackEncoder();
			wv.outStream = outstream;
			WavpackBlockOutput writeFunc = new WavpackBlockOutput(wv.StreamWrite);
			
			IntPtr wpc = WavpackOpenFileOutput(writeFunc, IntPtr.Zero, IntPtr.Zero);
			if(wpc == IntPtr.Zero) {
				throw new OutOfMemoryException("WavPack could not allocate memory for the context.");
			}
			
			WavpackConfig cfg = new WavpackConfig();
			cfg.bytes_per_sample = inData.FormatChunk.wBitsPerSample / 8;
			cfg.bits_per_sample = (inData.FormatChunk.cbSize > 0 ? inData.FormatChunk.wValidBitsPerSample : inData.FormatChunk.wBitsPerSample);
			cfg.channel_mask = (inData.FormatChunk.cbSize > 0 ? (int)inData.FormatChunk.dwChannelMask : GetDefaultChannelMask(inData.FormatChunk.nChannels));
			cfg.num_channels = inData.FormatChunk.nChannels;
			cfg.sample_rate = (int)inData.FormatChunk.nSamplesPerSec;
			cfg.flags = WavpackConfigFlags.VeryHigh | WavpackConfigFlags.ExtraMode;
			cfg.xmode = 3;
			
			if(WavpackSetConfiguration(wpc, ref cfg, unchecked((uint)-1)) == 0) {
				throw new ApplicationException(WavpackGetErrorMessage(wpc));
			}
			
			if(WavpackPackInit(wpc) == 0) {
				throw new ApplicationException(WavpackGetErrorMessage(wpc));
			}
			
			int[] pcm = new int[cfg.num_channels * cfg.sample_rate];
			uint sampleCounter = 0;
			WAVDataSample sample = inData.DataChunk.ReadSample();
			while(sample != null) {
				callback();
				
				for(int i = 0; i < sample.Channels; ++i) {
					pcm[sampleCounter * cfg.num_channels + i] = sample.GetSampleForChannel2(i);
				}
				
				++sampleCounter;
				sample = inData.DataChunk.ReadSample();
				
				if(sampleCounter == cfg.sample_rate || sample == null) {
					fixed(int* pcmbuff = pcm) {
						IntPtr ptr = new IntPtr(pcmbuff);
						if(WavpackPackSamples(wpc, ptr, sampleCounter) == 0) {
							throw new ApplicationException(WavpackGetErrorMessage(wpc));
						}
					}
					sampleCounter = 0;
				}
			}
			
			if(WavpackFlushSamples(wpc) == 0) {
				throw new ApplicationException(WavpackGetErrorMessage(wpc));
			}
			
			// Update number of samples if possible.
			if(outstream.CanSeek) {
				byte[] blockBuff = new byte[wv.firstBlockSize];
				
				outstream.Position = 0;
				outstream.Read(blockBuff, 0, blockBuff.Length);
				
				fixed(byte* blockBuffPtr = blockBuff) {
					IntPtr blockBuffIntPtr = new IntPtr(blockBuffPtr);
					WavpackUpdateNumSamples(wpc, blockBuffIntPtr);
				}
				
				outstream.Position = 0;
				outstream.Write(blockBuff, 0, blockBuff.Length);
			}
			
			WavpackCloseFile(wpc);
		}
		
		private static int GetDefaultChannelMask(int channels) {
			switch(channels) {
				case 1:
					return (int)WAVConst.Speaker.FrontCenter;
					
				case 2:
					return (int)(WAVConst.Speaker.FrontLeft | WAVConst.Speaker.FrontRight);
					
				default:
					throw new ApplicationException("Could not determine channel mapping.");
			}
		}
		
	}
}

