using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1FormatHandler : FormatHandler {
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			if(Path.GetExtension(file).ToUpper() != ".VC1") {
				return null;
			}
			else {
				return new FormatType(FormatType.FormatCodecType.VC1);
			}
		}
		
		public override bool HandlesProcessing(FormatType format, NeonOptions settings, FormatType next) {
			if(format.Codec == FormatType.FormatCodecType.VC1 && format.Container == FormatType.FormatContainer.None) {
				return NeonOptions.GetBoolValue(settings[this, "removepulldown"]);
			}
			else {
				return false;
			}
		}
		
		public override FormatCodec Process(FormatType input, NeonOptions settings, FormatType next) {
			if(HandlesProcessing(input, settings, next)) {
				return new VC1PulldownRemover();
			}
			else {
				return null;
			}
		}
	}
}

