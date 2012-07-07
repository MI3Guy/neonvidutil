using System;
using NeonVidUtil.Core;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			//string[] args = { "/media/PHANTOM/Videos/Shorts/Blender Open Movies/02 Big Buck Bunny.mkv", "test.flac" };
			string[] args = { "/media/EXTRADATA4/Videos/MUMMYRETURNS/Main_Movie_t01.mkv", "test.vc1" };
			
			FormatType inft = EncodePath.AutoReadInfo(args[0]);
			FormatType outft = EncodePath.AutoGenerateOutputType(args[1]);
			
			/*FormatHandler handler = FormatHandler.FindConverter(inft, outft, null);
			
			System.IO.FileStream infs = System.IO.File.OpenRead(args[0]);
			System.IO.FileStream outfs = System.IO.File.OpenWrite(args[1]);
			FormatCodec dec = handler.ConvertStream(inft, outft, null);
			dec.ConvertData(infs, outfs);*/
			
			//FormatHandler[] handlers = EncodePath.FindConvertPath(inft, outft);
			EncodePath path = new EncodePath(inft, outft);
			
			/*foreach(FormatHandler handler in handlers) {
				//handler.ConvertStream(
			}*/
			
			path.Run(args[0], args[1]);
			
			return 0;
		}
	}
}

