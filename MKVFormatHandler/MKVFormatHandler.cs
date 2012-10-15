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
		
		/*public override bool HandlesConversion(ConversionInfo conversion, out ConversionInfo updatedConversion) {
			if(conversion.InFormatType.Container != FormatType.FormatContainer.Matroska || !conversion.OutFormatType.IsRawContainer()) {
				updatedConversion = conversion;
				return false;
			}
			
			if(conversion.InFormatType.Index == -1) {
				for(int i = 0; i < conversion.InFormatType.Items.Length; ++i) {
					if(conversion.InFormatType.Items[i].Codec == conversion.OutFormatType.Codec) {
						try {
							return true;
						}
						catch {
						}
					}
				}
			}
			else {
				return true;
			}
			return false;
		}*/
		
		/*public override IEnumerable<ConversionInfo> FindConversionTypes(FormatType inputID) {
			if(inputID.Container != FormatType.FormatContainer.Matroska) {
				return null;
			}
			if(inputID.Index == -1) {
				return from x in inputID.Items select new ConversionInfo { InFormatType = inputID, OutFormatType = x };
			}
			else {
				return new ConversionInfo[] {
					new ConversionInfo {
						InFormatType = inputID,
						OutFormatType = inputID.Items[inputID.Index]
					}
				};
			}
		}*/
		
		public override FormatCodec ConvertStream(ConversionInfo conversion) {
			int index = conversion.OutFormatType.Index;
			if(conversion.InFormatType.Index != -1) {
				index = conversion.InFormatType.Items[conversion.InFormatType.Index].ID;
			}
			return new MKVFormatDecoder(index);
		}
	}
}

