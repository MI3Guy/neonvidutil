using System;
using System.IO;
using NeonVidUtil.Core;
using WAVSharp;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVStripBits : FormatCodec {
		public WAVStripBits() {
			
		}
		
		MemoryStream outStream;
		string fname;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			fname = UseTempFile(inbuff);
			
			if(!File.Exists(fname)) throw new FileNotFoundException("Temp/original file could not be found", fname);
			
			return (outStream = new MemoryStream());
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			int depth;
			using(FileStream fs = File.OpenRead(fname)) {
				WAVBitDepthDetector detector = new WAVBitDepthDetector(fs);
				depth = detector.Check();
			}
			
			
			using(FileStream fs = File.OpenRead(fname)) {
				WAVReader reader = new WAVReader(fs);
				WAVFormatChunk fmtChunk = reader.FormatChunk;
				WAVDataChunk dataChunk = reader.ReadDataChunk();
				
				WAVFormatChunk fmtChunk2 = new WAVFormatChunk(fmtChunk, depth);
				
			}
		}
	}
}

