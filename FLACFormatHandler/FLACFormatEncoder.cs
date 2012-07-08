using System;
using System.IO;
using Wav2Flac;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.FLACFormatHandler {
	public class FLACFormatEncoder : FormatCodec {
		public FLACFormatEncoder() {
			
		}
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			if(outfile != null) {
				return File.OpenWrite(outfile);
			}
			
			return new MemoryStream();
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			using(WavReader wavReader = new WavReader(inbuff)) {
				using(FlacWriter flacWriter = new FlacWriter(outbuff, wavReader.BitDepth, wavReader.Channels, wavReader.SampleRate)) {
					byte[] buffer = new byte[wavReader.Bitrate / 8];
					int bytesRead;
					
					do {
						bytesRead = wavReader.InputStream.Read(buffer, 0, buffer.Length);
						flacWriter.Write(buffer, 0, buffer.Length);
					} while(bytesRead > 0);
				}
			}
		}
	}
}

