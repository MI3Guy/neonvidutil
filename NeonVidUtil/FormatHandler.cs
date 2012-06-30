using System;
using System.Collections.Generic;

namespace NeonVidUtil {
	public abstract class FormatHandler {
		protected FormatHandler() {
			allHandlers.Add(this);
		}
		
		public abstract bool HandlesConversion(FormatType input, FormatType output);
		
		public abstract FormatDecoder Decode(FormatType format);
		
		private static List<FormatHandler> allHandlers = new List<FormatHandler>();
	}
}

