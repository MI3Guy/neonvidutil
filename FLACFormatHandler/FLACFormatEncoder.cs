using System;
using System.IO;
using Wav2Flac;
using WAVSharp;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatEncoder : FormatCodec {
		public FLACFormatEncoder() {
			
		}
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile != null) {
				return File.OpenWrite(outfile);
			}
			
			return new CircularStream();
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			WAVReader wavReader = new WAVReader(inbuff);
			WAVDataChunk dataChunk = wavReader.ReadDataChunk();
			
			using(FlacWriter flacWriter = new FlacWriter(outbuff, wavReader.FormatChunk.wBitsPerSample, wavReader.FormatChunk.nChannels, (int)wavReader.FormatChunk.nSamplesPerSec)) {
				byte[] buffer = new byte[wavReader.FormatChunk.wBitsPerSample * wavReader.FormatChunk.nChannels * wavReader.FormatChunk.nSamplesPerSec];
				int bytesRead;
				
				
				WAVDataStream stream = dataChunk.GetPCMStream();
				
				do {
					bytesRead = stream.Read(buffer, 0, buffer.Length);
					flacWriter.Write(buffer, 0, bytesRead);
				} while(bytesRead > 0);
			}
			
			if(outbuff is CircularStream) {
				((CircularStream)outbuff).MarkEnd();
			}
		}
	}
}

