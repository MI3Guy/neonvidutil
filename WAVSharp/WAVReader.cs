using System;
using System.IO;
using System.Text;

namespace WAVSharp {
	public class WAVReader {
		public WAVReader(Stream stream) {
			this.stream = stream;
			reader = new BinaryReader(stream);
			
			wavRiff = new WAVRIFF(reader);
			
			if(wavRiff.ckID != "RIFF" || wavRiff.WAVEID != "WAVE") {
				throw new ApplicationException("Not a valid WAV file");
			}
		}
		
		public WAVDataChunk ReadDataChunk() {
			while(reader.BaseStream.Position < reader.BaseStream.Length) {
				string ckID = Encoding.ASCII.GetString(reader.ReadBytes(4));
				switch(ckID) {
					case WAVConst.ChunkIdFormat:
						formatChunk = new WAVFormatChunk(reader);
						
						if(formatChunk.wFormatTag != WAVConst.FormatTag.PCM &&
							(formatChunk.wFormatTag != WAVConst.FormatTag.EXTENSIBLE && formatChunk.SubFormat != WAVConst.FormatSubtypePCM)) {
							throw new ApplicationException("WAV does not contain PCM data.");
						}
						break;
						
					case WAVConst.ChunkIdData:
						return new WAVDataChunk(reader, formatChunk);
						
					default:
						uint size = reader.ReadUInt32();
						reader.BaseStream.Position += size;
						break;
				}
			}
			return null;
		}
		
		private Stream stream;
		private BinaryReader reader;
		
		WAVRIFF wavRiff;
		WAVFormatChunk formatChunk;
		
		public WAVRIFF WavRiff {
			get { return wavRiff; }
		}
		
		public WAVFormatChunk FormatChunk {
			get { return formatChunk; }
		}
	}
}

