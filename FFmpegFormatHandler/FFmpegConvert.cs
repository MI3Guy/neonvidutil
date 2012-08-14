using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegConvert {
		static FFmpegConvert() {
			InitFFmpeg();
		}
		
		public FFmpegConvert() {
			
		}
		
		public static void Test() {
			// Will ensure that DLL loaded.
		}
		
		private Stream inStream;
		private Stream outStream;
		
		private delegate int FFmpegURLRead(IntPtr h, IntPtr buf, int size);
		private delegate int FFmpegURLWrite(IntPtr h, IntPtr buf, int size);
		
		private int FFmpegURLRead_Func(IntPtr h, IntPtr buf, int size) {
			byte[] data = new byte[size];
			int len = inStream.Read(data, 0, size);
			if(len == 0) {
				return FFmpegGetEOF();
			}
			System.Runtime.InteropServices.Marshal.Copy(data, 0, buf, size);
			return len;
		}
		
		private int FFmpegURLWrite_Func(IntPtr h, IntPtr buf, int size) {
			byte[] data = new byte[size];
			System.Runtime.InteropServices.Marshal.Copy(buf, data, 0, size);
			outStream.Write(data, 0, size);
			return size;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern void InitFFmpeg();
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegFileFile(string inFile, string inFormatName, 
		                                                 string outFile, string outFormat,
		                                                 string codecName, int streamIndex);
		
		public bool Convert(string inFile, string inFormatName, 
		                    string outFile, string outFormat,
		                    string codecName, int streamIndex) {
			return ConvertFFmpegFileFile(inFile, inFormatName, outFile, outFormat, codecName, streamIndex);
		}
		
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegFileStream(string inFile, string inFormatName,
		                                                   FFmpegURLWrite outStreamWrite, string outFormatName,
		                                                   string codecName, int streamIndex);
		
		public bool Convert(string inFile, string inFormatName,
		                    Stream outStream, string outFormatName,
		                    string codecName, int streamIndex) {
			FFmpegURLWrite write = new FFmpegURLWrite(FFmpegURLWrite_Func);
			this.outStream = outStream;
			
			bool ret = ConvertFFmpegFileStream(inFile, inFormatName, write, outFormatName, codecName, streamIndex);
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegStreamFile(FFmpegURLRead inStreamRead, string inFormatName,
		                                                   string outFile, string outFormatName,
		                                                   string codecName, int streamIndex);
		
		
		public bool Convert(Stream inStream, string inFormatName,
		                   string outFile, string outFormatName,
		                   string codecName, int streamIndex) {
			FFmpegURLRead read = new FFmpegURLRead(FFmpegURLRead_Func);
			this.inStream = inStream;
			bool ret = ConvertFFmpegStreamFile(read, inFormatName, outFile, outFormatName, codecName, streamIndex);
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool ConvertFFmpegStreamStream(FFmpegURLRead inStreamRead, string inFormatName,
		                                                     FFmpegURLWrite outStreamWrite, string outFormatName,
		                                                     string codecName, int streamIndex);
		
		public bool Convert(Stream inStream, string inFormatName,
		                   Stream outStream, string outFormatName,
		                   string codecName, int streamIndex) {
			FFmpegURLRead read = new FFmpegURLRead(FFmpegURLRead_Func);
			FFmpegURLWrite write = new FFmpegURLWrite(FFmpegURLWrite_Func);
			this.inStream = inStream;
			this.outStream = outStream;
			bool ret = ConvertFFmpegStreamStream(read, inFormatName, write, outFormatName, codecName, streamIndex);
			return ret;
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool FFmpegDemuxFileFile(string inFile, string inFormatName,
		                                               string outFile,
		                                               int streamIndex);
		
		public bool Demux(string inFile, string inFormatName,
		                  string outFile,
		                  int streamIndex) {
			return FFmpegDemuxFileFile(inFile, inFormatName, outFile, streamIndex);
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern bool FFmpegDemuxStreamStream(FFmpegURLRead inStreamRead, string inFormatName,
		                                                   FFmpegURLWrite outStreamWrite,
		                                                   int streamIndex);
		
		public bool Demux(Stream inStream, string inFormatName, Stream outStream, int streamIndex) {
			FFmpegURLRead read = new FFmpegURLRead(FFmpegURLRead_Func);
			FFmpegURLWrite write = new FFmpegURLWrite(FFmpegURLWrite_Func);
			this.inStream = inStream;
			this.outStream = outStream;
			return FFmpegDemuxStreamStream(read, inFormatName, write, streamIndex);
		}
		
		[DllImport("ffmpeg-convert")]
		private static extern int FFmpegGetEOF();
	}
}

