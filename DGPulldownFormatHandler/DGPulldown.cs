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
		
		[DllImport("DGPulldown", EntryPoint = "TestLoad")]
		private static extern void TestLoad2();
		
		public static void TestLoad() {
			try {
				TestLoad2();
			}
			catch {
				throw new Exception("Could not load DGPulldown.");
			}
		}
		
		[DllImport("DGPulldown")]
		private static extern bool DGPulldownRemoveFileFile(string inFile, string outFile);
		
		[DllImport("DGPulldown")]
		private static extern bool DGPulldownRemoveStreamFile(IOReadFunction read, IOResetFunction reset, string outFile);
		
		[DllImport("DGPulldown")]
		private static extern bool DGPulldownRemoveFileStream(string inFile, IOWriteFunction write);
		
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
		
		public bool RemovePulldown(Stream inStream, string outFile) {
			buffStream = new MemoryStream();
			hasReset = false;
			
			IOReadFunction read = new IOReadFunction(ReadFunc);
			IOResetFunction reset = new IOResetFunction(ResetFunc);
			this.inStream = inStream;
			this.outStream = null;
			return DGPulldownRemoveStreamFile(read, reset, outFile);
		}
		
		public bool RemovePulldown(string inFile, Stream outStream) {
			this.outStream = outStream;
			
			IOWriteFunction write = new IOWriteFunction(WriteFunc);
			return DGPulldownRemoveFileStream(inFile, write);
		}
		
		public bool RemovePulldown(Stream inStream, Stream outStream) {
			buffStream = new MemoryStream();
			hasReset = false;
			this.inStream = inStream;
			this.outStream = outStream;
			
			IOReadFunction read = new IOReadFunction(ReadFunc);
			IOResetFunction reset = new IOResetFunction(ResetFunc);
			IOWriteFunction write = new IOWriteFunction(WriteFunc);
			return DGPulldownRemoveStreamStream(read, reset, write);
		}
	}
}

