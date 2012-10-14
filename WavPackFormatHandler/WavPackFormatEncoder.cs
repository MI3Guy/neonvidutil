using System;
using System.IO;
using NeonVidUtil.Core;
using WavPackSharp;
using WAVSharp;

namespace NeonVidUtil.Plugin.WavPackFormatHandler {
	public class WavPackFormatEncoder : FormatCodec {
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData (Stream inbuff, Stream outbuff, int progressId) {
			WAVReader reader = new WAVReader(inbuff);
			reader.ReadDataChunk();
			WavPackEncoder.Encode(reader, outbuff, () => NeAPI.ProgressBar(progressId, inbuff));
		}
		
		public override string DisplayValue {
			get {
				return "WAV\t=>WavPack";
			}
		}
	}
}

