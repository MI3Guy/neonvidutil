using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegFormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Conversions");
			NeAPI.Output("\tTrueHD\t=>\tWAV");
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			switch(Path.GetExtension(file).ToUpper()) {
				case ".THD":
					return new FormatType(FormatType.FormatCodecType.TrueHD);
				case ".VC1":
					return new FormatType(FormatType.FormatCodecType.VC1);
			}
			return null;
		}
		
		public override bool IsRawCodec (FormatType type)
		{
			return new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD).Equals(type);
		}
		
		public override FormatType[] OutputTypes(FormatType input, NeonOptions settings) {
			IEnumerable<FormatType> ret = from setting in ffmpegSettings
					where setting.inFormatType.Equals(input)
					select setting.outFormatType;
			if(ret.Count() == 0) return null;
			return ret.ToArray();
		}
		
		private static readonly FFMpegSetting[] ffmpegSettings = new FFMpegSetting[] {
			new FFMpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "truehd", outFormatName = "wav", codecName = "pcm_s24le"
			}
		};
		
		public override object HandlesConversion(FormatType input, FormatType output, NeonOptions settings) {
			foreach(FFMpegSetting setting in ffmpegSettings) {
				if(setting.inFormatType.Equals(input) && setting.outFormatType.Equals(output)) {
					return setting;
				}
			}
			return null;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, NeonOptions settings) {
			FFMpegSetting setting = (FFMpegSetting)HandlesConversion(input, output, settings);
			if(setting == null) {
				return null;
			}
			
			return new FFMpegCodec(setting);
		}
	}
}

