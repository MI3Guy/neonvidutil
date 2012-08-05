using System;
using System.IO;
using FLACSharp;
using WAVSharp;
using NUnit.Framework;


namespace UnitTests {
	[TestFixture]
	public class FLACSharpTests {
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
						FLACEncoder encoder = new FLACEncoder(dataChunk, outFS,
						                                    new FLACInfo {
						                                        bits_per_sample = wavReader.FormatChunk.wBitsPerSample,
						                                        channels = wavReader.FormatChunk.nChannels,
						                                        sample_rate = wavReader.FormatChunk.nSamplesPerSec
						                                    });
						encoder.Encode();
					}
				}
			}
			
			
		}
	}
}

