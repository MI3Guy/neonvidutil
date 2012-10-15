using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NeonVidUtil.Core {
	public abstract class ConversionFormatHandler : FormatHandler {
		public sealed override IEnumerable<ProcessingInfo> Processes {
			get {
				return base.Processes;
			}
		}
		
		public sealed override FormatType ReadFileInfo(string file) {
			return FormatType.None;
		}
		
		
		public override IEnumerable<ConversionInfo> FindConversionTypes(FormatType input) {
			foreach(ConversionInfo conv in Conversions) {
				if(conv.InFormatType.Equals(input)) {
					yield return conv;
				}
				
				if(ConversionContainers.Contains(input.Container) &&
				   input.CodecString == conv.InFormatType.CodecString) {
					ConversionInfo outconv = conv.Clone();
					outconv.InFormatType = input;
					yield return outconv;
				}
			}
			
			int streamIndex;
			if(!int.TryParse(NeAPI.Settings["Core", "streamIndex"], out streamIndex)) {
				streamIndex = -1;
			}
			
			if(streamIndex == -1) {
				foreach(ConversionInfo conv in Conversions) {
					if(ConversionContainers.Contains(input.Container)) {
						for(int i = 0; i < input.Items.Length; ++i) {
							if(conv.InFormatType.Equals(input.Items[i])) {
								ConversionInfo outconv = conv.Clone();
								outconv.InFormatType = input;
								outconv.StreamIndex = i;
								yield return outconv;
							}
						}
					}
				}
			}
			else {
				foreach(ConversionInfo conv in Conversions) {
					if(ConversionContainers.Contains(input.Container) &&
					   streamIndex >= 0 && streamIndex < input.Items.Length &&
					   conv.InFormatType.Equals(input.Items[streamIndex])) {
						ConversionInfo outconv = conv.Clone();
						outconv.InFormatType = input;
						outconv.StreamIndex = streamIndex;
						yield return outconv;
					}
				}
			}
		}
		
		public override bool HandlesConversion(ConversionInfo conversion, out ConversionInfo updatedConversion) {
			foreach(ConversionInfo conv in Conversions) {
				if(conv.Equals(conversion)) {
					updatedConversion = conversion;
					return true;
				}
				
				if(ConversionContainers.Contains(conversion.InFormatType.Container) &&
				   conv.InFormatType.CodecString == conversion.InFormatType.CodecString &&
				   conv.OutFormatType.Equals(conversion.OutFormatType)) {
					updatedConversion = conversion;
					return true;
				}
			}
			
			if(conversion.StreamIndex == -1) {
				foreach(ConversionInfo conv in Conversions) {
					if(conv.OutFormatType.Equals(conversion.OutFormatType) && ConversionContainers.Contains(conversion.InFormatType.Container)) {
						for(int i = 0; i < conversion.InFormatType.Items.Length; ++i) {
							if(conv.InFormatType.Equals(conversion.InFormatType.Items[i])) {
								ConversionInfo outconv = conversion.Clone();
								outconv.StreamIndex = i;
								updatedConversion = outconv;
								return true;
							}
						}
					}
				}
			}
			else {
				foreach(ConversionInfo conv in Conversions) {
					if(conv.OutFormatType.Equals(conversion.OutFormatType) &&
					   ConversionContainers.Contains(conversion.InFormatType.Container) &&
					   conversion.StreamIndex >= 0 && conversion.StreamIndex < conversion.InFormatType.Items.Length &&
					   conv.InFormatType.Equals(conversion.InFormatType.Items[conversion.StreamIndex])) {
						updatedConversion = conversion;
						return true;
					}
				}
			}
			
			updatedConversion = conversion;
			return false;
		}
		
		
		
		public sealed override bool HandlesProcessing(FormatType format, FormatType next) {
			return false;
		}
		
		public sealed override FormatCodec Process(FormatType input, FormatType next) {
			return null;
		}
	}
}

