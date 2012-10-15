using System;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public abstract class InfoFormatHandler : FormatHandler {
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
		
		public sealed override IEnumerable<ProcessingInfo> Processes {
			get {
				return base.Processes;
			}
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
		
		public sealed override bool HandlesProcessing(FormatType format, FormatType next) {
			return false;
		}
		public sealed override FormatCodec Process(FormatType input, FormatType next) {
			return null;
		}
	}
}

