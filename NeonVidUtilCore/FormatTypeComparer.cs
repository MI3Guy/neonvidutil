using System;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public class FormatTypeComparer : IComparer<FormatType>, IEqualityComparer<FormatType> {
		public int Compare(FormatType x, FormatType y) {
			return x.ID.CompareTo(y.ID);
		}
		
		public bool Equals(FormatType x, FormatType y) {
			return x.Equals(y);
		}
		
		public int GetHashCode(FormatType x) {
			return x.GetHashCode();
		}
	}
}

