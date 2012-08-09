using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NeonVidUtil.Plugin.DGPulldownFormatHandler {
	public class DGPulldown {
		
		private Stream inStream;
		private Stream outStream;
		private MemoryStream buffStream;
		private bool hasReset;
		
		private delegate int IOReadFunction(IntPtr buff, int count);
		private delegate void IOWriteFunction(IntPtr buff, int count);
		private delegate void IOResetFunction();
		
		[DllImport("DGPulldown")]
		private static extern bool DGPulldownRemoveFileFile(string inFile, string outFile);
		
		[DllImport("DGPulldown")]
		private static extern bool DGPulldownRemoveStreamStream(IOReadFunction read, IOResetFunction reset, IOWriteFunction write);
		
		private int ReadFunc(IntPtr buff, int count) {
			byte[] data = new byte[count];
			int len;
			if(hasReset && buffStream != null) {
				len = buffStream.Read(data, 0, count);
				System.Runtime.InteropServices.Marshal.Copy(data, 0, buff, len);
				if(len != 0) { // If end of MemoryStream read from original stream.
					return len;
				}
				else {
					buffStream = null;
				}
			}
		
			len = inStream.Read(data, 0, count);
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buff, len);
			if(!hasReset) {
				buffStream.Write(data, 0, len);
			}
			return len;
		}
		
		private void ResetFunc() {
			hasReset = true;
			buffStream.Position = 0;
		}
		
		private void WriteFunc(IntPtr buff, int count) {
			byte[] data = new byte[count];
			System.Runtime.InteropServices.Marshal.Copy(buff, data, 0, count);
			outStream.Write(data, 0, count);
		}
		
		public bool RemovePulldown(string inFile, string outFile) {
			return DGPulldownRemoveFileFile(inFile, outFile);
		}
		
		public bool RemovePulldown(Stream inStream, Stream outStream) {
			buffStream = new MemoryStream();
			hasReset = false;
			this.inStream = inStream;
			this.outStream = outStream;
			
			IOReadFunction read = new IOReadFunction(ReadFunc);
			IOWriteFunction write = new IOWriteFunction(WriteFunc);
			IOResetFunction reset = new IOResetFunction(ResetFunc);
			return DGPulldownRemoveStreamStream(read, reset, write);
		}
	}
}

