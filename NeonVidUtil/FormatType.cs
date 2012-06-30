using System;

namespace NeonVidUtil {
	public class FormatType {
		/// <summary>
		/// Container specifies the container format.
		/// </summary>
		public enum FormatContainer {
			/// <summary>
			/// Unknown is an invalid container type.
			/// </summary>
			Unknown,
			
			/// <summary>
			/// Implies that the data is a raw stream.
			/// </summary>
			None,
			
			/// <summary>
			/// Specify value with string.
			/// </summary>
			Custom,
			
			FLAC,
			MKV,
			WAV
			
		}
		
		public enum FormatCodec {
			Unknown,
			
			Custom,
			
			FLAC,
			PCM,
			
		}
		
		public FormatType(FormatCodec codec) : this(FormatContainer.None, FormatCodec.Unknown) {
		}
		
		public FormatType(FormatContainer container, FormatCodec codec) {
			Container = container;
			Codec = codec;
			Items = null;
		}
		
		public FormatType(FormatContainer container, FormatType[] items) {
			Container = container;
			Codec = null;
			Items = items;
		}
		
		public FormatType(string codec) : this(null, codec) {
		}
		
		public FormatType(string container, string codec) : this(FormatContainer.Custom, FormatCodec.Custom) {
			containerString = container;
			codecString = codec;
			
			if(codecString == null) {
				throw new ArgumentException();
			}
			else if(containerString == null) {
				Container = FormatContainer.None;
			}
		}
		
		public FormatType(string container, FormatType[] items) : this(FormatContainer.Custom, items) {
			containerString = container;
		}
		
		
		public FormatContainer Container {
			get;
			protected set;
		}
		
		protected string containerString;
		
		public string ContainerString {
			get {
				if(Container == FormatContainer.Custom) {
					return containerString;
				}
				else {
					return Container.ToString();
				}
			}
		}
		
		
		public FormatCodec? Codec {
			get;
			protected set;
		}
		
		protected string codecString;
		
		public string CodecString {
			get {
				if(Codec == null) {
					return null;
				}
				else if(Codec == FormatCodec.Custom) {
					return codecString;
				}
				else {
					return ((FormatCodec)Codec).ToString();
				}
			}
		}
		
		public FormatType[] Items {
			get;
			protected set;
		}
		
	}
}

