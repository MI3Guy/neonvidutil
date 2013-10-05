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
				inFormatName = setting.InFormatName;
			}
			
			streamIndex = setting.StreamIndex;
		}

		FFmpegConvert converter;
		
		FFmpegSetting setting;
		string inFormatName;
		int streamIndex;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				converter = new FFmpegConvert(
					inStream: inbuff,
					inFormatName: inFormatName,
					outFormatName: setting.OutFormatName,
					codecName: setting.CodecName,
					bitrate: NeAPI.Settings[typeof(FFmpegFormatHandler), "bitrate"],
					streamIndex: streamIndex);
			}
			else {
				converter = new FFmpegConvert(
					inStream: inbuff,
					inFormatName: inFormatName,
					outFileName: outfile,
					outFormatName: setting.OutFormatName,
					codecName: setting.CodecName,
					bitrate: NeAPI.Settings[typeof(FFmpegFormatHandler), "bitrate"],
					streamIndex: streamIndex);
			}

			return converter.OutStream;
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			NeAPI.Output("ffmpeg command line arguments: {0}", converter.CommandLineArguments);

			if(!converter.Run(() => NeAPI.ProgressBar(progressId, inbuff))) {
				throw new Exception("An error occurred while converting.");
			}
		}
		
		public override string DisplayValue {
			get {
				string fromStr = setting.InFormatType.CodecString;
				/*if(!PluginHelper.AutoIsRawCodec(setting.inFormatType)) {
					fromStr = setting.inFormatType.ContainerString + ":" + fromStr;
				}*/
				
				string toStr = setting.OutFormatType.CodecString;
				/*if(!PluginHelper.AutoIsRawCodec(setting.inFormatType)) {
					toStr = setting.outFormatType.ContainerString + ":" + toStr;
				}*/
				
				return fromStr + "\t=>\t" + toStr;
			}
		}
	}
}

