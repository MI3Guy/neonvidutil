using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NeonVidUtil.Core {
	public class EncodePath {
		public EncodePath(FormatType input, FormatType output) {
			steps = FindConvertPath(input, output);
		}
		
		private FormatCodec[] steps;
		
		public int Length {
			get { return steps.Length; }
		}
		
		public FormatCodec this[int index] {
			get { return steps[index]; }
			set { steps[index] = value; }
		}
		
		public void Run(string infile, string outfile) {
			//Stream[] streams = new Stream[steps.Length + 1];
			//streams[0] = File.OpenRead(infile);
			//streams[streams.Length - 1] = File.OpenWrite(outfile);
			
			List<Stream> streams = new List<Stream>();
			
			List<Thread> threads = new List<Thread>();
			
			Stream lastStream = null;
			FileStream inStream = File.OpenRead(infile);
			streams.Add(inStream);
			lastStream = inStream;
			
			for(int i = 0; i < steps.Length; ++i) {
				Thread t = new Thread(new ParameterizedThreadStart(StepThreadCall));
				threads.Add(t);
				Stream s = steps[i].InitConvertData(lastStream, (i == steps.Length - 1) ? outfile : null);
				streams.Add(s);
				
				t.Start(new EncodeStepHandler(lastStream, s, steps[i]));
				lastStream = s;
			}
			
			foreach(Thread t in threads) {
				t.Join();
			}
			
			if(!(streams[streams.Count - 1] is FileStream)) {
				using(FileStream fs = File.OpenWrite(outfile)) {
					streams[streams.Count - 1].CopyTo(fs);
				}
			}
			
			foreach(Stream s in streams) {
				s.Close();
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
		
		public static FormatHandler FindProcessor(FormatType input, string name, FormatType next) {
			foreach(KeyValuePair<string, FormatHandler> kvp in FormatHandler.AllHandlers) {
				if(kvp.Value.HandlesProcessing(input, name, next)) {
					return kvp.Value;
				}
			}
			return null;
		}
		
		private static FormatCodec[] FindConvertPath(FormatType input, FormatType output) {
			Stack<FormatCodec> path = FindConvertPath(input, output, new List<FormatHandler>());
			return path == null ? null : path.ToArray();
		}
		
		private static Stack<FormatCodec> FindConvertPath(FormatType input, FormatType output, List<FormatHandler> previous) {
			// Don't look for path if only processor is needed.
			if(input.Equals(output)) {
				FormatHandler processor = FindProcessor(input, null, null);
				if(processor != null) {
					Stack<FormatCodec> ret = new Stack<FormatCodec>();
					ret.Push(processor.Process(input, null, null));
				}
				return null;
			}
			
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
						Stack<FormatCodec> ret = new Stack<FormatCodec>();
						
						// Last
						FormatHandler processor = FindProcessor(output, null, null);
						if(processor != null) {
							ret.Push(processor.Process(output, null, null));
						}
						
						ret.Push(kvp.Value.ConvertStream(input, output, null));
						
						// Before
						processor = FindProcessor(input, null, output);
						if(processor != null) {
							ret.Push(processor.Process(input, null, outputType));
						}
						return ret;
					}
				}
				// Recurse for path.
				foreach(FormatType outputType in outputTypes) {
					previous.Add(kvp.Value);
					Stack<FormatCodec> subPath = FindConvertPath(outputType, output, previous);
					previous.Remove(kvp.Value);
					if(subPath != null) {
						subPath.Push(kvp.Value.ConvertStream(input, outputType, null));
						
						// Happens before the conversion above
						FormatHandler processor = FindProcessor(input, null, outputType);
						if(processor != null) {
							subPath.Push(processor.Process(input, null, outputType));
						}
						
						return subPath;
					}
				}
			}
			return null;
		}
	}
}

