using System;
using System.IO;
using NeonVidUtil.Core;
using WAVSharp;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVStripBits : FormatCodec {
		public WAVStripBits() {
			
		}
		
		string fname;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			fname = UseTempFile(inbuff);
			
			if(!File.Exists(fname)) throw new FileNotFoundException("Temp/original file could not be found", fname);
			
			if(outfile == null) {
				return new CircularStream();
			}
			else {
				return File.Create(outfile);
			}
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			int depth;
			using(FileStream fs = File.OpenRead(fname)) {
				WAVBitDepthDetector detector = new WAVBitDepthDetector(fs);
				depth = detector.Check();
			}
			
			
			using(FileStream fs = File.OpenRead(fname)) {
				WAVReader reader = new WAVReader(fs);
				WAVDataChunk dataChunk = reader.ReadDataChunk();
				WAVFormatChunk fmtChunk = reader.FormatChunk;
				
				WAVFormatChunk fmtChunk2 = new WAVFormatChunk(fmtChunk, depth);
				
				WAVWriter writer = new WAVWriter(outbuff, fmtChunk2, (uint)dataChunk.CalcLength());
				
				WAVDataSample sample;
				while((sample = dataChunk.ReadSample()) != null) {
					writer.WriteSample(sample);
				}
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
	}
}

