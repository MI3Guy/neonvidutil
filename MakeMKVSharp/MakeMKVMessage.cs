using System;
using System.Collections.Generic;
using System.Text;

namespace MakeMKVSharp {
	public abstract class MakeMKVMessage {
		public static MakeMKVMessage Create(string msgstr) {
			if(msgstr.StartsWith("DRV:")) {
				return new DriveInfo(msgstr);
			}
			else {
				return null;
			}
		}
		
		protected object[] ParseCommand(string command) {
			command = command.Substring(command.IndexOf(':') + 1);
			List<object> ret = new List<object>();
			StringBuilder sb = new StringBuilder();
			
			bool inString = false;
			foreach(char c in command) {
				switch(c) {
					case '"':
						inString = !inString;
						break;
						
					case ',':
						try {
							ret.Add(int.Parse(sb.ToString()));
						}
						catch(FormatException) {
							ret.Add(sb.ToString());
						}
						sb = new StringBuilder();
						break;
						
					default:
						sb.Append(c);
						break;
				}
			}
			return ret.ToArray();
		}
	}
}

