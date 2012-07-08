using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFMpegFormatHandler {
	public class FFMpegCodec : FormatCodec {
		public FFMpegCodec(FFMpegSetting setting) {
			this.setting = setting;
		}
		
		FFMpegSetting setting;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			string fname = UseTempFile(inbuff, setting.inext);
			
			if(outfile == null) {
				outfile = CreateTempFileName(setting.outext);
			}
			
			string cmd = string.Format(setting.cmdline, fname, outfile);
			Console.WriteLine("Running: ffmpeg {0}", cmd);
			Process proc = Process.Start("ffmpeg", cmd);
			proc.WaitForExit();
			
			return File.OpenRead(outfile);
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			
		}
	}
}

