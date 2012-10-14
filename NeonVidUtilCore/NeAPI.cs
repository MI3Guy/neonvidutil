using System;
using System.IO;

namespace NeonVidUtil.Core {
	public static class NeAPI {
		static NeAPI() {
			Settings = new NeonOptions();
		}
		
		public static INeUI UI {
			private get;
			set;
		}
		
		public static NeonOptions Settings {
			get;
			private set;
		}
		
		public static void Output(string line) {
			UI.Output(line);
		}
		
		public static void Output(string format, params object[] args) {
			UI.Output(string.Format(format, args));
		}
		
		
		public static void ProgressBar(int id, Stream stream) {
			UI.ProgressBar(id, stream);
		}
	}
}

