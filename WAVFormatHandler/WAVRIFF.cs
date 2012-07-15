using System;
using System.IO;
using System.Text;

namespace NeonVidUtil.Plugin.WAVFormatHandler {
	public class WAVRIFF {
		public WAVRIFF(BinaryReader reader) {
			ckID = Encoding.ASCII.GetString(reader.ReadBytes(4));
			size = reader.ReadUInt32();
			WAVEID = Encoding.ASCII.GetString(reader.ReadBytes(4));
		}
		
		public WAVRIFF() {
			
		}
		
		public string ckID;
		public uint size;
		public string WAVEID;
		
		
		public void WriteTo(BinaryWriter writer) {
			writer.Write(Encoding.ASCII.GetBytes(ckID));
			writer.Write(size);
			writer.Write(Encoding.ASCII.GetBytes(WAVEID));
		}
		
		public const uint sizeValPartial = 4;
		
	}
}

