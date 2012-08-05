using System;
using System.IO;
using NUnit.Framework;
using WAVSharp;

namespace UnitTests {
	[TestFixture]
	public class WAVSharpTests {
		
		[Test]
		public void NonDataEnd() {
			string[] files = { "../../test-files/M1F1-int24WE-AFsp.wav" };
			
			foreach(string file in files) {
				using(FileStream fs = File.OpenRead(file)) {
					WAVReader reader = new WAVReader(fs);
					
					WAVDataChunk dataChunk = reader.ReadDataChunk();
					
					Assert.AreEqual(0, dataChunk.cksize % (reader.FormatChunk.wBitsPerSample/8));
					Assert.AreEqual(0, (dataChunk.cksize / (reader.FormatChunk.wBitsPerSample/8)) % reader.FormatChunk.nChannels);
					
					long numTotalSamples = dataChunk.cksize / (reader.FormatChunk.wBitsPerSample/8) / reader.FormatChunk.nChannels;
					
					
					for(long i = 0; i < numTotalSamples; ++i) {
						Assert.AreNotEqual(null, dataChunk.ReadSample());
					}
					Assert.AreEqual(null, dataChunk.ReadSample());
				}
				
				using(FileStream fs = File.OpenRead(file)) {
					WAVReader reader = new WAVReader(fs);
					
					WAVDataChunk dataChunk = reader.ReadDataChunk();
					
					uint length = dataChunk.cksize;
					uint total = 0;
					
					WAVDataStream stream = dataChunk.GetPCMStream();
					
					byte[] buff = new byte[1024];
					int curr;
					while((curr = stream.Read(buff, 0, buff.Length)) != 0) {
						total += (uint)curr;
					}
					
					Assert.AreEqual(length, total);
				}
			}
		}
		
		[Test]
		public void DataSame() {
			string[] files = { "../../test-files/M1F1-int24WE-AFsp.wav", "/home/john/Projects/tmp4c280100.tmp" };
			
			foreach(string file in files) {
				using(FileStream fs1 = File.OpenRead(file)) {
					using(FileStream fs2 = File.OpenRead(file)) {
						WAVReader reader1 = new WAVReader(fs1);
						WAVReader reader2 = new WAVReader(fs2);
						
						WAVDataChunk dataChunk1 = reader1.ReadDataChunk();
						WAVDataChunk dataChunk2 = reader2.ReadDataChunk();
						
						WAVDataStream stream = dataChunk1.GetPCMStream();
						
						byte[] buff = new byte[reader1.FormatChunk.nChannels * (reader1.FormatChunk.wBitsPerSample / 8)];
						int curr;
						while((curr = stream.Read(buff, 0, buff.Length)) != 0) {
							WAVDataSample sample = dataChunk2.ReadSample();
							Assert.AreEqual(sample.Data.Length, curr);
							Assert.AreEqual(buff.Length, curr);
							
							for(int i = 0; i < buff.Length; ++i) {
								Assert.AreEqual(buff[i], sample.Data[i]);
							}
						}
					}
				}
			}
		}
		
	}
}

