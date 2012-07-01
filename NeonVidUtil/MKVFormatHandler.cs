using System;

namespace NeonVidUtil {
	public class MKVFormatHandler : FormatHandler {
		public MKVFormatHandler() : base() {
			
		}
		
		public override object HandlesConversion(FormatType input, FormatType output, string option) {
			if(input.Container != FormatType.FormatContainer.Matroska || !output.IsRawContainer()) {
				return false;
			}
			
			int? index;
			try {
				index = int.Parse(option);
			}
			catch {
				
			}
			
			for(int i = 0; i < input.Items.Length; ++i) {
				if(input.Items[i].Codec == output.Codec) {
					try {
						if(index == null || (int)index == (int)input.Items[i].Param) {
							return input.Items[i].Param;
						}
					}
					catch {
					}
				}
			}
			return null;
		}
		
		public override FormatType[] OutputTypes(FormatType input) {
			return input.Items;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, string option) {
			object param = HandlesConversion(input, output, option);
			if(!(param is int)) {
				return null;
			}
			return new MKVFormatDecoder((int)param);
		}
	}
}

