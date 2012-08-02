using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NeonVidUtil.Core;


namespace UnitTests.NeonVidUtilCore {
	[TestFixture, RequiresThread]
	public class CircularStreamTests {
		//[Test, RequiresThread]
		public void TestData() {
			CircularStream stream = new CircularStream();
			
			const int MaxValueToTest = 10000;//00;
			
			Thread t0 = new Thread(delegate() {
				BinaryWriter writer = new BinaryWriter(stream);
				for(int i = 0; i < MaxValueToTest; ++i) {
					writer.Write(i);
				}
				stream.MarkEnd();
			});
			
			Thread t1 = new Thread(delegate() {
				BinaryReader reader = new BinaryReader(stream);
				for(int i = 0; i < MaxValueToTest; ++i) {
					int result = reader.ReadInt32();
					Console.WriteLine("Read: {0}", result);
					Assert.AreEqual(i, result);
				}
				byte[] data = new byte[1];
				Assert.AreEqual(0, stream.Read(data, 0, 1));
			});
			
			t0.Start();
			t1.Start();
			t0.Join();
			t1.Join();
			
			Console.WriteLine("Finished CircularStreamTests.TestData()");
		}
	}
}

