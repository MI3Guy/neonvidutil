using System;
using NeonVidUtil.Core;

namespace DGPulldownSharp {
	public class DGPulldownFormatHandler : FormatHandler {
		public override void OutputHandlerInfo() {
			NeAPI.Output("Supported Processing");
			NeAPI.Output("\tMPEG-2\t:\tPulldown Removal");
		}
	}
}

