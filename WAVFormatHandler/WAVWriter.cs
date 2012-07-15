using System;
using System.IO;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVWriter {
		public WAVWriter(Stream outstream, WAVFormatChunk formatChunk, uint length) {
			writer = new BinaryWriter(outstream);
			
			WAVRIFF header = new WAVRIFF();
			header.ckID = WAVConst.ChunkIdRiff;
			header.size = WAVRIFF.sizeValPartial + formatChunk.cbSize + length;
			header.WAVEID = WAVConst.ChunkIdWave;
			header.WriteTo(writer);
			
			formatChunk.WriteTo(writer);
			
			WAVDataChunk dataChunk = new WAVDataChunk();
			dataChunk.cksize = length;
			dataChunk.WriteTo(writer);
		}
		
		BinaryWriter writer;
		
		public void WriteSample(WAVDataSample sample) {
			writer.Write(sample.Data);
		}
		
		public static uint CalcLength(WAVFormatChunk fmtChunk, WAVDataChunk datChunk, int numBytes) {
			return (uint)(datChunk.CalcLength() * (fmtChunk.wBitsPerSample / 8) / numBytes);
		}
	}
}

