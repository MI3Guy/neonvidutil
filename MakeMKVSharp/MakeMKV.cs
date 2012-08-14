using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MakeMKVSharp {
	public class MakeMKV {
		const string DefaultMakeMKVCommand = "makemkvcon";
		const string MakeMKVListDrives = "-r --cache=1 info disc:9999";
		
		public MakeMKV(string command = DefaultMakeMKVCommand) {
			makemkvCommand = command;
		}
		
		private string makemkvCommand;
		
		public IEnumerable<DriveInfo> ReadDrives() {
			return from message in RunMakeMKV(MakeMKVListDrives) where message is DriveInfo select (DriveInfo)message;
		}
		
		private IEnumerable<MakeMKVMessage> RunMakeMKV(string args) {
			ProcessStartInfo psi = new ProcessStartInfo(makemkvCommand, args) {
				UseShellExecute = false,
				RedirectStandardOutput = true
			};
			Process proc = Process.Start(psi);
			
			string line;
			while((line = proc.StandardOutput.ReadLine()) != null) {
				MakeMKVMessage msg = MakeMKVMessage.Create(line);
				if(msg == null) continue;
				yield return msg;
			}
			proc.WaitForExit();
		}
	}
}

