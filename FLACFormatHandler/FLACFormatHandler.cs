using System;
using System.Collections.Generic;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatHandler : ConversionFormatHandler {
		public FLACFormatHandler() {
			rawFormats = new Dictionary<FormatType, FormatType>();
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.FLAC),
			               new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC));
			
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".FLAC", new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC));
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
						OutFormatType = new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC)
					}
				};
			}
		}
		
		public override FormatCodec ConvertStream(ConversionInfo conversion) {
			return new FLACFormatEncoder();
		}
	}
}

