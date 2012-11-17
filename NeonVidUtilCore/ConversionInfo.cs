using System;

namespace NeonVidUtil.Core {
	public class ConversionInfo {
		[Flags]
		public enum ConversionFlags {
			None = 0x00,
			RequiresTempFile = 0x01,
			Lossy = 0x02
		}
			
		
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
		
		public ConversionFlags Flags {
			get;
			set;
		}
		
		public virtual ConversionInfo Clone() {
			return CloneHelper(new ConversionInfo());
		}
		
		protected virtual ConversionInfo CloneHelper(ConversionInfo conv) {
			conv.InFormatType = this.InFormatType;
			conv.OutFormatType = this.OutFormatType;
			conv.StreamIndex = this.StreamIndex;
			return conv;
		}
		
		public int Weight {
			get {
				return 1 +
					(Flags & ConversionFlags.RequiresTempFile) == ConversionFlags.RequiresTempFile ? 1 : 0 +
					(Flags & ConversionFlags.Lossy) == ConversionFlags.Lossy ? 1 : 0;
			}
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
			return unchecked(InFormatType.GetHashCode() + 17*OutFormatType.GetHashCode() + 3*StreamIndex);
		}
	}
}

