using System;
using System.Collections.Generic;
using NeonVidUtil.Core;
using NDesk.Options;
using System.Threading;

using MakeMKVSharp;
using System.Linq;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			/*MakeMKV makemkv = new MakeMKV();
			
			DriveInfo[] di = makemkv.ReadDrives().ToArray();
			
			Console.WriteLine(di.Length);
			
			return 0;*/
			//string[] args = { "--plugin-ignore=WAV", "--streamindex=2", "/home/john/Videos/vid2.mkv", "test.wav" };
			//string[] args = { "/home/john/Projects/audio.thd", "test.flac" };
			//string[] args = { "/home/john/Projects/tmp4c280100.tmp", "test.flac" };
			//string[] args = { "/media/EXTRADATA4/Videos/JAWS/JAWS_t00.mkv" };
			//string[] args = { "--plugin-ignore=WAV", "/home/john/Videos/Main_Movie_t01.mkv", "test.wav" };
			//string[] args = { "--removepulldown", "/home/john/Videos/title00.mkv", "test.m2v" };
			string[] args = { "--plugin-ignore=WAV", "/media/EXTRADATA4/Videos/The_Lady_Vanishes/The_Lady_Vanishes_t00.mkv", "test.wav" };
			

			
			
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
			
			path.Run(inFileName, outFileName);
			
			FormatCodec.DeleteTempFiles();
			
			return 0;
		}
	}
}

