using System;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace NeonVidUtil.Core {
	public struct FormatType {
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
			
			VC1,
			MPEG,
			
			Wave,
			FLAC,
			WavPack,
			TrueHD,
			AC3,
			EAC3,
			DTS
			
		}
		
		public enum FormatCodecType {
			Unknown,
			
			None,
			
			Custom,
			
			// Video
			AVC,
			[Description("VC-1")]
			VC1,
			MPEGVideo,
			
			// Audio
			PCM,
			FLAC,
			WavPack,
			TrueHD,
			[Description("DTS-HD MA")]
			DTSHDMA,
			[Description("AC-3")]
			AC3,
			[Description("EAC-3")]
			EAC3,
			DTS,
			
			// Subtitles
			SRT
		}
		
		public static readonly FormatType None = new FormatType(FormatContainer.None, FormatCodecType.None);
		
		public FormatType(FormatCodecType codec) : this(FormatContainer.None, codec) {
		}
		
		public FormatType(FormatContainer container, FormatCodecType codec) : this() {
			Container = container;
			Codec = codec;
			Items = null;
			Index = -1;
			ID = -1;
		}
		
		public FormatType(FormatContainer container, FormatType[] items) : this() {
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
				Codec = FormatCodecType.None;
			}
			if(containerString == null) {
				Container = FormatContainer.None;
			}
			
			if(container != null) {
				FormatContainer econt;
				if(Enum.TryParse<FormatContainer>(container.Replace("-", "").Replace(" ", ""), out econt)) {
					containerString = null;
					Container = econt;
				}
			}
			
			if(codec != null) {
				FormatCodecType ecodec;
				if(Enum.TryParse<FormatCodecType>(codec.Replace("-", "").Replace(" ", ""), out ecodec)) {
					codecString = null;
					Codec = ecodec;
				}
			}
		}
		
		public FormatType(string container, FormatType[] items) : this(FormatContainer.Custom, items) {
			containerString = container;
			
			if(container == null) {
				throw new ArgumentException();
			}
			
			FormatContainer econt;
			if(Enum.TryParse<FormatContainer>(container.Replace("-", "").Replace(" ", ""), out econt)) {
				containerString = null;
				Container = econt;
			}
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
			return FormatType.None;
		}
		
		public FormatContainer Container {
			get;
			private set;
		}
		
		private string containerString;
		
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
			private set;
		}
		
		private string codecString;
		
		public string CodecString {
			get {
				if(Codec == null) {
					return null;
				}
				else if(Codec == FormatCodecType.Custom) {
					return codecString;
				}
				else {
					FieldInfo fi = typeof(FormatCodecType).GetField(((FormatCodecType)Codec).ToString());
					DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
					if(attributes.Length > 0) {
						return attributes[0].Description;
					}
					else {
						return ((FormatCodecType)Codec).ToString();
					}
				}
			}
		}
		
		public FormatType[] Items {
			get;
			private set;
		}
		
		public int Index {
			get;
			set;
		}
		
		public int ID {
			get;
			set;
		}
		
		public string ExtraInfo {
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
				if(Items.Length != other.Items.Length) return false;
				for(int i = 0; i < Items.Length; ++i) {
					if(!Items[i].Equals(other.Items[i])) {
						return false;
					}
				}
			}
			
			return this.ContainerString == other.ContainerString &&
				this.CodecString == other.CodecString;// &&
				//this.Index == other.Index;
		}
		
		public override int GetHashCode() {
			return 3*ContainerString.GetHashCode() + (CodecString == null ? 0 : 29*CodecString.GetHashCode()) + (Items == null ? 0 : 31*Items.GetHashCode());// + 41*Index.GetHashCode();
		}
		
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			if(!IsRawContainer()) sb.AppendLine(ContainerString);
			if(CodecString != null) {
				sb.Append(ID);
				sb.Append("\t");
				sb.AppendLine(CodecString);
				sb.Append("\t");
				sb.AppendLine(ExtraInfo);
			}
			else {
				foreach(FormatType ft in Items) {
					sb.AppendLine(ft.ToString());
				}
			}
			return sb.ToString();
		}
	}
}

