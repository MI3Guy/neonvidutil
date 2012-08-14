using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.DGPulldownFormatHandler {
	public class DGPulldownRemover : FormatCodec {
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			string inFileName = null;
			string outFileName = null;
			if(inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				inFileName = fs.Name;
				fs.Close();
			}
			
			if(outbuff is FileStream) {
				FileStream fs = (FileStream)outbuff;
				outFileName = fs.Name;
				fs.Close();
			}
			
			DGPulldown dgpd = new DGPulldown();
			
			if(inFileName == null && outFileName == null) {
				dgpd.RemovePulldown(inbuff, outbuff);
			}
			else if(inFileName == null /* && outFileName != null */) {
				dgpd.RemovePulldown(inbuff, outFileName);
			}
			else if(/*inFileName != null && */outFileName == null) {
				dgpd.RemovePulldown(inFileName, outbuff);
			}
			else /*if(inFileName != null && outFileName != null)*/ {
				dgpd.RemovePulldown(inFileName, outFileName);
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
	}
}

