using System;
using System.IO;
using FLACSharp;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatDecoder : FormatCodec {
		public FLACFormatDecoder() {
		}
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile != null) {
				return File.Create(outfile);
			}
			
			return new CircularStream();
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			FLACDecoder decoder = new FLACDecoder(inbuff, outbuff, () => { NeAPI.ProgressBar(progressId, inbuff); });
			decoder.Process();
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
		
		public override string DisplayValue {
			get {
				return "FLAC\t=>\tWAV";
			}
		}
	}
}

