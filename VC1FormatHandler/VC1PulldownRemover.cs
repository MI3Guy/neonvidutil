using System;
using System.IO;
using System.Diagnostics;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.VC1FormatHandler {
	public class VC1PulldownRemover : FormatCodec {
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			/*string fname = UseTempFile(inbuff);
			
			if(!File.Exists(fname)) throw new FileNotFoundException("Temp/original file could not be found", fname);
			
			if(outfile == null) {
				outfile = CreateTempFileName();
			}
			string cmd = string.Format("\"{0}\" \"{1}\"", fname, outfile);
			Console.WriteLine("Running: vc1conv {0}", cmd);
			Process proc = Process.Start("Plugins/vc1conv", cmd);
			proc.WaitForExit();
			
			return File.OpenRead(outfile);
			
			if(outfile != null) {
				file = outfile;
			}*/
			
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData(System.IO.Stream inbuff, System.IO.Stream outbuff) {
			string inFileName = null;
			string outFileName = null;
			if(inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				inFileName = fs.Name;
				fs.Close();
			}
			
			if(outbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				outFileName = fs.Name;
				fs.Close();
			}
			
			VC1Conv conv = new VC1Conv();
			
			if(inFileName == null && outFileName == null) {
				conv.VC1ConvRemovePulldown(inbuff, outbuff);
			}
			else if(inFileName == null /* && outFileName != null */) {
				conv.VC1ConvRemovePulldown(inbuff, outFileName);
			}
			else if(/*inFileName != null && */outFileName == null) {
				conv.VC1ConvRemovePulldown(inFileName, outbuff);
			}
			else /*if(inFileName != null && outFileName != null)*/ {
				conv.VC1ConvRemovePulldown(inFileName, outFileName);
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
	}
}

