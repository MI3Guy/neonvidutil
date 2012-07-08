using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatHandler : FormatHandler {
		public FLACFormatHandler() {
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
		
		public override FormatType[] OutputTypes(FormatType input) {
			if(input.Container == FormatType.FormatContainer.FLAC && input.Codec == FormatType.FormatCodec.FLAC) {
				return null;//new FormatType[] { new FormatType(FormatType.FormatContainer.WAV, FormatType.FormatCodec.PCM) };
			}
			else if(input.Container == FormatType.FormatContainer.WAV && input.Codec == FormatType.FormatCodec.PCM) {
					return new FormatType[] { new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodec.FLAC) };
				}
				else {
					return null;
				}
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
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, string option) {
			return new FLACFormatEncoder();
		}
	}
}

