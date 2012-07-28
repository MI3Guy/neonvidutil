using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NeonVidUtil.Core {
	public abstract class FormatHandler {
		
		public virtual bool IsRawCodec(FormatType type) {
			return false;
		}
		
		/// <summary>
		/// Determines whether type is a raw codec and specifies the corresponding container.
		/// </summary>
		/// <returns>
		/// <c>true</c> if type is a raw codec; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='type'>
		/// Type to test.
		/// </param>
		/// <param name='outtype'>
		/// The format to use instead of the one passed.
		/// </param>
		public virtual bool IsRawCodec(FormatType type, out FormatType outtype) {
			outtype = null;
			return IsRawCodec(type);
		}
		
		public virtual FormatType ReadInfo(string file, NeonOptions settings) {
			return null;
		}
		
		public virtual FormatType GenerateOutputType(string file, NeonOptions settings) {
			return null;
		}
		
		public virtual object HandlesConversion(FormatType input, FormatType output, NeonOptions settings) {
			return null;
		}
		
		public virtual FormatType[] OutputTypes(FormatType input, NeonOptions settings) {
			return null;
		}
		
		public virtual FormatCodec ConvertStream(FormatType input, FormatType output, NeonOptions settings) {
			return null;
		}
		
		public virtual bool HandlesProcessing(FormatType format, NeonOptions settings, FormatType next) {
			return false;
		}
		
		public virtual FormatCodec Process(FormatType input, NeonOptions settings, FormatType next) {
			return null;
		}
	}
}

