using System;
using System.Collections.Generic;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVFormatHandler : ProcessFormatHandler {
		public WAVFormatHandler() {
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".WAV", new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM));
		}
		
		private Dictionary<string, FormatType> outputTypes;
		public override Dictionary<string, FormatType> OutputTypes {
			get {
				return outputTypes;
			}
		}
		
		public override IEnumerable<ProcessingInfo> Processes {
			get {
				return new ProcessingInfo[] {
					new ProcessingInfo {
						HandledType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
						Description = "WAV Strip Bits"
					}
				};
			}
		}

		public override bool HandlesProcessing(FormatType format, FormatType next) {
			if(base.HandlesProcessing(format, next)) {
				string depth = NeAPI.Settings[this, "bitdepth"];
				int bitDepth;
				return depth.ToUpper() == "AUTO" || int.TryParse(depth, out bitDepth);
			}
			else {
				return false;
			}
		}
		
		public override FormatCodec Process(FormatType input, FormatType next) {
			if(HandlesProcessing(input, next)) {
				return new WAVStripBits(NeAPI.Settings[this, "bitdepth"]);
			}
			else {
				return null;
			}
		}
	}
}

