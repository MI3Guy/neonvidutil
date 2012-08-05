using System;
using System.Collections.Generic;
using NeonVidUtil.Core;
using NDesk.Options;
using System.Threading;

namespace NeonVidUtil {
	public static class Program {
		public static int Main() {
			/*using(System.IO.FileStream ifs = System.IO.File.OpenRead("/home/john/Projects/tmp4c280100.tmp")) {
				using(System.IO.FileStream ofs = System.IO.File.Open("test.wav", System.IO.FileMode.Create)) {
					//FFMpegFormatHandler.FFmpegConvert.ConvertFFmpeg(ifs, "truehd", ofs, "wav", "pcm_s24le");
					//ret = VC1FormatHandler.VC1Conv.VC1ConvRemovePulldown(ifs, ofs);
					CircularStream stream = new CircularStream();
					Thread t1 = new Thread(delegate() {
						ifs.CopyTo(stream);
						stream.MarkEnd();
					});
					
					Thread t2 = new Thread(delegate() {
						stream.CopyTo(ofs);
					});
					
					t1.Start();
					t2.Start();
					
					t1.Join();
					t2.Join();
				}
			}*/
			//FFMpegFormatHandler.FFmpegConvert.ConvertFFmpeg("/home/john/Projects/audio.thd", "truehd", "test.wav", "wav", "pcm_s24le");
			
			//string[] args = { "/home/john/Videos/vid2.mkv", "test.flac" };
			//string[] args = { "/home/john/Projects/audio.thd", "test.flac" };
			string[] args = { "/home/john/Projects/tmp4c280100.tmp", "test.wav" };
			
			
			Console.WriteLine("Neon VidUtil pre relase test");
			Console.WriteLine(Environment.Version);
			
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
				Console.WriteLine("Usage: NeonVidUtil [Options] inputfile outputfile");
				Console.WriteLine("Options:");
				options.WriteOptionDescriptions(Console.Out);
				return 0;
			}
			
			FormatType inft = PluginHelper.AutoReadInfo(inFileName, Settings);
			FormatType outft = PluginHelper.AutoGenerateOutputType(outFileName, Settings);
			
			/*FormatHandler handler = FormatHandler.FindConverter(inft, outft, null);
			
			System.IO.FileStream infs = System.IO.File.OpenRead(args[0]);
			System.IO.FileStream outfs = System.IO.File.OpenWrite(args[1]);
			FormatCodec dec = handler.ConvertStream(inft, outft, null);
			dec.ConvertData(infs, outfs);*/
			
			//FormatHandler[] handlers = EncodePath.FindConvertPath(inft, outft);
			EncodePath path = new EncodePath(inft, outft, Settings);
			
			/*foreach(FormatHandler handler in handlers) {
				//handler.ConvertStream(
			}*/
			
			path.Run(args[0], args[1]);
			
			FormatCodec.DeleteTempFiles();
			
			return 0;
		}
	}
}

