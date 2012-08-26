using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FFmpegFormatHandler {
	public class FFmpegFormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Conversions");
			NeAPI.Output("\tTrueHD\t=>\tWAV");
		}
		
		public override FormatType GenerateOutputType(string file, NeonOptions settings) {
			switch(Path.GetExtension(file).ToUpper()) {
				case ".WAV":
					return new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM);
				case ".THD":
					return new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD);
				case ".VC1":
					return new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1);
				case ".M2V":
					return new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo);
			}
			return FormatType.None;
		}
		
		public override bool IsRawCodec(FormatType type) {
			return new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD).Equals(type) ||
				new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3).Equals(type) ||
				new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3).Equals(type) ||
				new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo).Equals(type);
		}
		
		public override bool IsRawCodec(FormatType type, out FormatType outtype) {
			if(new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD).Equals(type) ||
			   new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.TrueHD).Equals(type)) {
				outtype = new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD);
				return true;
			}
			else if(new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3).Equals(type) ||
			        new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.AC3).Equals(type)) {
				outtype = new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3);
				return true;
			}
			else if(new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3).Equals(type) ||
			        new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.EAC3).Equals(type)) {
				outtype = new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3);
				return true;
			}
			else if(new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS).Equals(type) ||
			        new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.DTS).Equals(type)) {
				outtype = new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS);
				return true;
			}
			else if(new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo).Equals(type) ||
			        new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.MPEGVideo).Equals(type)) {
				outtype = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo);
				return true;
			}
			else {
				outtype = FormatType.None;
				return false;
			}
		}
		
		public override FormatType[] OutputTypes(FormatType input, NeonOptions settings) {
			IEnumerable<FormatType> ret = null;
			if(input.Container == FormatType.FormatContainer.Matroska && input.Items != null) {
				if(input.Index != -1) {
					if(input.Index >= 0 && input.Index < input.Items.Length) {
						ret =
							ffmpegSettings.Where(setting => setting.inFormatType.Equals(input.Items[input.Index]))
								.Select(setting => { FormatType type = setting.outFormatType; type.Index = input.Index; return type; });
					}
				}
				else if(input.Items != null) {
					List<FormatType> ret2 = new List<FormatType>();
					for(int i = 0; i < input.Items.Length; ++i) {
						var list =
							from setting in ffmpegSettings
								where setting.inFormatType.Equals(input.Items[i])
								select setting.outFormatType;
						FormatType[] list2 = list.ToArray();
						for(int j = 0; j < list2.Length; ++j) {
							FormatType type = list2[j];
							type.Index = i;
							list2[j] = type;
						}
						ret2.AddRange(list2);
					}
					ret = ret2;
				}
			}
			else {
				ret = from setting in ffmpegSettings
						where setting.inFormatType.Equals(input)
						select setting.outFormatType;
			}
			
			if(ret == null || ret.Count() == 0) {
				return null;
			}
			else {
				return ret.ToArray();
			}
		}
		
		private static readonly FFmpegSetting[] ffmpegSettings = new FFmpegSetting[] {
			// Decoding/Encoding
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.TrueHD, FormatType.FormatCodecType.TrueHD),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "truehd", outFormatName = "wav", codecName = "pcm_s24le"
			},
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.AC3, FormatType.FormatCodecType.AC3),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "ac3", outFormatName = "wav", codecName = "pcm_s16le"
			},
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.EAC3, FormatType.FormatCodecType.EAC3),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "eac3", outFormatName = "wav", codecName = "pcm_s16le"
			},
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.DTS, FormatType.FormatCodecType.DTS),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "dts", outFormatName = "wav", codecName = "pcm_s16le"
			},
			
			// Demuxing
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1),
				outFormatType = new FormatType(FormatType.FormatContainer.VC1, FormatType.FormatCodecType.VC1),
				inFormatName = "vc1"
			},
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.None, FormatType.FormatCodecType.PCM),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "wav", outFormatName = "wav", codecName = "copy"
			},
			new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				outFormatType = new FormatType(FormatType.FormatContainer.Wave, FormatType.FormatCodecType.PCM),
				inFormatName = "wav"
			},
			/*new FFmpegSetting {
				inFormatType = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo),
				outFormatType = new FormatType(FormatType.FormatContainer.MPEG, FormatType.FormatCodecType.MPEGVideo),
				inFormatName = "mpeg", outFormatName = "mpeg2video", codecName = "copy"
			}*/
		};
		
		public override object HandlesConversion(FormatType input, FormatType output, NeonOptions settings) {
			/*foreach(FFmpegSetting setting in ffmpegSettings) {
				if(setting.inFormatType.Equals(input) && setting.outFormatType.Equals(output)) {
					return setting;
				}
			}
			if(input.Container == FormatType.FormatContainer.Matroska && input.Items != null) {
				if(input.Index != -1) {
					if(input.Index >= 0 && input.Index < input.Items.Length) {
						foreach(FFmpegSetting setting in ffmpegSettings) {
							if(setting.inFormatType.Equals(input) && setting.outFormatType.Equals(output)) {
								return setting;
							}
						}
					}
				}
				else {
					foreach(FormatType subInput in input.Items) {
						foreach(FFmpegSetting setting in ffmpegSettings) {
							if(setting.inFormatType.Equals(subInput) && setting.outFormatType.Equals(output)) {
								return setting;
							}
						}
					}
				}
			}*/
			
			var list =
				from setting in ffmpegSettings
					where setting.inFormatType.Equals(input) && setting.outFormatType.Equals(output)
					select setting;
			try {
				return new object[] { list.First(), -1 };
			}
			catch(InvalidOperationException) {}
			
			if(input.Index != -1) {
				if(input.Index >= 0 && input.Index < input.Items.Length) {
					list =
						from setting in ffmpegSettings
							where setting.inFormatType.Equals(input.Items[input.Index]) && setting.outFormatType.Equals(output)
							select setting;
					try {
						return new object[] { list.First(), -1 };
					}
					catch(InvalidOperationException) {}
				}
			}
			else {
				for(int i = 0; i < input.Items.Length; ++i) {
					list =
						from setting in ffmpegSettings
							where setting.inFormatType.Equals(input.Items[i]) && setting.outFormatType.Equals(output)
							select setting;
					try {
						return new object[] { list.First(), i };
					}
					catch(InvalidOperationException) {}
				}
			}
				
			
			return null;
		}
		
		public override FormatCodec ConvertStream(FormatType input, FormatType output, NeonOptions settings) {
			object[] rets = (object[])HandlesConversion(input, output, settings);
			if(rets == null) {
				return null;
			}
			
			FFmpegSetting setting = (FFmpegSetting)rets[0];
			//int index = (int)rets[1];
			
			return new FFmpegCodec(setting, input, output.Index);
		}
	}
}

