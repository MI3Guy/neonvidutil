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
			if(steps == null) {
				throw new ApplicationException("No conversion was found between the source and output.");
			}
		}
		
		private FormatCodec[] steps;
		
		public int Length {
			get { return steps.Length; }
		}
		
		public FormatCodec this[int index] {
			get { return steps[index]; }
			set { steps[index] = value; }
		}
		
		public bool Success {
			get;
			protected set;
		}
		
		public void Run(string infile, string outfile) {
			List<Stream> streams = new List<Stream>();
			List<EncodeStepHandler> stepHandlers = new List<EncodeStepHandler>();
			
			Stream lastStream = null;
			FileStream inStream = File.OpenRead(infile);
			streams.Add(inStream);
			lastStream = inStream;
			
			for(int i = 0; i < steps.Length; ++i) {
				Thread t = new Thread(new ParameterizedThreadStart(StepThreadCall));
				Stream s = steps[i].InitConvertData(lastStream, (i == steps.Length - 1) ? outfile : null);
				streams.Add(s);
				
				EncodeStepHandler stepHandler = new EncodeStepHandler(lastStream, s, steps[i], t, i);
				stepHandlers.Add(stepHandler);
				stepHandler.RunThread();
				lastStream = s;
			}
			
			try {
				foreach(EncodeStepHandler handler in stepHandlers) {
					handler.JoinThread();
				}
			}
			catch(Exception ex) {
				NeAPI.Output(ex.Message);
				NeAPI.Output(ex.InnerException.Message);
				foreach(EncodeStepHandler handler in stepHandlers) {
					handler.AbortThread();
				}
				Success = false;
			}
			
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
			graph.Add(input, new EncodeNode { Cost = 0, Previous = FormatType.None, Using = null });
			bool hasUpdated = true;
			
			while(hasUpdated) {
				hasUpdated = false;
				
				Dictionary<FormatType, EncodeNode> NewItems = new Dictionary<FormatType, EncodeNode>();
				
				foreach(KeyValuePair<FormatType, EncodeNode> graphItem in graph) {
					foreach(KeyValuePair<string, FormatHandler> handler in PluginHelper.AllHandlers) {
						
						IEnumerable<ConversionInfo> conversions = handler.Value.FindConversionTypes(graphItem.Key);
						if(conversions != null) {
							foreach(ConversionInfo conversion in conversions) {
								EncodeNode node = new EncodeNode {
									Cost = graphItem.Value.Cost + 1,
									Previous = graphItem.Key,
									Using = handler.Value,
									Conversion = conversion
								};
								
								if(!NewItems.ContainsKey(conversion.OutFormatType) && (!graph.ContainsKey(conversion.OutFormatType) || graph[conversion.OutFormatType].Cost > node.Cost + 1)) {
									NewItems.Add(conversion.OutFormatType, node);
									hasUpdated = true;
								}
								else {
									if(!graph.ContainsKey(conversion.OutFormatType) && NewItems[conversion.OutFormatType].Cost > node.Cost + 1) {
										NewItems[conversion.OutFormatType] = node;
										hasUpdated = true;
									}
								}
							}
						}
						
						
					}
				}
				
				foreach(KeyValuePair<FormatType, EncodeNode> updItem in NewItems) {
					if(graph.ContainsKey(updItem.Key)) {
						graph[updItem.Key] = updItem.Value;
					}
					else {
						graph.Add(updItem.Key, updItem.Value);
					}
				}
				
			}
			
			
			if(graph.ContainsKey(output)) {
				Stack<FormatCodec> path = new Stack<FormatCodec>();
				
				FormatType next = FormatType.None;
				FormatType curr = output;
				FormatHandler processor;
				while(!graph[curr].Previous.Equals(FormatType.None)) {
					processor = PluginHelper.FindProcessor(curr, next);
					if(processor != null) {
						path.Push(processor.Process(curr, next));
					}
					
					path.Push(graph[curr].Using.ConvertStream(graph[curr].Conversion));
					next = curr;
					curr = graph[curr].Previous;
				}
				
				processor = PluginHelper.FindProcessor(curr, next);
				if(processor != null) {
					path.Push(processor.Process(curr, next));
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
			
			public ConversionInfo Conversion {
				get;
				set;
			}
		}
		
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			foreach(FormatCodec codec in steps) {
				sb.AppendLine(codec.GetType().Name);
			}
			return sb.ToString();
		}
	}
}

