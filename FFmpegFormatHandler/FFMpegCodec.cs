using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFMpegCodec : FormatCodec {
		public FFMpegCodec(FFMpegSetting setting) {
			this.setting = setting;
		}
		
		FFMpegSetting setting;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new MemoryStream();
			}
			else {
				return File.OpenRead(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			string inFileName = null;
			string outFileName = null;
			if(inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				inFileName = fs.Name;
				fs.Close();
			}
			
			if(outbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				outFileName = fs.Name;
				fs.Close();
			}
			
			if(inFileName == null && outFileName == null) {
				FFmpegConvert.ConvertFFmpeg(inbuff, setting.inFormatName, outbuff, setting.outFormatName, setting.codecName);
			}
			else if(inFileName == null /* && outFileName != null */) {
				
			}
			else if(/*inFileName != null && */outFileName == null) {
				
			}
			else /*if(inFileName != null && outFileName != null)*/ {
				FFmpegConvert.ConvertFFmpeg(inFileName, setting.inFormatName, outFileName, setting.outFormatName, setting.codecName);
			}
		}
	}
}

