using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace NeonVidUtil.Core {
	public class EncodePath {
		public EncodePath(FormatType input, FormatType output, NeonOptions settings) {
			steps = FindConvertPath(input, output, settings);
		}
		
		public EncodePath(string path) {
			
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
			
			/*if(!(streams[streams.Count - 1] is FileStream)) {
				using(FileStream fs = File.OpenWrite(outfile)) {
					streams[streams.Count - 1].CopyTo(fs);
				}
			}*/
			
			foreach(Stream s in streams) {
				s.Close();
			}
		}
		
		
		private void StepThreadCall(object o) {
			EncodeStepHandler handler = (EncodeStepHandler)o;
			handler.Run();
		}
		
		
		
		/*private static FormatCodec[] FindConvertPath(FormatType input, FormatType output, NeonOptions settings) {
			Stack<FormatCodec> path = FindConvertPath(input, output, new List<FormatHandler>(), settings);
			return path == null ? null : path.ToArray();
		}*/
		
		private static FormatCodec[] FindConvertPath(FormatType input, FormatType output, NeonOptions settings) {
			Dictionary<FormatType, EncodeNode> graph = new Dictionary<FormatType, EncodeNode>(new FormatTypeComparer());
			graph.Add(input, new EncodeNode { Cost = 0, Previous = null, Using = null });
			bool hasUpdated = true;
			
			while(hasUpdated) {
				hasUpdated = false;
				
				Dictionary<FormatType, EncodeNode> NewItems = new Dictionary<FormatType, EncodeNode>();
				
				foreach(KeyValuePair<FormatType, EncodeNode> graphItem in graph) {
					foreach(KeyValuePair<string, FormatHandler> handler in PluginHelper.AllHandlers) {
						
						FormatType[] formatOutputTypes = handler.Value.OutputTypes(graphItem.Key, settings);
						if(formatOutputTypes != null) {
							foreach(FormatType formatOutputType in formatOutputTypes) {
								EncodeNode node = new EncodeNode { Cost = graphItem.Value.Cost + 1, Previous = graphItem.Key, Using = handler.Value };
								if(!NewItems.ContainsKey(formatOutputType) && (!graph.ContainsKey(formatOutputType) || graph[formatOutputType].Cost > node.Cost + 1)) {
									NewItems.Add(formatOutputType, node);
									hasUpdated = true;
								}
								else {
									if(!graph.ContainsKey(formatOutputType) && NewItems[formatOutputType].Cost > node.Cost + 1) {
										NewItems[formatOutputType] = node;
										hasUpdated = true;
									}
								}
							}
						}
						
						
					}
				}
				
				foreach(KeyValuePair<FormatType, EncodeNode> updItem in NewItems) {
					if(!graph.ContainsKey(updItem.Key)) {
						graph.Add(updItem.Key, updItem.Value);
					}
					else {
						graph[updItem.Key] = updItem.Value;
					}
				}
				
			}
			
			
			if(graph.ContainsKey(output)) {
				Stack<FormatCodec> path = new Stack<FormatCodec>();
				
				FormatType next = null;
				FormatType curr = output;
				FormatHandler processor;
				while(graph[curr].Previous != null) {
					processor = PluginHelper.FindProcessor(curr, settings, next);
					if(processor != null) {
						path.Push(processor.Process(curr, settings, next));
					}
					
					path.Push(graph[curr].Using.ConvertStream(graph[curr].Previous, curr, settings));
					next = curr;
					curr = graph[curr].Previous;
				}
				
				processor = PluginHelper.FindProcessor(curr, settings, next);
				if(processor != null) {
					path.Push(processor.Process(curr, settings, next));
				}
				
				return path.ToArray();
			}
			else {
				return null;
			}
		}
		
		private class EncodeNode {
			public int Cost {
				get;
				set;
			}
			
			public FormatType Previous {
				get;
				set;
			}
			
			public FormatHandler Using {
				get;
				set;
			}
		}
	}
}

