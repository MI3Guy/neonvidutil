using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegFormatHandler : ConversionFormatHandler {
		public FFmpegFormatHandler() {
			rawFormats = new Dictionary<FormatType, FormatType>();
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.TrueHD),
			               new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD));
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.AC3),
			               new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3));
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.EAC3),
			               new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3));
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.DTS),
			               new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS));
			rawFormats.Add(new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.MPEGVideo),
			               new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo));
			
			
			outputTypes = new Dictionary<string, FormatType>();
			outputTypes.Add(".WAV", new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM));
			outputTypes.Add(".THD", new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD));
			outputTypes.Add(".VC1", new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1));
			outputTypes.Add(".M2V", new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo));
			
			ffmpegSettings = new FFmpegSetting[] {
				// Decoding/Encoding
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "truehd", outFormatName = "wav", codecName = "pcm_s24le"
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "ac3", outFormatName = "wav", codecName = "pcm_s16le"
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "eac3", outFormatName = "wav", codecName = "pcm_s16le"
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "dts", outFormatName = "wav", codecName = "pcm_s16le"
				},
				
				// Demuxing
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1),
					OutFormatType = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1),
					inFormatName = "vc1"
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.PCM),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "wav", outFormatName = "wav", codecName = "copy"
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					inFormatName = "wav"
				},
				/*new FFmpegSetting {
					inFormatType = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo),
					outFormatType = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo),
					inFormatName = "mpeg", outFormatName = "mpeg2video", codecName = "copy"
				}*/
			};
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
		
		private FFmpegSetting[] ffmpegSettings;
		public override IEnumerable<ConversionInfo> Conversions {
			get {
				return ffmpegSettings;
			}
		}
		
		public override IEnumerable<FormatType.FormatContainer> ConversionContainers {
			get {
				return new FormatType.FormatContainer[] {
					FormatType.FormatContainer.Matroska
				};
			}
		}
		
		public override FormatCodec ConvertStream(ConversionInfo conversion) {
			ConversionInfo updatedConversionInfo;
			if(!HandlesConversion(conversion, out updatedConversionInfo)) {
				return null;
			}
			
			FFmpegSetting setting = (FFmpegSetting)updatedConversionInfo;
			
			return new FFmpegCodec(setting);
		}
	}
}

