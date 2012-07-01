using System;
using System.Collections.Generic;
using MediaInfoLib;

namespace NeonVidUtil {
	public class MediaInfoFormatHandler : FormatHandler {
		public MediaInfoFormatHandler() : base() {
			MI = new MediaInfo();
			MI.Option("Info_Version", "0.7.0.0;NeonVidUtil_MediaInfoFormatHandler;0.0.0.1");
		}
		
		protected MediaInfo MI;
		
		public override FormatType ReadInfo(string file) {
			MI.Open(file);
			Console.WriteLine(MI.Inform());
			
			FormatType.FormatContainer container = (FormatType.FormatContainer)Enum.Parse(typeof(FormatType.FormatContainer), MI.Get(StreamKind.General, 0, "Format"));
			List<FormatType> items = new List<FormatType>();//[MI.Count_Get(StreamKind.Video) + MI.Count_Get(StreamKind.Audio) + MI.Count_Get(StreamKind.Text)];
			
			try {
				int count = MI.Count_Get(StreamKind.Video);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Video, i, "Format");
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(tmpft != null) {
						ft = tmpft;
					}
					try {
						ft.Param = int.Parse(MI.Get(StreamKind.Video, i, "ID"));
					}
					catch {}
					items.Add(ft);
				}
				
				count = MI.Count_Get(StreamKind.Audio);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Audio, i, "Format");
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(tmpft != null) {
						ft = tmpft;
					}
					try {
						ft.Param = int.Parse(MI.Get(StreamKind.Audio, i, "ID"));
					}
					catch {}
					items.Add(ft);
				}
				
				count = MI.Count_Get(StreamKind.Text);
				for(int i = 0; i < count; ++i) {
					string codecid = MI.Get(StreamKind.Text, i, "Format");
					FormatType ft = new FormatType(codecid);
					FormatType tmpft = ft.IsRawCodec();
					if(tmpft != null) {
						ft = tmpft;
					}
					try {
						ft.Param = int.Parse(MI.Get(StreamKind.Text, i, "ID"));
					}
					catch {}
					items.Add(ft);
				}
			}
			catch {
				return null;
			}
			
			MI.Close();
			
			return new FormatType(container, items.ToArray());
		}
	}
}

