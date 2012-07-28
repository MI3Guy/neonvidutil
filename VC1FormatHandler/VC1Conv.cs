using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace VC1FormatHandler {
	public static class VC1Conv {
		private static Dictionary<int, Stream> VirtualFiles = new Dictionary<int, Stream>();
		private static int AddVFileStream(Stream vfile) {
			int attempts = 0;
			int fid;
			Random rand = new Random();
			lock(VirtualFiles) {
				do {
					fid = rand.Next();
					if(fid == 0) continue;
					
					if(++attempts > 10) {
						throw new ApplicationException("Could not find virtual file name.");
					}
				} while(VirtualFiles.ContainsKey(fid) || fid == 0);
				VirtualFiles.Add(fid, vfile);
			}
			return fid;
		}
		
		private delegate uint StreamRead(IntPtr stream, IntPtr buffer, uint length);
		private delegate uint StreamWrite(IntPtr stream, IntPtr buffer, uint length);
		
		private static uint StreamRead_Func(IntPtr stream, IntPtr buffer, uint length) {
			byte[] data = new byte[(int)length];
			uint len = (uint)VirtualFiles[(int)stream].Read(data, 0, (int)length);
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buffer, (int)length);
			return len;
		}
		
		private static uint StreamWrite_Func(IntPtr stream, IntPtr buffer, uint length) {
			byte[] data = new byte[(int)length];
			System.Runtime.InteropServices.Marshal.Copy(buffer, data, 0, (int)length);
			VirtualFiles[(int)stream].Write(data, 0, (int)length);
			return length;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownFileFile(string inFile, string outFile);
		
		public static bool VC1ConvRemovePulldown(string inFile, string outFile) {
			return VC1ConvRemovePulldownFileFile(inFile, outFile) != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownFileStream(string inFile, StreamWrite outStreamWrite, IntPtr outStream);
		
		public static bool VC1ConvRemovePulldown(string inFile, Stream outStream) {
			int outStreamId = AddVFileStream(outStream);
			int ret = VC1ConvRemovePulldownFileStream(inFile, StreamWrite_Func, (IntPtr)outStreamId);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(outStreamId);
			}
			return ret != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownStreamFile(StreamRead inStreamRead, IntPtr inStream, string outFile);
		
		public static bool VC1ConvRemovePulldown(Stream inStream, string outFile) {
			int inStreamId = AddVFileStream(inStream);
			int ret = VC1ConvRemovePulldownStreamFile(StreamRead_Func, (IntPtr)inStreamId, outFile);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(inStreamId);
			}
			return ret != 0;
		}
		
		[DllImport("vc1conv")]
		private static extern int VC1ConvRemovePulldownStreamStream(StreamRead inStreamRead, IntPtr inStream, StreamWrite outStreamWrite, IntPtr outStream);
		
		public static bool VC1ConvRemovePulldown(Stream inStream, Stream outStream) {
			int inStreamId = AddVFileStream(inStream);
			int outStreamId = AddVFileStream(outStream);
			int ret = VC1ConvRemovePulldownStreamStream(StreamRead_Func, (IntPtr)inStreamId, StreamWrite_Func, (IntPtr)outStreamId);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(inStreamId);
				VirtualFiles.Remove(outStreamId);
			}
			return ret != 0;
		}
	}
}

