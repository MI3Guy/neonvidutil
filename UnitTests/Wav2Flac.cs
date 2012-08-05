using System;
using System.IO;
using WAVSharp;
using Wav2Flac;
using NUnit.Framework;

namespace UnitTests {
	[TestFixture]
	public class Wav2FlacTests {
		[Test]
		public void Encode() {
			string[][] files = new string[][] {
				new string[] { "/home/john/Projects/tmp4c280100.tmp", "test.flac", "test2.flac" },
			};
			foreach(string[] iofiles in files) {
				using(FileStream inFS = File.OpenRead(iofiles[0])) {
					WAVReader wavReader = new WAVReader(inFS);
					WAVDataChunk dataChunk = wavReader.ReadDataChunk();
					
					using(FileStream outFS = File.Create(iofiles[1])) {
						using(FlacWriter flacWriter = new FlacWriter(outFS, wavReader.FormatChunk.wBitsPerSample, wavReader.FormatChunk.nChannels, (int)wavReader.FormatChunk.nSamplesPerSec)) {
							byte[] buffer = new byte[wavReader.FormatChunk.wBitsPerSample/8 * wavReader.FormatChunk.nChannels * wavReader.FormatChunk.nSamplesPerSec];
							int bytesRead;
							
							
							WAVDataStream stream = dataChunk.GetPCMStream();
							
							do {
								bytesRead = stream.Read(buffer, 0, buffer.Length);
								flacWriter.Write(buffer, 0, bytesRead);
							} while(bytesRead > 0);
						}
					}
				}
				
				using (WavReader wav = new WavReader(iofiles[0]))
		        {
		            using (FlacWriter flac = new FlacWriter(File.Create(iofiles[2]), wav.BitDepth, wav.Channels, wav.SampleRate))
		            {
		                // Buffer for 1 second's worth of audio data
		                byte[] buffer = new byte[wav.BitDepth / 8 * wav.Channels * wav.SampleRate];
		                int bytesRead;
		
		                do
		                {
		                    //ConsoleProgress.Update(wav.InputStream.Position, wav.InputStream.Length);
		
		                    bytesRead = wav.InputStream.Read(buffer, 0, buffer.Length);
		                    flac.Write(buffer, 0, bytesRead);
		                } while (bytesRead > 0);
		
		                // Finished!
		            }
		        }
			}
		}
	}
}

