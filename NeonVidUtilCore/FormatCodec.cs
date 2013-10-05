using System;
using System.IO;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public abstract class FormatCodec {
		public abstract Stream InitConvertData(Stream inbuff, string outfile);
		public abstract void ConvertData(Stream inbuff, Stream outbuff, int progressId);
		
		public abstract string DisplayValue {
			get;
		}
		
		private static List<string> tempFiles = new List<string>();
		
		
		private static string GetTempFileName(string extension) {
			if(extension == null) {
				return Path.GetTempFileName();
			}
			string fileName;
			int attempt = 0;
			bool exit = false;
			do {
				fileName = Path.GetRandomFileName();
				fileName = Path.ChangeExtension(fileName, extension);
				fileName = Path.Combine(Path.GetTempPath(), fileName);
		
				try {
					using(new FileStream(fileName, FileMode.CreateNew)) {
					}
					
					exit = true;
				}
				catch(IOException ex) {
					if(++attempt == 10) {
						throw new IOException("No unique temporary file name is available.", ex);
					}
				}
		
			} while (!exit);
		
			return fileName;
		}
		
		public static FileStream CreateTempFile() {
			return CreateTempFile(null);
		}
		
		public static FileStream CreateTempFile(string ext) {
			return File.Create(CreateTempFileName(ext));
		}
		
		public static string CreateTempFileName() {
			return CreateTempFileName(null);
		}
		
		public static string CreateTempFileName(string ext) {
			string fname = GetTempFileName(ext);
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
		
		protected string UseTempFile(Stream inbuff) {
			var fsInbuff = inbuff as FileStream;
			if(fsInbuff != null && File.Exists(fsInbuff.Name)) { // File.Exists to filter out streams like stdout.
				string fname = fsInbuff.Name;
				fsInbuff.Close();
				return fname;
			}
			else {
				string fname = Path.GetTempFileName();
				using(FileStream fs = File.Create(fname)) {
					inbuff.CopyTo(fs);
				}
				return fname;
			}
		}
		
	}
}

