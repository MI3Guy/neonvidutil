using System;
using System.Linq;
using System.Collections.Generic;
using MediaInfoLib;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.MediaInfoFormatHandler {
	public class MediaInfoFormatHandler : FormatHandler {
		public MediaInfoFormatHandler() : base() {
			MI = new MediaInfo();
			MI.Option("Info_Version", "0.7.0.0;NeonVidUtil_MediaInfoFormatHandler;0.0.0.1");
		}
		
		protected MediaInfo MI;
		
		public override void OutputHandlerInfo() {
			
		}
		
		public override FormatType ReadInfo(string file, NeonOptions settings) {
			MI.Open(file);
			Console.WriteLine(MI.Option("Info_Parameters"));
			
			string container = MI.Get(StreamKind.General, 0, "Format");
			List<FormatType> items = new List<FormatType>();//[MI.Count_Get(StreamKind.Video) + MI.Count_Get(StreamKind.Audio) + MI.Count_Get(StreamKind.Text)];
			
			try {
				int count = MI.Count_Get(StreamKind.Video);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Video, i, "Format");
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(!tmpft.Equals(FormatType.None)) {
						ft = tmpft;
					}
					try {
						ft.ID = int.Parse(MI.Get(StreamKind.Video, i, "ID"));
					}
					catch {}
					string[] values = { "FrameRate/String", "ScanType/String", "BitRate/String" };
					ft.ExtraInfo =
						string.Join(", ", from y in
							from x in values select MI.Get(StreamKind.Video, i, x)
							where !string.IsNullOrEmpty(y) select y);
					items.Add(ft);
				}
				
				count = MI.Count_Get(StreamKind.Audio);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Audio, i, "Format");
					if(codecid == "DTS") {
						if(MI.Get(StreamKind.Audio, i, "Format_Profile") == "MA / Core") {
							codecid = "DTS-HD MA";
						}
					}
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(!tmpft.Equals(FormatType.None)) {
						ft = tmpft;
					}
					try {
						ft.ID = int.Parse(MI.Get(StreamKind.Audio, i, "ID"));
					}
					catch {}
					string[] values = { "Channel(s)/String", "SamplingRate/String", "BitDepth/String", "BitRate/String" };
					ft.ExtraInfo =
						string.Join(", ", from y in
							from x in values select MI.Get(StreamKind.Audio, i, x)
							where !string.IsNullOrEmpty(y) select y);
					items.Add(ft);
				}
				
				count = MI.Count_Get(StreamKind.Text);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Text, i, "Format");
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(!tmpft.Equals(FormatType.None)) {
						ft = tmpft;
					}
					try {
						ft.ID = int.Parse(MI.Get(StreamKind.Text, i, "ID"));
					}
					catch {}
					items.Add(ft);
				}
			}
			catch {
				return FormatType.None;
			}
			
			MI.Close();
			
			int idx;
			if(!int.TryParse(settings["Core", "streamindex"], out idx)) {
				idx = -1;
			}
			
			if(items.Count != 1) {
				items.Sort(new FormatTypeComparer());
				for(int i = 0; i < items.Count; ++i) {
					FormatType t = items[i];
					t.Index = i;
					items[i] = t;
				}
				return new FormatType(container, items.ToArray()) { Index = idx };
			}
			else {
				return new FormatType(container, items[0].CodecString) { ExtraInfo = items[0].ExtraInfo };
			}
		}
	}
}

