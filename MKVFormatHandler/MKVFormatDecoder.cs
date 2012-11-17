using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.MKVFormatHandler {
	public class MKVFormatDecoder : FormatCodec {
		public MKVFormatDecoder(int index, string mkvExtractPath) {
			this.index = index;
			this.mkvExtractPath = mkvExtractPath;
		}
		
		private int index;
		private string mkvExtractPath;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			string fname = UseTempFile(inbuff);
			
			if(!File.Exists(fname)) {
				throw new FileNotFoundException("Temp/original file could not be found", fname);
			}
			
			if(outfile == null) {
				outfile = CreateTempFileName();
			}
			string cmd = string.Format("tracks \"{0}\" --fullraw \"{2}:{1}\"", fname, outfile, index);
			NeAPI.Output(string.Format("Running: mkvextract {0}", cmd));
			Process proc = Process.Start(mkvExtractPath, cmd);
			proc.WaitForExit();
			
			return File.OpenRead(outfile);
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			
		}
		
		public override string DisplayValue {
			get {
				return "MKVEXTRACTTEMP";
			}
		}
		
	}
}

