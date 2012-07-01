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
			Matroska,
			WAV
			
		}
		
		public enum FormatCodec {
			Unknown,
			
			Custom,
			
			// Video
			AVC,
			VC1,
			MPEG2,
			
			FLAC,
			PCM,
			
			SRT
		}
		
		public FormatType(FormatCodec codec) : this(FormatContainer.None, codec) {
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
			
			FormatContainer econt;
			if(Enum.TryParse<FormatContainer>(container, out econt)) {
				containerString = null;
				Container = econt;
			}
			
			FormatCodec ecodec;
			if(Enum.TryParse<FormatCodec>(codec, out ecodec)) {
				codecString = null;
				Codec = ecodec;
			}
		}
		
		public FormatType(string container, FormatType[] items) : this(FormatContainer.Custom, items) {
			containerString = container;
		}
		
		public bool IsRawContainer() {
			return Container == FormatContainer.None || FormatHandler.AutoIsRawCodec(this);
		}
		
		public FormatType IsRawCodec() {
			FormatType outtype;
			bool res = FormatHandler.AutoIsRawCodec(this, out outtype);
			if(res) return outtype;
			return null;
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
		
		public object Param {
			get;
			set;
		}
	}
}

