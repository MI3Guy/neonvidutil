using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1PulldownRemover : FormatCodec {
		public override void ConvertData(System.IO.Stream inbuff, System.IO.Stream outbuff) {
			string fname = UseTempFile(inbuff);
			
			if(!File.Exists(fname)) throw new FileNotFoundException("Temp/original file could not be found", fname);
			
			string outfile = Path.GetTempFileName();
			string cmd = string.Format("\"{0}\" \"{1}\"", fname, outfile);
			Console.WriteLine("Running: vc1conv {0}", cmd);
			Process proc = Process.Start("Plugins/vc1conv", cmd);
			proc.WaitForExit();
			
			using(FileStream fs = File.OpenRead(outfile)) {
				fs.CopyTo(outbuff);
			}
		}
	}
}

