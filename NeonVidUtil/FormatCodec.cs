using System;
using System.IO;
using System.Collections.Generic;

namespace NeonVidUtil {
	public abstract class FormatCodec {
		private static List<string> tempFiles = new List<string>();
		
		public static FileStream CreateTempFile(string basis) {
			string fname;
			Random rand = new Random();
			do {
				fname = string.Format("{0}.{1}.tmp", basis, rand.Next());
			} while(File.Exists(fname));
			
			lock(tempFiles) {
				tempFiles.Add(fname);
			}
			
			return File.OpenWrite(fname);
		}
		
		public static FileStream CreateTempFile() {
			return File.OpenWrite(CreateTempFileName());
		}
		
		public static string CreateTempFileName() {
			string fname = Path.GetTempFileName();
			lock(tempFiles) {
				tempFiles.Add(fname);
			}
			return fname;
		}
		
		public static void DeleteTempFiles() {
			lock(tempFiles) {
				foreach(string filename in tempFiles) {
					File.Delete(filename);
				}
				tempFiles.Clear();
			}
		}
		
		public abstract void ConvertData(Stream inbuff, Stream outbuff);
		
	}
}

