using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NeonVidUtil.Core {
	public abstract class FormatHandler {
		
		public virtual Dictionary<FormatType, FormatType> RawFormats {
			get { return new Dictionary<FormatType, FormatType>(); }
		}
		
		public virtual IEnumerable<ConversionInfo> Conversions {
			get { return new ConversionInfo[] {}; }
		}
		
		public virtual IEnumerable<FormatType.FormatContainer> ConversionContainers {
			get { return new FormatType.FormatContainer[] {}; }
		}
		
		public virtual IEnumerable<ProcessingInfo> Processes {
			get { return new ProcessingInfo[] {}; }
		}
		
		public virtual Dictionary<string, FormatType> OutputTypes {
			get { return new Dictionary<string, FormatType>(); }
		}
		
		public virtual bool IsRawFormat(FormatType type) {
			foreach(KeyValuePair<FormatType, FormatType> rawFormat in RawFormats) {
				if(rawFormat.Key.Equals(type) || rawFormat.Value.Equals(type)) {
					return true;
				}
			}
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
		public virtual bool FindRawFormatContainer(FormatType type, out FormatType outtype) {
			foreach(KeyValuePair<FormatType, FormatType> rawFormat in RawFormats) {
				if(rawFormat.Key.Equals(type)) {
					outtype = rawFormat.Value;
					return true;
				}
			}
			
			outtype = FormatType.None;
			return false;
		}
		
		public virtual FormatType GenerateOutputType(string file) {
			try {
				return OutputTypes[Path.GetExtension(file).ToUpper()];
			}
			catch {
				return FormatType.None;
			}
		}
		
		public abstract FormatType ReadFileInfo(string file);
		
		public abstract IEnumerable<ConversionInfo> FindConversionTypes(FormatType input);
		
		public bool HandlesConversion(ConversionInfo conversion) {
			ConversionInfo updatedConversion;
			return HandlesConversion(conversion, out updatedConversion);
		}
		public abstract bool HandlesConversion(ConversionInfo conversion, out ConversionInfo updatedConversion);
		public abstract FormatCodec ConvertStream(ConversionInfo conversion);
		
		public abstract bool HandlesProcessing(FormatType format, FormatType next);
		public abstract FormatCodec Process(FormatType input, FormatType next);
	}
}

