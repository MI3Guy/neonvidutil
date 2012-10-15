using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegCodec : FormatCodec {
		public FFmpegCodec(FFmpegSetting setting) {
			this.setting = setting;
			if(setting.InFormatType.Container == FormatType.FormatContainer.Matroska) {
				inFormatName = "matroska";
			}
			else {
				inFormatName = setting.inFormatName;
			}
		}
		
		FFmpegSetting setting;
		string inFormatName;
		int streamIndex;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			FFmpegConvert ffmpeg = new FFmpegConvert();
			
			Action callback = () => NeAPI.ProgressBar(progressId, inbuff);
			
			if(setting.codecName != null) {
				/*string inFileName = null;
				string outFileName = null;
				if(inbuff is FileStream) {
					FileStream fs = (FileStream)inbuff;
					inFileName = fs.Name;
					fs.Close();
				}
				
				if(outbuff is FileStream) {
					FileStream fs = (FileStream)outbuff;
					outFileName = fs.Name;
					fs.Close();
				}*/
				
				bool errorCode;
				//if(inFileName == null && outFileName == null) {
					errorCode = ffmpeg.Convert(inbuff, inFormatName, outbuff, setting.outFormatName, setting.codecName, streamIndex, callback);
				//}
				//else if(inFileName == null /* && outFileName != null */) {
				//	errorCode = ffmpeg.Convert(inbuff, inFormatName, outFileName, setting.outFormatName, setting.codecName, streamIndex, callback);
				//}
				//else if(/*inFileName != null && */outFileName == null) {
				//	errorCode = ffmpeg.Convert(inFileName, inFormatName, outbuff, setting.outFormatName, setting.codecName, streamIndex, callback);
				//}
				//else /*if(inFileName != null && outFileName != null)*/ {
				//	errorCode = ffmpeg.Convert(inFileName, inFormatName, outFileName, setting.outFormatName, setting.codecName, streamIndex, callback);
				//}
				
				if(!errorCode) {
					//TODO: Throw exception.
					System.Diagnostics.Debugger.Break();
				}
			}
			else {
				//string inFileName = null;
				//string outFileName = null;
				/*if(inbuff is FileStream && outbuff is FileStream) {
					FileStream fs = (FileStream)inbuff;
					inFileName = fs.Name;
					fs.Close();
					fs = (FileStream)inbuff;
					outFileName = fs.Name;
					fs.Close();
				}*/
				
				//if(inFileName == null && outFileName == null) {
					ffmpeg.Demux(inbuff, inFormatName, outbuff, streamIndex, callback);
				//}
				//else {
				//	ffmpeg.Demux(inFileName, inFormatName, outFileName, streamIndex, callback);
				//}
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
		
		public override string DisplayValue {
			get {
				string fromStr = setting.InFormatType.CodecString;
				/*if(!PluginHelper.AutoIsRawCodec(setting.inFormatType)) {
					fromStr = setting.inFormatType.ContainerString + ":" + fromStr;
				}*/
				
				string toStr = setting.OutFormatType.Codec.ToString();
				/*if(!PluginHelper.AutoIsRawCodec(setting.inFormatType)) {
					toStr = setting.outFormatType.ContainerString + ":" + toStr;
				}*/
				
				return fromStr + "\t=>\t" + toStr;
			}
		}
	}
}

