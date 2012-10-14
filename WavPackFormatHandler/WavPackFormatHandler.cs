using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WavPackFormatHandler {
	public class WavPackFormatHandler : FormatHandler {
		public WavPackFormatHandler() {
			
		}
		
		public override void OutputHandlerInfo()
		{
			NeAPI.Output("Supported Conversions");
			NeAPI.Output("\tWAV\t=>\tWavPack");
		}
		
		public override bool IsRawCodec(FormatType type) {
			return (type.Container == FormatType.FormatContainer.WavPack || type.Container == FormatType.FormatContainer.None) && type.Codec == FormatType.FormatCodecType.WavPack;
		}
		
		public override bool IsRawCodec(FormatType type, out FormatType outtype) {
			if(IsRawCodec(type)) {
				outtype = new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack);
				return true;
			}
			
			outtype = FormatType.None;
			return false;
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			if(Path.GetExtension(file).ToUpper() == ".WV") {
				return new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack);
			}
			else if(Path.GetExtension(file).ToUpper() == ".WAV") {
				return new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM);
			}
			else {
				return FormatType.None;
			}
		}
		
		public override FormatType[] OutputTypes(FormatType input, NeonOptions settings) {
			if(input.Container == FormatType.FormatContainer.WavPack && input.Codec == FormatType.FormatCodecType.WavPack) {
				return new FormatType[] { new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM) };
			}
			else if(input.Container == FormatType.FormatContainer.Wave && input.Codec == FormatType.FormatCodecType.PCM) {
				return new FormatType[] { new FormatType(FormatType.FormatContainer.WavPack, FormatType.FormatCodecType.WavPack) };
			}
			else {
				return null;
			}
		}
		
		public override object HandlesConversion(FormatType input, FormatType output, NeonOptions settings) {
			return
				((
					(input.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.Wave) &&
					input.Codec == FormatType.FormatCodecType.PCM &&
					output.Container == FormatType.FormatContainer.WavPack &&
					output.Codec == FormatType.FormatCodecType.WavPack
				) ||
				(
					input.Container == FormatType.FormatContainer.WavPack &&
					input.Codec == FormatType.FormatCodecType.WavPack &&
					(output.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.Wave) &&
					output.Codec == FormatType.FormatCodecType.PCM
				)) ? new object() : null;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, NeonOptions settings) {
			if(
					(input.Container == FormatType.FormatContainer.None || input.Container == FormatType.FormatContainer.Wave) &&
					input.Codec == FormatType.FormatCodecType.PCM &&
					output.Container == FormatType.FormatContainer.WavPack &&
					output.Codec == FormatType.FormatCodecType.WavPack
				) {
				return new WavPackFormatEncoder();	
			}
			else {
				return new WavPackFormatDecoder();
			}
		}
		
	}
}

