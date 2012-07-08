using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVFormatHandler : FormatHandler {
		public override FormatType GenerateOutputType(string file) {
			if(Path.GetExtension(file).ToUpper() != ".WAV") {
				return null;
			}
			
			return new FormatType(FormatType.FormatContainer.WAV, FormatType.FormatCodec.PCM);
		}

		public override bool HandlesProcessing(FormatType format, string name, FormatType next) {
			return false;//return name == null || name == "stripbits";
		}
		
		public override FormatCodec Process(FormatType input, string name, FormatType next) {
			switch(name) {
				case null:
				case "stripbits":
					return new WAVStripBits(next);
			}
			
			return null;
		}
	}
}

