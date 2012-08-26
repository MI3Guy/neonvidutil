using System;
using System.Runtime.InteropServices;

namespace WavPackSharp {
	public class WavPack {
		
		private delegate int WavpackBlockOutput(IntPtr id, IntPtr data, int bcount);
		
		[StructLayout(LayoutKind.Sequential)]
		private struct WavpackStreamReader {
			public WavpackStreamReader(bool init) : this() {
				if(init) {
					md5_checksum = new byte[16];
				}
			}
			public float bitrate;
			public float shaping_weight;
			public int bits_per_sample;
			public int bytes_per_sample;
			public int qmode;
			public int flags;
			public int xmode;
			public int num_channels;
			public int float_norm_exp;
			public int block_samples;
			public int extra_flags;
			public int sample_rate;
			public int channel_mask;
			public byte[] md5_checksum;
			public byte md5_read;
			public int num_tag_strings;
			public IntPtr tag_strings;
		}
		
		[DllImport("wavpack")]
		private static extern IntPtr WavpackOpenFileOutput(IntPtr wpc, IntPtr wv_id, IntPtr wvc_id);
		
		[DllImport("wavpack")]
		private static extern int WavpackSetConfiguration(IntPtr wpc, IntPtr config, uint total_samples);
		
		public WavPack() {
		}
		
		
	}
}

