using System;

namespace NeonVidUtil.Core {
	public class EncodeStep {
		public EncodeStep(FormatHandler handler, FormatType output) {
			Handler = handler;
			Output = output;
		}
		
		public FormatHandler Handler {
			get;
			protected set;
		}
		
		public FormatType Output {
			get;
			protected set;
		}
	}
}

