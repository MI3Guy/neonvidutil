using System;
using System.IO;

namespace WAVSharp {
	internal static class ExtensionMethods {
		const int numToRead = 1024;
		internal static void ReadBytesIgnoreUInt(this BinaryReader reader, uint num) {
			for(uint i = 0; i < num; i += numToRead) {
				if(i + numToRead > num) {
					reader.ReadBytes((int)num);
				}
				else {
					reader.ReadBytes((int)numToRead);
				}
			}
		}
	}
}

