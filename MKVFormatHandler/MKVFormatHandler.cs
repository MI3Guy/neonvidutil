using System;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.MKVFormatHandler {
	public class MKVFormatHandler : FormatHandler {
		public MKVFormatHandler() {
			
		}
		
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Conversions");
			NeAPI.Output("\tMKV:???\t=>\t???");
		}
		
		public override object HandlesConversion(FormatType input, FormatType output, NeonOptions settings) {
			if(input.Container != FormatType.FormatContainer.Matroska || !output.IsRawContainer()) {
				return false;
			}
			
			if(input.Index == -1) {
				for(int i = 0; i < input.Items.Length; ++i) {
					if(input.Items[i].Codec == output.Codec) {
						try {
							return input.Items[i].ID;
						}
						catch {
						}
					}
				}
			}
			else {
				return input.Items[input.Index].ID;
			}
			return null;
		}
		
		public override FormatType[] OutputTypes(FormatType inputID, NeonOptions settings) {
			if(inputID.Container != FormatType.FormatContainer.Matroska) {
				return null;
			}
			if(inputID.Index == -1) {
				return inputID.Items;
			}
			else {
				return new FormatType[] { inputID.Items[inputID.Index] };
			}
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, NeonOptions settings) {
			int index = output.Index;
			if(input.Index != -1) {
				index = input.Items[input.Index].ID;
			}
			return new MKVFormatDecoder(index);
		}
	}
}

