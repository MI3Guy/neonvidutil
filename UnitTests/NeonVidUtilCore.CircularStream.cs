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
			
			byte[] buff = new byte[0x20000];
			using(FileStream fs = File.OpenRead("/media/EXTRADATA4/Videos/NeTestVideos/Main_Movie_t01.vc1")) {
				Assert.AreEqual(buff.Length, fs.Read(buff, 0, buff.Length));
			}
			
			Random rand = new Random();
			
			Thread t0 = new Thread(delegate() {
				BinaryWriter writer = new BinaryWriter(stream);
				for(int i = 0; i < buff.Length;) {
					int amount = rand.Next() / 4;
					if(amount > buff.Length - i) {
						amount = buff.Length - i;
					}
					stream.Write(buff, i, amount);
					i += amount;
				}
				stream.MarkEnd();
			});
			
			Thread t1 = new Thread(delegate() {
				int i;
				while(true) {
					byte[] buffer = new byte[rand.Next() / 4];
					int len = stream.Read(buffer, 0, buffer.Length);
					
					for(int j = 0; j < len; ++j) {
						Assert.AreEqual(buff[i + j], buffer[j]);
					}
					
					i += len;
					Assert.IsTrue(len != 0 || i == buff.Length);
					if(len == 0) {
						break;
					}
				}
				
			});
			
			t0.Start();
			t1.Start();
			t0.Join();
			t1.Join();
			
			Console.WriteLine("Finished CircularStreamTests.TestData()");
		}
	}
}

