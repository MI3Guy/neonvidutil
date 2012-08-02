using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public static class FFmpegConvert {
		static FFmpegConvert() {
			InitFFmpeg();
		}
		
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
		
		[DllImport("ffmpeg-convert")]
		private static extern void InitFFmpeg();
		
		[DllImport("ffmpeg-convert", EntryPoint = "ConvertFFmpegFileFile")]
		public static extern bool ConvertFFmpeg(string inFile, string inFormatName, 
		                                                string outFile, string outFormat,
		                                                string codecName);
		
		
		private delegate int FFmpegURLRead(IntPtr h, IntPtr buf, int size);
		private delegate int FFmpegURLWrite(IntPtr h, IntPtr buf, int size);
		
		private static int FFmpegURLRead_Func(IntPtr h, IntPtr buf, int size) {
			byte[] data = new byte[size];
			int len = VirtualFiles[(int)h].Read(data, 0, size);
			if(len == 0) {
				return FFmpegGetEOF();
			}
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buf, size);
			return len;
		}
		
		private static int FFmpegURLWrite_Func(IntPtr h, IntPtr buf, int size) {
			byte[] data = new byte[size];
			System.Runtime.InteropServices.Marshal.Copy(buf, data, 0, size);
			VirtualFiles[(int)h].Write(data, 0, size);
			return size;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegFileStream(string inFile, string inFormatName,
		                                                   FFmpegURLWrite outStreamWrite, int outFid, string outFormatName,
		                                                   string codecName);
		
		public static bool ConvertFFmpeg(string inFile, string inFormatName,
		                                 Stream outStream, string outFormatName,
		                                 string codecName) {
			int outStreamId = AddVFileStream(outStream);
			bool ret = ConvertFFmpegFileStream(inFile, inFormatName, FFmpegURLWrite_Func, outStreamId, outFormatName, codecName);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(outStreamId);
			}
			
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegStreamFile(FFmpegURLRead inStreamRead, int inFid, string inFormatName,
		                                                   string outFile, string outFormatName,
		                                                   string codecName);
		
		
		public static bool ConvertFFmpeg(Stream inStream, string inFormatName,
		                                 string outFile, string outFormatName,
		                                 string codecName) {
			int inStreamId = AddVFileStream(inStream);
			bool ret = ConvertFFmpegStreamFile(FFmpegURLRead_Func, inStreamId, inFormatName, outFile, outFormatName, codecName);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(inStreamId);
			}
			
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegStreamStream(FFmpegURLRead inStreamRead, int inFid, string inFormatName,
		                                                     FFmpegURLWrite outStreamWrite, int outFid, string outFormatName,
		                                                     string codecName);
		
		public static bool ConvertFFmpeg(Stream inStream, string inFormatName,
		                                 Stream outStream, string outFormatName,
		                                 string codecName) {
			int inStreamId = AddVFileStream(inStream);
			int outStreamId = AddVFileStream(outStream);
			bool ret = ConvertFFmpegStreamStream(FFmpegURLRead_Func, inStreamId, inFormatName, FFmpegURLWrite_Func, outStreamId, outFormatName, codecName);
			
			lock(VirtualFiles) {
				VirtualFiles.Remove(inStreamId);
				VirtualFiles.Remove(outStreamId);
			}
			
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern int FFmpegGetEOF();
	}
}

