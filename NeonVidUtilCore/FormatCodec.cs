using System;
using System.IO;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public abstract class FormatCodec {
		private static List<string> tempFiles = new List<string>();
		
		
		public static string GetTempFileName(string extension) {
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
		
		public abstract Stream InitConvertData(Stream inbuff, string outfile);
		public abstract void ConvertData(Stream inbuff, Stream outbuff, int progressId);
		
		protected string UseTempFile(Stream inbuff) {
			return UseTempFile(inbuff, null);
		}
		
		protected string UseTempFile(Stream inbuff, string ext) {
			string fname;
			if(ext == null && inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				fname = fs.Name;
				fs.Close();
			}
			else if(inbuff is FileStream) {
				FileStream fs = (FileStream)inbuff;
				fname = CreateTempFileName(ext);
				fs.Close();
				File.Delete(fname);
				File.Move(fs.Name, fname);
			}
			else {
				using(FileStream fs = CreateTempFile(ext)) {
					//inbuff.CopyTo(fs);
					byte[] buff = new byte[1024*4];
					int len = 0;
					while((len = inbuff.Read(buff, 0, buff.Length)) != 0) {
						fs.Write(buff, 0, len);
					}
					fname = fs.Name;
				}
			}
			
			
			return fname;
		}
		
	}
}

