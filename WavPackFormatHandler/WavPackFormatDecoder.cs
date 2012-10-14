using System;
using System.IO;
using NeonVidUtil.Core;
using WAVSharp;
using JVL.Audio.WavPackWrapper;

namespace NeonVidUtil.Plugin.WavPackFormatHandler {
	public class WavPackFormatDecoder : FormatCodec {
		public WavPackFormatDecoder() {
		}
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff, int progressId) {
			using(WavPackDecoder decoder = new WavPackDecoder(inbuff))
			{
				
				WAVWriter writer = new WAVWriter(outbuff, decoder.WaveFormat, 0);
				int length;
				byte[] buffer = new byte[4];
				while((length = decoder.Read(buffer)) != 0) {
					outbuff.Write(buffer, 0, length);
				}
			}
		}
		
		public override string DisplayValue {
			get {
				return "WavPack\t=>\tWAV";
			}
		}
	}
}

