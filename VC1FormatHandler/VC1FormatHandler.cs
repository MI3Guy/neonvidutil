using System;
using System.Collections.Generic;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1FormatHandler : ProcessFormatHandler {
		public VC1FormatHandler() {
			rawFormats = new Dictionary<FormatType, FormatType>();
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.VC1),
			               new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1));
			
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".VC1", new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1));
		}
		
		private Dictionary<FormatType, FormatType> rawFormats;
		public override Dictionary<FormatType, FormatType> RawFormats {
			get {
				return rawFormats;
			}
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
						HandledType = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1),
						Description = "VC-1 Pulldown Removal"
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
				return new VC1PulldownRemover();
			}
			else {
				return null;
			}
		}
	}
}

