using System;
using System.Collections.Generic;
using NeonVidUtil.Core;
using NDesk.Options;
using System.Threading;
using System.Linq;

namespace NeonVidUtil {
	public static class Program {
		public static int Main(string[] args) {
#if DEBUG
			//args = new string[] { "/media/john/EXTRADATA4/Videos/NeTestVideos/test.thd", "test.wav" };
			//args = new string[] { "--plugin-ignore=WAV", "/media/john/EXTRADATA4/Videos/NeTestVideos/test.wav", "test.wv" };
			//args = new string[] { "test.wv", "test.wav" };
			//args = new string[] { "--plugin-ignore=WAV", "--bitrate=640k", "/media/john/EXTRADATA4/Videos/NeTestVideos/test.thd", "test.ac3" };
			//args = new string[] { "--plugin-ignore=WAV", "--bitrate=640k", "/media/john/EXTRADATA4/Videos/NeTestVideos/test.thd", "test.eac3" };
			args = new string[] { "--plugin-ignore=WAV", "--bitrate=768k", "/media/john/EXTRADATA4/Videos/NeTestVideos/test.thd", "test.dts" };
			//args = new string[] { "--plugin-ignore=WAV", "/media/john/EXTRADATA4/Videos/NeTestVideos/test.wav", "test.wav" };
			//args = new string[] { "/media/john/EXTRADATA4/Videos/NeTestVideos/test.mka", "test.wav" };
#endif
			
			Console.WriteLine("Neon VidUtil pre relase test");
			if(Type.GetType("Mono.Runtime") != null) {
				Console.WriteLine("Detected .NET Runtime: Mono {0}", Environment.Version);
			} else {
				Console.WriteLine("Unknown .NET Runtime: Version {0}", Environment.Version);
				Console.WriteLine("Probably Microsoft.NET");
			}
			
			NeAPI.UI = new CommandLineUI();
			
			NeonOptions Settings = NeAPI.Settings;
			
			string inFileName = null;
			string outFileName = null;
			bool show_help = false;
			bool show_valid_bitrates = false;
			
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
				{ "b|bitrate=", "Specify the bitrate used for lossy audio. See --valid-bitrates.",
					b => {
						Settings["FFmpeg", "bitrate"] = b;
					}
				},
				{ "h|help", "show this message",
					v => show_help = (v != null)
				},
				{ "valid-bitrates", "Show valid bitrates for different audio types.",
					v => show_valid_bitrates = (v != null)
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

			if(show_valid_bitrates) {
				Console.WriteLine("NeonVidUtil Valid Bitrates");
				Console.WriteLine("Note: These lists may not be complete and are for reference only.");
				Console.WriteLine();
				Console.WriteLine("AC3");
				Console.WriteLine("\t128k / 128000");
				Console.WriteLine("\t448k / 448000");
				Console.WriteLine("\t640k / 640000");
				Console.WriteLine();
				Console.WriteLine("EAC3");
				Console.WriteLine("\tUnknown. I was unable to find a list.");
				Console.WriteLine();
				Console.WriteLine("DTS");
				Console.WriteLine("\t768k / 768000");
				Console.WriteLine("\t1536k / 1536000");

				return 0;
			}
			
			if(inFileName == null) {
				PluginHelper.AutoOutputHandlerInfo();
				return 0;
			}
			
			FormatType inft = PluginHelper.AutoReadFileInfo(inFileName);
			if(inft.Equals(FormatType.None)) {
				Console.WriteLine("Could not identify the input format.");
				return 1;
			}
			
			Console.WriteLine("Input Format:");
			Console.WriteLine(inft);
			
			if(outFileName == null) {
				return 0;
			}
			
			FormatType outft = PluginHelper.AutoGenerateOutputType(outFileName);
			
			EncodePath path;
			try {
				path = new EncodePath(inft, outft, Settings);
			}
			catch(Exception ex) {
				Console.WriteLine(ex.Message);
				return 1;
			}
			
			Console.WriteLine("Conversion Path:");
			Console.WriteLine(path.ToString());
			
			path.Run(inFileName, outFileName);
			
			FormatCodec.DeleteTempFiles();
			
			return path.Success ? 0 : 1;
		}
	}
}

