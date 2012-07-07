using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NeonVidUtil.Core {
	public abstract class FormatHandler {
		
		static FormatHandler() {
			DirectoryInfo directory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins"));
			FileInfo[] files = directory.GetFiles("*.dll");
			foreach(FileInfo file in files) {
				string className = Path.GetFileNameWithoutExtension(file.Name);
				try {
					Assembly assembly = Assembly.LoadFrom(file.FullName);
					Type type = assembly.GetType("NeonVidUtil.Plugin." + className + "." + className);

					FormatHandler instance = (FormatHandler)Activator.CreateInstance(type);
					allHandlers.Add(instance.GetType().Name, instance);
				}
				catch(Exception ex) {
					Console.WriteLine("Error loading plugin: {0}", ex);
				}
			}
		}
		
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
		public static IEnumerable<KeyValuePair<string, FormatHandler>> AllHandlers {
			get { return allHandlers; }
		}
		
		// Static methods
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

		
	}
}

