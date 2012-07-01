using System;
using System.Collections.Generic;

namespace NeonVidUtil {
	public abstract class FormatHandler {
		protected FormatHandler() {
			allHandlers.Add(this.GetType().Name, this);
		}
		
		public virtual bool IsRawCodec(FormatType type) {
			return false;
		}
		
		public virtual bool IsRawCodec(FormatType type, out FormatType outtype) {
			outtype = null;
			return IsRawCodec(type);
		}
		
		public virtual FormatType ReadInfo(string file) {
			return null;
		}
		
		public virtual FormatType GenerateOutputType(string file) {
			return null;
		}
		
		public virtual object HandlesConversion(FormatType input, FormatType output, string option) {
			return null;
		}
		
		public virtual FormatType[] OutputTypes(FormatType input) {
			return null;
		}
		
		public virtual FormatCodec ConvertStream(FormatType input, FormatType output, string option) {
			return null;
		}
		
		public virtual bool HandlesProcessing(FormatType format, string name) {
			return false;
		}
		
		public virtual FormatCodec Process(FormatType input, string name) {
			return null;
		}
		
		private static Dictionary<string,FormatHandler> allHandlers = new Dictionary<string,FormatHandler>();
		
		public static bool AutoIsRawCodec(FormatType type) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				if(kvp.Value.IsRawCodec(type)) return true;
			}
			return false;
		}
		
		public static bool AutoIsRawCodec(FormatType type, out FormatType outtype) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				FormatType temp;
				if(kvp.Value.IsRawCodec(type, out temp)) {
					outtype = temp;
					return true;
				}
			}
			outtype = null;
			return false;
		}

		public static FormatType AutoReadInfo(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				FormatType type = kvp.Value.ReadInfo(file);
				if(type != null) return type;
			}
			return null;
		}
		
		public static FormatType AutoGenerateOutputType(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				FormatType type = kvp.Value.GenerateOutputType(file);
				if(type != null) return type;
			}
			return null;
		}
		
		public static FormatHandler FindConverter(FormatType input, FormatType output, string option) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				object testparam = kvp.Value.HandlesConversion(input, output, option);
				if(testparam != null) {
					return kvp.Value;
				}
			}
			return null;
		}
		
		public static FormatHandler[] FindConvertPath(FormatType input, FormatType output) {
			return null;
		}
	}
}

