using System;
using System.IO;
using FLACSharp;
using WAVSharp;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatEncoder : FormatCodec {
		public FLACFormatEncoder() {
			
		}
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile != null) {
				return File.Create(outfile);
			}
			
			return new CircularStream();
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			WAVReader wavReader = new WAVReader(inbuff);
			WAVDataChunk dataChunk = wavReader.ReadDataChunk();
			
			FLACEncoder encoder = new FLACEncoder(dataChunk, outbuff, new FLACInfo(wavReader.FormatChunk), () => { NeAPI.ProgressBar(progressId, inbuff); });
			encoder.Encode();
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
		
		public override string DisplayValue {
			get {
				return "WAV\t=>\tFLAC";
			}
		}
	}
}

