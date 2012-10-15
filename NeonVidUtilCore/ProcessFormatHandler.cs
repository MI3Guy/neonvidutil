using System;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public abstract class ProcessFormatHandler : FormatHandler {
		public sealed override IEnumerable<ConversionInfo> Conversions {
			get {
				return base.Conversions;
			}
		}
		
		public sealed override IEnumerable<FormatType.FormatContainer> ConversionContainers {
			get {
				return base.ConversionContainers;
			}
		}
		
		public sealed override FormatType ReadFileInfo(string file) {
			return FormatType.None;
		}
		
		public sealed override IEnumerable<ConversionInfo> FindConversionTypes(FormatType input) {
			return new ConversionInfo[] {};
		}
		
		public sealed override bool HandlesConversion(ConversionInfo conversion, out ConversionInfo updatedConversion) {
			updatedConversion = conversion;
			return false;
		}
		public sealed override FormatCodec ConvertStream(ConversionInfo conversion) {
			return null;
		}
		
		public override bool HandlesProcessing(FormatType format, FormatType next) {
			foreach(ProcessingInfo proc in Processes) {
				if(proc.HandledType.Equals(format)) {
					return true;
				}
			}
			return false;
		}
	}
}

