using System;
using System.IO;
using System.Threading;

namespace NeonVidUtil.Core {
	public class EncodeStepHandler {
		public EncodeStepHandler(Stream _instream, Stream _outstream, FormatCodec _codec, Thread _thread, int _progressId) {
			instream = _instream;
			outstream = _outstream;
			codec = _codec;
			thread = _thread;
			progressId = _progressId;
		}
		
		private Stream instream;
		private Stream outstream;
		private FormatCodec codec;
		private Thread thread;
		private Exception error;
		private int progressId;
		
		public void Run() {
			NeAPI.Output(string.Format("Beginning Conversion: {0}", codec.DisplayValue));
			try {
				codec.ConvertData(instream, outstream, progressId);
				error = null;
			}
			catch(Exception ex) {
				error = ex;
			}
			finally {
				NeAPI.ProgressBar(progressId, null);
			}
			
			NeAPI.Output("Ended Conversion");
		}
		
		public void RunThread() {
			thread.Start(this);
		}
		
		public void JoinThread() {
			thread.Join();
			if(error != null) {
				throw new Exception("An error occurred during processing.", error);
			}
		}
		
		public void AbortThread() {
			thread.Abort();
		}
	}
}

