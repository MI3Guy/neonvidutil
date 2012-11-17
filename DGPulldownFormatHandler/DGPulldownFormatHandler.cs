using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.DGPulldownFormatHandler {
	public class DGPulldownFormatHandler : ProcessFormatHandler {
		public DGPulldownFormatHandler() {
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".MPG", new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo));
			outputTypes.Add(".MPEG", new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo));
			outputTypes.Add(".M2V", new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo)); 
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
						HandledType = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo),
						Description = "MPEG-2 Pulldown Removal"
					}
				};
			}
		}
		
		public override bool HandlesProcessing(FormatType format, FormatType next) {
			if(base.HandlesProcessing(format, next)) {
				return NeonOptions.GetBoolValue(NeAPI.Settings[this, "removepulldown"]);
			}
			else {
				return false;
			}
		}
		
		public override FormatCodec Process(FormatType input, FormatType next) {
			if(HandlesProcessing(input, next)) {
				return new DGPulldownRemover();
			}
			else {
				return null;
			}
		}
	}
}

