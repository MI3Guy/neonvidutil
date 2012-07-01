using System;
using System.IO;
using System.Diagnostics;

namespace NeonVidUtil {
	public class MKVFormatDecoder : FormatCodec {
		public MKVFormatDecoder(int index) {
			this.index = index - 1; // Handle mkvextract using odd index.
		}
		
		private int index;
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			string fname;
			if(inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				fname = fs.Name;
			}
			else {
				using(FileStream fs = CreateTempFile()) {
					inbuff.CopyTo(fs);
					fname = fs.Name;
				}
			}
			
			if(!File.Exists(fname)) throw new FileNotFoundException("Temp/original file could not be found", fname);
			
			string outfile = Path.GetTempFileName();
			string cmd = string.Format("tracks \"{0}\" --fullraw \"{2}:{1}\"", fname, outfile, index);
			Console.WriteLine("Running: mkvextract {0}", cmd);
			Process proc = Process.Start("mkvextract", cmd);
			proc.WaitForExit();
			
			using(FileStream fs = File.OpenRead(outfile)) {
				fs.CopyTo(outbuff);
			}
		}
	}
}

