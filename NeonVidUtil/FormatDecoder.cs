using System;
using System.IO;

namespace NeonVidUtil {
	public abstract class FormatDecoder {
		public abstract void PrintInfo();
		public abstract void ReadData(Stream buff);
	}
}

