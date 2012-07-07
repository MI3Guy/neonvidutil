using System;
using System.IO;

namespace NeonVidUtil.Core {
	public class EncodeStepHandler {
		public EncodeStepHandler(Stream _instream, Stream _outstream, FormatCodec _codec) {
			instream = _instream;
			outstream = _outstream;
			codec = _codec;
		}
		
		private Stream instream;
		private Stream outstream;
		private FormatCodec codec;

		
		public void Run() {
			codec.ConvertData(instream, outstream);
		}
	}
}

