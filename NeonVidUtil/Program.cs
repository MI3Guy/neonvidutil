using System;
using System.Collections.Generic;
using NeonVidUtil.Core;
using NDesk.Options;
using System.Threading;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			//string[] args = { "/home/john/Videos/vid2.mkv", "test.flac" };
			//string[] args = { "/home/john/Projects/audio.thd", "test.flac" };
			string[] args = { "/home/john/Projects/tmp4c280100.tmp", "test.flac" };
			//string[] args = { "/home/john/Videos/vid2.mkv" };
			
			
			Console.WriteLine("Neon VidUtil pre relase test");
			if(Type.GetType("Mono.Runtime") != null) {
				Console.WriteLine("Detected .NET Runtime: Mono {0}", Environment.Version);
			}
			else {
				Console.WriteLine("Unknown .NET Runtime: Version {0}", Environment.Version);
				Console.WriteLine("Probably Microsoft.NET");
			}
			
			NeonOptions Settings = new NeonOptions();
			
			string inFileName = null;
			string outFileName = null;
			bool show_help = false;
			
			OptionSet options = new OptionSet() {
				{ "n|streamindex=", "the index of the stream to use.",
					n => {
						Settings["Core", "streamindex"] = n;
					}
				},
				{ "plugin-ignore=", "the {PLUGIN} to ignore.",
					plugin => {
						PluginHelper.RemovePlugin(PluginHelper.PluginFullName(plugin));
					}
				},
				{ "removepulldown", "Specify whether to remove pulldown.",
					pd => {
						if(pd != null) {
							Settings["VC1", "removepulldown"] = "true";
							Settings["DGPulldown", "removepulldown"] = "true";
						}
					}
				},
				{ "stripbits", "Specify whether to strip zero bits from wav audio.",
					sb => {
						if(sb != null) {
							Settings["WAV", "depth"] = "auto";
						}
					}
				},
				{ "h|help", "show this message",
					v => show_help = (v != null)
				},
				
				{
					"<>",
					v => {
						Console.WriteLine("Default Handler: {0}", v);
						if(v.StartsWith("--set")) {
							string[] parts = v.Substring("--set".Length).Split(new char[] {'='}, 2);
							if(parts.Length != 2) {
								Console.WriteLine("Warning: Error parsing command line argument: {0}.", v);
							}
							else {
								Settings[new NeonOptions.SettingItem(parts[0])] = parts[1];
							}
						}
						else if(inFileName == null) {
							inFileName = v;
						}
						else if(outFileName == null) {
							outFileName = v;
						}
						else {
							Console.WriteLine("Warning: Unknown command line argument: {0}.", v);
						}
					}
				}
			};
			
			List<string> rest = options.Parse(args);
			
			if(show_help) {
				Console.WriteLine("Usage: NeonVidUtil.exe [Options] inputfile outputfile");
				Console.WriteLine("Options:");
				options.WriteOptionDescriptions(Console.Out);
				return 0;
			}
			
			if(inFileName == null) {
				PluginHelper.AutoOutputHandlerInfo();
				return 0;
			}
			
			FormatType inft = PluginHelper.AutoReadInfo(inFileName, Settings);
			Console.WriteLine(inft);
			
			if(outFileName == null) {
				return 0;
			}
			
			FormatType outft = PluginHelper.AutoGenerateOutputType(outFileName, Settings);
			
			/*FormatHandler handler = FormatHandler.FindConverter(inft, outft, null);
			
			System.IO.FileStream infs = System.IO.File.OpenRead(args[0]);
			System.IO.FileStream outfs = System.IO.File.Create(args[1]);
			FormatCodec dec = handler.ConvertStream(inft, outft, null);
			dec.ConvertData(infs, outfs);*/
			
			//FormatHandler[] handlers = EncodePath.FindConvertPath(inft, outft);
			EncodePath path = new EncodePath(inft, outft, Settings);
			
			/*foreach(FormatHandler handler in handlers) {
				//handler.ConvertStream(
			}*/
			
			Console.WriteLine("Conversion Path:");
			Console.WriteLine(path.ToString());
			
			path.Run(args[0], args[1]);
			
			FormatCodec.DeleteTempFiles();
			
			return 0;
		}
	}
}

