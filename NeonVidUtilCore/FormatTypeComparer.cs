using System;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public class FormatTypeComparer : IComparer<FormatType> {
		public int Compare(FormatType x, FormatType y) {
			return x.ID.CompareTo(y.ID);
		}
	}
}

