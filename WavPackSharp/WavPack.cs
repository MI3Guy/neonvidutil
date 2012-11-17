using System;
using System.Runtime.InteropServices;

namespace WavPackSharp {
	public static class WavPack {
		
		[DllImport("wavpack")]
		private static extern uint WavpackGetLibraryVersion();
		
		public static uint Version {
			get {
				return WavpackGetLibraryVersion();
			}
		}
		
		public static void TestLoad() {
			try {
				WavpackGetLibraryVersion();
			}
			catch {
				throw new Exception("Could not load WavPack.");
			}
		}
	}
}

