using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1FormatHandler : FormatHandler {
		public override FormatType GenerateOutputType(string file) {
			if(Path.GetExtension(file).ToUpper() != ".VC1") {
				return null;
			}
			
			return new FormatType(FormatType.FormatCodecType.VC1);
		}
		
		public override bool HandlesProcessing(FormatType format, string name, FormatType next) {
			if(format.Codec == FormatType.FormatCodecType.VC1 && format.Container == FormatType.FormatContainer.None) {
				return name == null || name == "removepulldown";
			}
			return false;
		}
		
		public override FormatCodec Process(FormatType input, string name, FormatType next) {
			if(input.Codec == FormatType.FormatCodecType.VC1 && input.Container == FormatType.FormatContainer.None) {
				switch(name) {
					case null:
					case "removepulldown":
						return new VC1PulldownRemover();
				}
			}
			return null;
		}
	}
}

