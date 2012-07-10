using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFMpegFormatHandler {
	public class FFMpegFormatHandler : FormatHandler {
		public override FormatType GenerateOutputType(string file) {
			switch(Path.GetExtension(file).ToUpper()) {
				case ".THD":
					return new FormatType(FormatType.FormatCodecType.TrueHD);
			}
			return null;
		}
		
		public override FormatType[] OutputTypes(FormatType input) {
			IEnumerable<FormatType> ret = from setting in ffmpegSettings
					where setting.inFormatType.Equals(input)
					select setting.outFormatType;
			if(ret.Count() == 0) return null;
			return ret.ToArray();
		}
		
		private static readonly FFMpegSetting[] ffmpegSettings = new FFMpegSetting[] {
			new FFMpegSetting {
				inFormatType = new FormatType(FormatType.FormatCodecType.TrueHD),
				outFormatType = new FormatType(FormatType.FormatContainer.WAV, FormatType.FormatCodecType.PCM),
				cmdline = "-f truehd -i \"{0}\" -y -acodec pcm_s24le -f wav \"{1}\"",
			}
		};
		
		public override object HandlesConversion(FormatType input, FormatType output, string option) {
			foreach(FFMpegSetting setting in ffmpegSettings) {
				if(setting.inFormatType.Equals(input) && setting.outFormatType.Equals(output)) {
					return setting;
				}
			}
			return null;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, string option) {
			FFMpegSetting setting = (FFMpegSetting)HandlesConversion(input, output, option);
			if(setting == null) {
				return null;
			}
			
			return new FFMpegCodec(setting);
		}
	}
}

