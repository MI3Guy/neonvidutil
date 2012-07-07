using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NeonVidUtil.Core {
	public class EncodePath {
		public EncodePath(FormatType input, FormatType output) {
			steps = FindConvertPath(input, output);
			formats = new FormatType[steps.Length + 1];
			formats[0] = input;
			for(int i = 0; i < steps.Length; ++i) {
				formats[i + 1] = steps[i].Output;
			}
		}
		
		private EncodeStep[] steps;
		private FormatType[] formats;
		
		public int Length {
			get { return steps.Length; }
		}
		
		public EncodeStep this[int index] {
			get { return steps[index]; }
			set { steps[index] = value; }
		}
		
		public void Run(string infile, string outfile) {
			Stream[] streams = new Stream[steps.Length + 1];
			streams[0] = File.OpenRead(infile);
			streams[streams.Length - 1] = File.OpenWrite(outfile);
			for(int i = 1; i < streams.Length - 1; ++i) {
				streams[i] = new MemoryStream();
			}
			
			Thread[] threads = new Thread[steps.Length];
			for(int i = 0; i < steps.Length; ++i) {
				threads[i] = new Thread(new ParameterizedThreadStart(StepThreadCall));
				FormatCodec codec = steps[i].Handler.ConvertStream(formats[i], formats[i + 1], null);
				threads[i].Start(new EncodeStepHandler(streams[i], streams[i + 1], codec));
			}
			
			for(int i = 0; i < steps.Length; ++i) {
				threads[i].Join();
			}
		}
		
		
		private void StepThreadCall(object o) {
			EncodeStepHandler handler = (EncodeStepHandler)o;
			handler.Run();
		}
		
		
		
		
		
		
		
		public static FormatType AutoReadInfo(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in FormatHandler.AllHandlers) {
				FormatType type = kvp.Value.ReadInfo(file);
				if(type != null) {
					return type;
				}
			}
			return null;
		}
		
		public static FormatType AutoGenerateOutputType(string file) {
			foreach(KeyValuePair<string, FormatHandler> kvp in FormatHandler.AllHandlers) {
				FormatType type = kvp.Value.GenerateOutputType(file);
				if(type != null) {
					return type;
				}
			}
			return null;
		}
		
		public static FormatHandler FindConverter(FormatType input, FormatType output, string option) {
			foreach(KeyValuePair<string, FormatHandler> kvp in FormatHandler.AllHandlers) {
				object testparam = kvp.Value.HandlesConversion(input, output, option);
				if(testparam != null) {
					return kvp.Value;
				}
			}
			return null;
		}
		
		private static EncodeStep[] FindConvertPath(FormatType input, FormatType output) {
			Stack<EncodeStep> path = FindConvertPath(input, output, new List<FormatHandler>());
			return path == null ? null : path.ToArray();
		}
		
		private static Stack<EncodeStep> FindConvertPath(FormatType input, FormatType output, List<FormatHandler> previous) {
			foreach(KeyValuePair<string, FormatHandler> kvp in FormatHandler.AllHandlers) {
				if(previous.IndexOf(kvp.Value) != -1) {
					continue;
				} // Prevent infinite recursion via conversion back and forth.
				
				FormatType[] outputTypes = kvp.Value.OutputTypes(input);
				if(outputTypes == null) {
					continue;
				}
				
				// Stop if path found.
				foreach(FormatType outputType in outputTypes) {
					if(outputType.Equals(output)) {
						Stack<EncodeStep> ret = new Stack<EncodeStep>();
						ret.Push(new EncodeStep(kvp.Value, output));
						return ret;
					}
				}
				// Recurse for path.
				foreach(FormatType outputType in outputTypes) {
					previous.Add(kvp.Value);
					Stack<EncodeStep> subPath = FindConvertPath(outputType, output, previous);
					previous.Remove(kvp.Value);
					if(subPath != null) {
						subPath.Push(new EncodeStep(kvp.Value, outputType));
						return subPath;
					}
				}
			}
			return null;
		}
	}
}

