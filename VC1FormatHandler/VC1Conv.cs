using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1Conv {
		private delegate uint StreamRead(IntPtr stream, IntPtr buffer, uint length);
		private delegate uint StreamWrite(IntPtr stream, IntPtr buffer, uint length);
		
		private Stream inStream;
		private Stream outStream;
		
		private uint StreamRead_Func(IntPtr stream, IntPtr buffer, uint length) {
			byte[] data = new byte[(int)length];
			uint len = (uint)inStream.Read(data, 0, (int)length);
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buffer, (int)len);
			return len;
		}
		
		private uint StreamWrite_Func(IntPtr stream, IntPtr buffer, uint length) {
			byte[] data = new byte[(int)length];
			System.Runtime.InteropServices.Marshal.Copy(buffer, data, 0, (int)length);
			outStream.Write(data, 0, (int)length);
			return length;
		}
		
		[DllImport("vc1conv", EntryPoint = "TestLoad")]
		private static extern void TestLoad2();
		
		public static void TestLoad() {
			try {
				TestLoad2();
			}
			catch {
				throw new Exception("An error occurred while loading vc1conv.");
			}
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownFileFile(string inFile, string outFile);
		
		public bool VC1ConvRemovePulldown(string inFile, string outFile) {
			return VC1ConvRemovePulldownFileFile(inFile, outFile) != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownFileStream(string inFile, StreamWrite outStreamWrite, IntPtr outStream);
		
		public bool VC1ConvRemovePulldown(string inFile, Stream outStream) {
			this.outStream = outStream;
			
			StreamWrite writer = new StreamWrite(StreamWrite_Func);
			
			return VC1ConvRemovePulldownFileStream(inFile, writer, IntPtr.Zero) != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownStreamFile(StreamRead inStreamRead, IntPtr inFid, string outFile);
		
		public bool VC1ConvRemovePulldown(Stream inStream, string outFile) {
			this.inStream = inStream;
			
			StreamRead reader = new StreamRead(StreamRead_Func);
			
			return VC1ConvRemovePulldownStreamFile(reader, IntPtr.Zero, outFile) != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownStreamStream(StreamRead inStreamRead, IntPtr inFid, StreamWrite outStreamWrite, IntPtr outFid);
		
		public bool VC1ConvRemovePulldown(Stream inStream, Stream outStream) {
			this.outStream = outStream;
			this.inStream = inStream;
			
			StreamWrite writer = new StreamWrite(StreamWrite_Func);
			StreamRead reader = new StreamRead(StreamRead_Func);
			
			return VC1ConvRemovePulldownStreamStream(reader, IntPtr.Zero, writer, IntPtr.Zero) != 0;
		}
	}
}

