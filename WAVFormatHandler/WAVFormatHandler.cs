using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVFormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Processing");
			NeAPI.Output("\tWAV\t:\tChange Bit Depth");
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			if(Path.GetExtension(file).ToUpper() != ".WAV") {
				return null;
			}
			else {
				return new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM);
			}
		}

		public override bool HandlesProcessing(FormatType format, NeonOptions settings, FormatType next) {
			if(new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM).Equals(format)) {
				string depth = settings[this, "bitdepth"];
				if(depth.ToUpper() == "AUTO") {
					return true;
				}
				else {
					return false;
				}
			}
			else {
				return false;
			}
		}
		
		public override FormatCodec Process(FormatType input, NeonOptions settings, FormatType next) {
			if(HandlesProcessing(input, settings, next)) {
				return new WAVStripBits();
			}
			else {
				return null;
			}
		}
	}
}

