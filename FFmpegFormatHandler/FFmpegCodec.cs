using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegCodec : FormatCodec {
		public FFmpegCodec(FFmpegSetting setting, FormatType input, int index) {
			this.setting = setting;
			if(input.Container == FormatType.FormatContainer.Matroska) {
				inFormatName = "matroska";
			}
			else {
				inFormatName = setting.inFormatName;
			}
			streamIndex = index;
		}
		
		FFmpegSetting setting;
		string inFormatName;
		int streamIndex;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.OpenRead(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			FFmpegConvert ffmpeg = new FFmpegConvert();
			
			if(setting.codecName != null) {
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
					ffmpeg.Convert(inbuff, inFormatName, outbuff, setting.outFormatName, setting.codecName, streamIndex);
				}
				else if(inFileName == null /* && outFileName != null */) {
					ffmpeg.Convert(inbuff, inFormatName, outFileName, setting.outFormatName, setting.codecName, streamIndex);
				}
				else if(/*inFileName != null && */outFileName == null) {
					ffmpeg.Convert(inFileName, inFormatName, outbuff, setting.outFormatName, setting.codecName, streamIndex);
				}
				else /*if(inFileName != null && outFileName != null)*/ {
					ffmpeg.Convert(inFileName, inFormatName, outFileName, setting.outFormatName, setting.codecName, streamIndex);
				}
			}
			else {
				string inFileName = null;
				string outFileName = null;
				if(inbuff is FileStream && outbuff is FileStream) {
					FileStream fs = (FileStream)inbuff;
					inFileName = fs.Name;
					fs.Close();
					fs = (FileStream)inbuff;
					outFileName = fs.Name;
					fs.Close();
				}
				
				if(inFileName == null && outFileName == null) {
					ffmpeg.Demux(inbuff, inFormatName, outbuff, streamIndex);
				}
				else {
					ffmpeg.Demux(inFileName, inFormatName, outFileName, streamIndex);
				}
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
	}
}

