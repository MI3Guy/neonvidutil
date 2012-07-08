using System;
using System.IO;
using NeonVidUtil.Core;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVStripBits : FormatCodec {
		public WAVStripBits(FormatType next) {
			this.next = next;
		}
		
		FormatType next;
		
		public override Stream InitConvertData(Stream inbuff, string outfile) {
			return null;
		}
		
		public override void ConvertData(Stream inbuff, Stream outbuff) {
			throw new NotImplementedException();
		}
	}
}

