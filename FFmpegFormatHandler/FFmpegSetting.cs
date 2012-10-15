using System;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegSetting : ConversionInfo {
		public string inFormatName;
		public string outFormatName;
		public string codecName;
		
		public override ConversionInfo Clone()
		{
			return CloneHelper(new FFmpegSetting {
				inFormatName = this.inFormatName,
				outFormatName = this.outFormatName,
				codecName = this.codecName
			});
		}
	}
}

