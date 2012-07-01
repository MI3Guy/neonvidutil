using System;
using System.IO;

namespace NeonVidUtil {
	public class FLACFormatHandler : FormatHandler {
		public FLACFormatHandler() : base() {
		}
		
		public override bool IsRawCodec(FormatType type) {
			return (type.Container == FormatType.FormatContainer.FLAC || type.Container == FormatType.FormatContainer.None) && type.Codec == FormatType.FormatCodec.FLAC;
		}
		
		public override bool IsRawCodec(FormatType type, out FormatType outtype) {
			if(IsRawCodec(type)) {
				outtype = new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodec.FLAC);
				return true;
			}
			
			outtype = null;
			return false;
		}
		
		public override FormatType GenerateOutputType(string file) {
			if(Path.GetExtension(file).ToUpper() != ".FLAC") {
				return null;
			}
			
			return new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodec.FLAC);
		}
		
		public override object HandlesConversion(FormatType input, FormatType output, string option) {
			return
				((
					(input.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.WAV) &&
					input.Codec == FormatType.FormatCodec.PCM &&
					output.Container == FormatType.FormatContainer.FLAC &&
					output.Codec == FormatType.FormatCodec.FLAC
				) ||
				(
					input.Container == FormatType.FormatContainer.FLAC &&
					input.Codec == FormatType.FormatCodec.FLAC &&
					(output.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.WAV) &&
					output.Codec == FormatType.FormatCodec.PCM
				)) ? new object() : null;
		}
	}
}

