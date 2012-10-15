using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace NeonVidUtil.Core {
	public static class PluginHelper {
		static PluginHelper() {
			DirectoryInfo directory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins"));
			FileInfo[] files = directory.GetFiles("*FormatHandler.dll");
			foreach(FileInfo file in files) {
				Console.WriteLine("Loading Plugin: {0}", file.Name);
				string className = Path.GetFileNameWithoutExtension(file.Name);
				try {
					Assembly assembly = Assembly.LoadFrom(file.FullName);
					Type type = assembly.GetType("NeonVidUtil.Plugin." + className + "." + className);

					FormatHandler instance = (FormatHandler)Activator.CreateInstance(type);
					allHandlers.Add(instance.GetType().Name, instance);
				}
				catch(Exception ex) {
					Console.WriteLine("Error loading plugin {0}: {1}", file.Name, ex.Message);
				}
			}
			
			Console.WriteLine();
			Console.WriteLine();
		}
		
		private static Dictionary<string,FormatHandler> allHandlers = new Dictionary<string,FormatHandler>();
		public static IEnumerable<KeyValuePair<string, FormatHandler>> AllHandlers {
			get { return allHandlers; }
		}
		
		public static void AutoOutputHandlerInfo() {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				IEnumerable<ConversionInfo> conversions = kvp.Value.Conversions;
				IEnumerable<ProcessingInfo> processes = kvp.Value.Processes;
				
				if(conversions.Count() > 0 || processes.Count() > 0) {
					NeAPI.Output(kvp.Key);
					
					foreach(ConversionInfo conversion in conversions) {
						NeAPI.Output("\t{0}\t=>\t{1}", conversion.InFormatType.CodecString, conversion.OutFormatType.CodecString);
					}
					
					foreach(ProcessingInfo process in processes) {
						NeAPI.Output("\t{0}\t:\t{1}", process.HandledType.CodecString, process.Description);
					}
					
					NeAPI.Output("");
				}
			}
		}
		
		public static FormatType AutoReadFileInfo(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				FormatType type = kvp.Value.ReadFileInfo(file);
				if(!type.Equals(FormatType.None)) {
					return type;
				}
			}
			return FormatType.None;
		}
		
		public static FormatType AutoGenerateOutputType(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				FormatType type = kvp.Value.GenerateOutputType(file);
				if(!type.Equals(FormatType.None)) {
					return type;
				}
			}
			return FormatType.None;
		}
		
		public static FormatHandler FindConverter(ConversionInfo conversion, out ConversionInfo updatedConversion) {
			ConversionInfo conversionCandidate;
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				object testparam = kvp.Value.HandlesConversion(conversion, out conversionCandidate);
				if(testparam != null) {
					updatedConversion = conversionCandidate;
					return kvp.Value;
				}
			}
			updatedConversion = conversion;
			return null;
		}
		
		public static FormatHandler FindProcessor(FormatType input, FormatType next) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				if(kvp.Value.HandlesProcessing(input, next)) {
					return kvp.Value;
				}
			}
			return null;
		}
		
		public static bool AutoIsRawFormat(FormatType type) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				if(kvp.Value.IsRawFormat(type)) return true;
			}
			return false;
		}
		
		public static bool AutoFindRawFormatContainer(FormatType type, out FormatType outtype) {
			foreach(KeyValuePair<string, FormatHandler> kvp in allHandlers) {
				FormatType temp;
				if(kvp.Value.FindRawFormatContainer(type, out temp)) {
					outtype = temp;
					return true;
				}
			}
			outtype = FormatType.None;
			return false;
		}
		
		private const string FormatHandlerPostfix = "FormatHandler";
		public static string PluginFullName(string name) {
			return name + FormatHandlerPostfix;
		}
		
		public static string PluginShortName(string name) {
			if(name.EndsWith(FormatHandlerPostfix)) {
				return name.Substring(0, name.Length - FormatHandlerPostfix.Length);
			}
			return null;
		}
		
		public static void RemovePlugin(string handler) {
			allHandlers.Remove(handler);
		}
	}
}

