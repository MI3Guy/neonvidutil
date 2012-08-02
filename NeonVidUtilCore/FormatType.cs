using System;

namespace NeonVidUtil.Core {
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
			
			Matroska,
			
			
			FLAC,
			TrueHD,
			WAV
			
		}
		
		public enum FormatCodecType {
			Unknown,
			
			Custom,
			
			// Video
			AVC,
			VC1,
			MPEG2,
			
			// Audio
			FLAC,
			PCM,
			TrueHD,
			
			// Subtitles
			SRT
		}
		
		public FormatType(FormatCodecType codec) : this(FormatContainer.None, codec) {
		}
		
		public FormatType(FormatContainer container, FormatCodecType codec) {
			Container = container;
			Codec = codec;
			Items = null;
			Index = -1;
			ID = -1;
		}
		
		public FormatType(FormatContainer container, FormatType[] items) {
			Container = container;
			Codec = null;
			Items = items;
			Index = -1;
			ID = -1;
		}
		
		public FormatType(string codec) : this(null, codec) {
		}
		
		public FormatType(string container, string codec) : this(FormatContainer.Custom, FormatCodecType.Custom) {
			containerString = container;
			codecString = codec;
			
			if(codecString == null) {
				throw new ArgumentException();
			}
			else if(containerString == null) {
					Container = FormatContainer.None;
				}
			
			if(container != null) {
				FormatContainer econt;
				if(Enum.TryParse<FormatContainer>(container.Replace("-", ""), out econt)) {
					containerString = null;
					Container = econt;
				}
			}
			
			FormatCodecType ecodec;
			if(Enum.TryParse<FormatCodecType>(codec.Replace("-", ""), out ecodec)) {
				codecString = null;
				Codec = ecodec;
			}
		}
		
		public FormatType(string container, FormatType[] items) : this(FormatContainer.Custom, items) {
			containerString = container;
		}
		
		public bool IsRawContainer() {
			return Container == FormatContainer.None || PluginHelper.AutoIsRawCodec(this);
		}
		
		public FormatType IsRawCodec() {
			FormatType outtype;
			if(ContainerString == null) return this;
			bool res = PluginHelper.AutoIsRawCodec(this, out outtype);
			if(res) {
				return outtype;
			}
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
		
		public FormatCodecType? Codec {
			get;
			protected set;
		}
		
		protected string codecString;
		
		public string CodecString {
			get {
				if(Codec == null) {
					return null;
				}
				else if(Codec == FormatCodecType.Custom) {
						return codecString;
					}
					else {
						return ((FormatCodecType)Codec).ToString();
					}
			}
		}
		
		public FormatType[] Items {
			get;
			protected set;
		}
		
		public int Index {
			get;
			set;
		}
		
		public int ID {
			get;
			set;
		}
		
		public override bool Equals(object obj) {
			if(!(obj is FormatType)) {
				return false;
			}
			FormatType other = (FormatType)obj;
			
			if((this.Items == null && other.Items != null) || (this.Items != null && other.Items == null)) {
				return false;
			}
			else if(this.Items != null && other.Items != null) {
				
			}
			
			return this.ContainerString == other.ContainerString &&
				this.CodecString == other.CodecString &&
				this.Index == other.Index;
		}
		
		public override int GetHashCode() {
			return 3*ContainerString.GetHashCode() + (CodecString == null ? 0 : 29*CodecString.GetHashCode()) + (Items == null ? 0 : 31*Items.GetHashCode()) + 41*Index.GetHashCode();
		}
	}
}

