using System;
using System.Runtime.InteropServices;

namespace FFMpegFormatHandler {
	public static class FFmpegConvert {
		//bool ConvertFFmpegFile(char* inFile, char* inFormat, char* outFile, char* outFormat, char* codec);
		[DllImport("ffmpeg-convert")]
		public static extern bool ConvertFFmpegFile(string inFile, string inFormatName,
		                                            string outFile, string outFormat,
		                                            string codecName);
		
		//typedef int (*FFmpegURLRead)(URLContext* h, unsigned char* buf, int size);
		//typedef int (*FFmpegURLWrite)(URLContext* h, const unsigned char* buf, int size);
		public delegate int FFmpegURLRead(IntPtr h, IntPtr buf, int size);
		public delegate int FFmpegURLWrite(IntPtr h, IntPtr buf, int size);
		
		[DllImport("ffmpeg-convert")]
		public static extern bool ConvertFFmpegStream(FFmpegURLRead inStreamRead, string inFormatName,
		                                              FFmpegURLWrite outStreamWrite, string outFormatName,
		                                              string codecName);
	}
}

