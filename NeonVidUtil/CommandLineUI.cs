using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil {
	public class CommandLineUI : INeUI {
		public void Output(string str) {
			lock(prevProgressValueLock) {
				if(prevProgressValue > 0) {
					Console.CursorLeft = 0;
					Console.Write(new string(' ', prevProgressValue));
					Console.CursorLeft = 0;
					prevProgressValue = 0;
				}
				Console.WriteLine(str);
			}
		}
		
		int lastId = 0;
		object prevProgressValueLock = new object();
		int prevProgressValue = 0;
		public void ProgressBar(int id, Stream stream) {
			if(stream == null) {
				lock(prevProgressValueLock) {
					if(id > lastId) {
						lastId = id + 1;
					}
					else {
						lastId += 1;
					}
				}
			}
			else if(id >= lastId) {
				if(stream.CanSeek) {
					lock(prevProgressValueLock) {
						if(id > lastId) {
							lastId = id + 1;
						}
					}
					int newValue = (int)(((double)stream.Position / (double)stream.Length) * (double)Console.WindowWidth);
					lock(prevProgressValueLock) {
						if(newValue != prevProgressValue) {
							if(newValue < prevProgressValue) {
								Console.CursorLeft = 0;
								Console.Write(new string(' ', prevProgressValue));
								Console.CursorLeft = 0;
								prevProgressValue = 0;
							}
							
							Console.Write(new string('-', newValue - prevProgressValue));
							prevProgressValue = newValue;
						}
					}
				}
			}
		}
	}
}
