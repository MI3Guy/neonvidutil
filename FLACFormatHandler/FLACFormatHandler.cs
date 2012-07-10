using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatHandler : FormatHandler {
		public FLACFormatHandler() {
		}
		
		public override bool IsRawCodec(FormatType type) {
			return (type.Container == FormatType.FormatContainer.FLAC || type.Container == FormatType.FormatContainer.None) && type.Codec == FormatType.FormatCodecType.FLAC;
		}
		
		public override bool IsRawCodec(FormatType type, out FormatType outtype) {
			if(IsRawCodec(type)) {
				outtype = new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC);
				return true;
			}
			
			outtype = null;
			return false;
		}
		
		public override FormatType GenerateOutputType(string file) {
			if(Path.GetExtension(file).ToUpper() != ".FLAC") {
				return null;
			}
			
			return new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC);
		}
		
		public override FormatType[] OutputTypes(FormatType input) {
			if(input.Container == FormatType.FormatContainer.FLAC && input.Codec == FormatType.FormatCodecType.FLAC) {
				return null;//new FormatType[] { new FormatType(FormatType.FormatContainer.WAV, FormatType.FormatCodecType.PCM) };
			}
			else if(input.Container == FormatType.FormatContainer.WAV && input.Codec == FormatType.FormatCodecType.PCM) {
					return new FormatType[] { new FormatType(FormatType.FormatContainer.FLAC, FormatType.FormatCodecType.FLAC) };
				}
				else {
					return null;
				}
		}
		
		public override object HandlesConversion(FormatType input, FormatType output, string option) {
			return
				((
					(input.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.WAV) &&
					input.Codec == FormatType.FormatCodecType.PCM &&
					output.Container == FormatType.FormatContainer.FLAC &&
					output.Codec == FormatType.FormatCodecType.FLAC
				) ||
				(
					input.Container == FormatType.FormatContainer.FLAC &&
					input.Codec == FormatType.FormatCodecType.FLAC &&
					(output.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.WAV) &&
					output.Codec == FormatType.FormatCodecType.PCM
				)) ? new object() : null;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, string option) {
			return new FLACFormatEncoder();
		}
	}
}

