using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegFormatHandler : ConversionFormatHandler {
		public FFmpegFormatHandler() {
			FFmpegConvert.LoadFFmpegConvert();
			
			rawFormats = new Dictionary<FormatType, FormatType>
			{
				{
					new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.TrueHD),
			               new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD)
				},
				{
					new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.AC3),
			               new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3)
				},
				{
					new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.EAC3),
			               new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3)
				},
				{
					new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.DTS),
			               new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS)
				},
				{
					new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.MPEGVideo),
			               new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo)
				}
			};

			outputTypes = new Dictionary<string, FormatType>
			{
				{ ".WAV", new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM) },
				{ ".THD", new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD) },
				{ ".VC1", new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1) },
				{ ".M2V", new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo) },
				{ ".AC3", new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3) },
				{ ".EAC3", new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3) }
			};
			
			ffmpegSettings = new FFmpegSetting[] {
				// Decoding/Encoding
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					InFormatName = "truehd", OutFormatName = "wav", CodecName = "pcm_s24le",
					Flags = ConversionInfo.ConversionFlags.None
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					InFormatName = "ac3", OutFormatName = "wav", CodecName = "pcm_s16le",
					Flags = ConversionInfo.ConversionFlags.None
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					InFormatName = "eac3", OutFormatName = "wav", CodecName = "pcm_s16le",
					Flags = ConversionInfo.ConversionFlags.None
				},
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					InFormatName = "dts", OutFormatName = "wav", CodecName = "pcm_s16le",
					Flags = ConversionInfo.ConversionFlags.None
				},

				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					OutFormatType = new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3),
					InFormatName = "wav", OutFormatName = "ac3", CodecName = "ac3",
					Flags = ConversionInfo.ConversionFlags.Lossy
				},

				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					OutFormatType = new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3),
					InFormatName = "wav", OutFormatName = "eac3", CodecName = "eac3",
					Flags = ConversionInfo.ConversionFlags.Lossy
				},

				// Demuxing
				new FFmpegSetting {
					InFormatType = new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.PCM),
					OutFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
					InFormatName = "wav", OutFormatName = "wav", CodecName = "copy",
					Flags = ConversionInfo.ConversionFlags.None
				}
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

