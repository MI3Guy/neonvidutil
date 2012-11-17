using System;
using System.Collections.Generic;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WavPackFormatHandler {
	public class WavPackFormatHandler : ConversionFormatHandler {
		public WavPackFormatHandler() {
			rawFormats = new Dictionary<FormatType, FormatType>();
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.WavPack),
			               new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack));
			
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".WAV", new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM));
			outputTypes.Add(".WV", new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack));
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
		
		public override IEnumerable<ConversionInfo> Conversions {
			get {
				return new ConversionInfo[] {
					new ConversionInfo {
						InFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
						OutFormatType = new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack),
						Flags = ConversionInfo.ConversionFlags.None
					},
					/*new ConversionInfo {
						InFormatType = new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack),
						OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
						Flags = ConversionInfo.ConversionFlags.None
					}*/
				};
			}
		}
		
		public override FormatCodec ConvertStream(ConversionInfo conversion) {
			if(!HandlesConversion(conversion, out conversion)) {
				return null;
			}
			
			if(conversion.OutFormatType.Codec == FormatType.FormatCodecType.WavPack) {
				return new WavPackFormatEncoder();	
			}
			else {
				return new WavPackFormatDecoder();
			}
		}
		
	}
}

