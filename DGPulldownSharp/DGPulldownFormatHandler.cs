using System;
using System.Linq;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.DGPulldownFormatHandler {
	public class DGPulldownFormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Processing");
			NeAPI.Output("\tMPEG-2\t:\tPulldown Removal");
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			if(new string[] { ".MPG", ".MPEG", ".M2V" }.Contains(Path.GetExtension(file).ToUpper())) {
				return new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo);
			}
			else {
				return FormatType.None;
			}
		}
		
		public override bool HandlesProcessing(FormatType format, NeonOptions settings, FormatType next) {
			if(format.Codec == FormatType.FormatCodecType.MPEGVideo && format.Container == FormatType.FormatContainer.MPEG) {
				return NeonOptions.GetBoolValue(settings[this, "removepulldown"]);
			}
			else {
				return false;
			}
		}
		
		public override FormatCodec Process(FormatType input, NeonOptions settings, FormatType next) {
			if(HandlesProcessing(input, settings, next)) {
				return new DGPulldownRemover();
			}
			else {
				return null;
			}
		}
	}
}

