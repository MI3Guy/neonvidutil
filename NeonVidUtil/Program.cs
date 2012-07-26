using System;
using System.Collections.Generic;
using NeonVidUtil.Core;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			/*using(System.IO.FileStream ifs = System.IO.File.OpenRead("/home/john/Projects/audio.thd")) {
				using(System.IO.FileStream ofs = System.IO.File.Open("test.wav", System.IO.FileMode.Create)) {
					FFMpegFormatHandler.FFmpegConvert.ConvertFFmpeg(ifs, "truehd", ofs, "wav", "pcm_s24le");
				}
			}*/
			//FFMpegFormatHandler.FFmpegConvert.ConvertFFmpeg("/home/john/Projects/audio.thd", "truehd", "test.wav", "wav", "pcm_s24le");
			
			//string[] args = { "/media/PHANTOM/Videos/Shorts/Blender Open Movies/02 Big Buck Bunny.mkv", "test.flac" };
			//string[] args = { "/media/EXTRADATA4/Videos/MUMMYRETURNS/Main_Movie_t01.mkv", "test.vc1" };
			string[] args = { "/media/EXTRADATA4/Videos/Megamind_3D/Megamind_3D_t00.mkv", "test.flac" };
			
			
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
			
			FormatCodec.DeleteTempFiles();
			
			return 0;
		}
	}
}

