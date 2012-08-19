using System;
using System.Runtime.InteropServices;

namespace WavPackSharp {
	public class WavPack {
		
		private delegate int WavpackBlockOutput(IntPtr id, IntPtr data, int bcount);
		
		[DllImport("wavpack")]
		private static extern IntPtr WavpackOpenFileOutput(IntPtr wpc, IntPtr wv_id, IntPtr wvc_id);
		
		[DllImport("wavpack")]
		private static extern int WavpackSetConfiguration(IntPtr wpc, IntPtr config, uint total_samples);
		
		public WavPack() {
		}
		
		
	}
}

