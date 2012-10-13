using System;
using System.IO;

namespace NeonVidUtil.Core {
	public interface INeUI {
		void Output(string text);
		void ProgressBar(int progessId, Stream stream);
	}
}

