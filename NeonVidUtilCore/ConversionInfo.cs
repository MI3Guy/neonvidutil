using System;

namespace NeonVidUtil.Core {
	public class ConversionInfo {
		public ConversionInfo() {
			StreamIndex = -1;
		}
		
		public virtual FormatType InFormatType {
			get;
			set;
		}
		
		public virtual FormatType OutFormatType {
			get;
			set;
		}
		
		public int StreamIndex {
			get;
			set;
		}
		
		public virtual ConversionInfo Clone() {
			return CloneHelper(new ConversionInfo());
		}
		
		protected ConversionInfo CloneHelper(ConversionInfo conv) {
			conv.InFormatType = this.InFormatType;
			conv.OutFormatType = this.OutFormatType;
			conv.StreamIndex = this.StreamIndex;
			return conv;
		}
		
		public override bool Equals(object obj) {
			if(!(obj is ConversionInfo)) {
				return false;
			}
			
			ConversionInfo other = (ConversionInfo)obj;
			
			return this.InFormatType.Equals(other.InFormatType) &&
				this.OutFormatType.Equals(other.OutFormatType) &&
				this.StreamIndex == other.StreamIndex;
		}
		
		public override int GetHashCode() {
			return InFormatType.GetHashCode() + 17*OutFormatType.GetHashCode() + 3*StreamIndex;
		}
	}
}

