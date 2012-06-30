using System;
using System.IO;

namespace NeonVidUtil {
	public abstract class FormatEncoder {
		public abstract void WriteData(Stream buff);
	}
}

