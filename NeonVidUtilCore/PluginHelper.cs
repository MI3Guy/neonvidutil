using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

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
				NeAPI.Output(kvp.Key);
				kvp.Value.OutputHandlerInfo();
			}
		}
		
		public static FormatType AutoReadInfo(string file, NeonOptions settings) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				FormatType type = kvp.Value.ReadInfo(file, settings);
				if(type != null) {
					return type;
				}
			}
			return null;
		}
		
		public static FormatType AutoGenerateOutputType(string file, NeonOptions settings) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				FormatType type = kvp.Value.GenerateOutputType(file, settings);
				if(type != null) {
					return type;
				}
			}
			return null;
		}
		
		public static FormatHandler FindConverter(FormatType input, FormatType output, NeonOptions settings) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				object testparam = kvp.Value.HandlesConversion(input, output, settings);
				if(testparam != null) {
					return kvp.Value;
				}
			}
			return null;
		}
		
		public static FormatHandler FindProcessor(FormatType input, NeonOptions settings, FormatType next) {
			foreach(KeyValuePair<string, FormatHandler> kvp in AllHandlers) {
				if(kvp.Value.HandlesProcessing(input, settings, next)) {
					return kvp.Value;
				}
			}
			return null;
		}
		
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

