using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1FormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Processing");
			NeAPI.Output("\tVC1\t:\tPulldown Removal");
		}
		
		public override bool IsRawCodec(FormatType type) {
			return (type.Container == FormatType.FormatContainer.VC1 || type.Container == FormatType.FormatContainer.None) && type.Codec == FormatType.FormatCodecType.VC1;
		}
		
		public override bool IsRawCodec(FormatType type, out FormatType outtype) {
			if(IsRawCodec(type)) {
				outtype = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1);
				return true;
			}
			else {
				outtype = FormatType.None;
				return false;
			}
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			if(Path.GetExtension(file).ToUpper() != ".VC1") {
				return FormatType.None;
			}
			else {
				return new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1);
			}
		}
		
		public override bool HandlesProcessing(FormatType format, NeonOptions settings, FormatType next) {
			if(format.Codec == FormatType.FormatCodecType.VC1 && format.Container == FormatType.FormatContainer.VC1) {
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

