using System;

namespace MakeMKVSharp {
	public class DriveInfo : MakeMKVMessage {
		public DriveInfo() {
		}
		
		public DriveInfo(string msgstr) {
			object[] msgParts = ParseCommand(msgstr);
			if(msgParts.Length != 6) {
				throw new FormatException();
			}
			
			try {
				Index = (int)msgParts[0];
				Visible = (int)msgParts[1] != 0;
				Enabled = (int)msgParts[2] != 0;
				Flags = (int)msgParts[3];
				DriveName = (string)msgParts[4];
				DiscName = (string)msgParts[5];
			}
			catch(Exception ex) {
				throw new FormatException("Invalid type for arguement", ex); 
			}
		}
		
		public int Index {
			get;
			set;
		}
		public bool Visible {
			get;
			set;
		}
		public bool Enabled {
			get;
			set;
		}
		public int Flags {
			get;
			set;
		}
		string DriveName {
			get;
			set;
		}
		string DiscName {
			get;
			set;
		}
	}
}

