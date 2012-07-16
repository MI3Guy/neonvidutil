using System;
using System.Runtime.InteropServices;

namespace FFMpegFormatHandler {
	public static class FFmpegConvert {
		//bool ConvertFFmpegFile(char* inFile, char* inFormat, char* outFile, char* outFormat, char* codec);
		[DllImport("libffmpeg-convert")]
		public static extern bool ConvertFFmpegFile(string inFile, string inFormat, string outFile, string outFormat, string codec);
	}
}

