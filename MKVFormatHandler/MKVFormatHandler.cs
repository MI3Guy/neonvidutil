using System;
using System.Collections.Generic;
using NeonVidUtil.Core;
using System.Linq;

namespace NeonVidUtil.Plugin.MKVFormatHandler {
	public class MKVFormatHandler : ConversionFormatHandler {
		public MKVFormatHandler() {
			
		}
		
		public override IEnumerable<ConversionInfo> Conversions {
			get {
				return new ConversionInfo[] {
					new ConversionInfo {
						InFormatType = new FormatType(FormatType.FormatContainer.Matroska, FormatType.FormatCodecType.None),
						OutFormatType = new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.None)
					}
				};
			}
		}
		
		public override bool HandlesConversion(ConversionInfo conversion, out ConversionInfo updatedConversion) {
			if(conversion.InFormatType.Container != FormatType.FormatContainer.Matroska || !conversion.OutFormatType.IsRawContainer()) {
				updatedConversion = conversion;
				return false;
			}
			
			if(conversion.StreamIndex == -1) {
				int streamIndex;
				if(int.TryParse(NeAPI.Settings["Core", "streamIndex"], out streamIndex)) {
					updatedConversion = conversion.Clone();
					updatedConversion.StreamIndex = streamIndex;
					return true;
				}
				else {
					for(int i = 0; i < conversion.InFormatType.Items.Length; ++i) {
						if(conversion.InFormatType.Items[i].Codec == conversion.OutFormatType.Codec) {
							try {
								updatedConversion = conversion.Clone();
								updatedConversion.StreamIndex = i;
								return true;
							}
							catch {
							}
 						}
 					}
					
					updatedConversion = conversion;
					return false;
				}
			}
			else {
				updatedConversion = conversion;
				return true;
			}
			return false;
		}
		
		public override IEnumerable<ConversionInfo> FindConversionTypes(FormatType inputID) {
			if(inputID.Container != FormatType.FormatContainer.Matroska) {
				return null;
			}
			
			return inputID.Items.Select((item, index) => new ConversionInfo { InFormatType = inputID, OutFormatType = item, StreamIndex = index });
		}
		
		public override FormatCodec ConvertStream(ConversionInfo conversion) {
			ConversionInfo updConv;
			if(!HandlesConversion(conversion, out updConv)) {
				return null;
 			}
			
			int index = updConv.InFormatType.Items[updConv.StreamIndex].ID;
			return new MKVFormatDecoder(index);
		}
	}
}

