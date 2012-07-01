using System;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			MediaInfoFormatHandler theMediaInfoFormatHandler = new MediaInfoFormatHandler();
			MKVFormatHandler theMKVFormatHandler = new MKVFormatHandler();
			FLACFormatHandler theFLACFormatHandler = new FLACFormatHandler();
			
			string[] args = { "/media/PHANTOM/Videos/Shorts/Blender Open Movies/02 Big Buck Bunny.mkv", "test.flac" };
			
			FormatType inft = FormatHandler.AutoReadInfo(args[0]);
			FormatType outft = FormatHandler.AutoGenerateOutputType(args[1]);
			
			FormatHandler handler = FormatHandler.FindConverter(inft, outft, null);
			
			System.IO.FileStream infs = System.IO.File.OpenRead(args[0]);
			System.IO.FileStream outfs = System.IO.File.OpenWrite(args[1]);
			FormatCodec dec = handler.ConvertStream(inft, outft, null);
			dec.ConvertData(infs, outfs);
			
			return 0;
		}
	}
}

