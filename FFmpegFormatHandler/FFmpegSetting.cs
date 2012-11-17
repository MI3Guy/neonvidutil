using System;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegSetting : ConversionInfo {
		
		public string InFormatName {
			get;
			set;
		}
		
		public string OutFormatName {
			get;
			set;
		}
		
		public string CodecName
		{
			get;
			set;
		}
		
		public override ConversionInfo Clone()
		{
			return CloneHelper(new FFmpegSetting());
		}
		
		protected override ConversionInfo CloneHelper(ConversionInfo conv) {
			FFmpegSetting setting = (FFmpegSetting)conv;
			setting.InFormatName = this.InFormatName;
			setting.OutFormatName = this.OutFormatName;
			setting.CodecName = this.CodecName;
			
			return base.CloneHelper(setting);
		}
	}
}

