using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LibSndFileSharp {
	public class SoundFileReader : IDisposable {
		public SoundFileReader(Stream input) {
			sfvirtual.read = new LibSndFile.SfVirtualIO.sf_vio_read(VioRead);
			sfvirtual.write = new LibSndFile.SfVirtualIO.sf_vio_write(VioWrite);
			
			if(input.CanSeek) {
				sfvirtual.get_filelen = new LibSndFile.SfVirtualIO.sf_vio_get_filelen(VioGetFileLength);
				sfvirtual.seek = new LibSndFile.SfVirtualIO.sf_vio_seek(VioSeek);
				sfvirtual.tell = new LibSndFile.SfVirtualIO.sf_vio_tell(VioTell);
			}
			
			
			
			sndfile = LibSndFile.sf_open_virtual(ref sfvirtual, LibSndFile.SFM_READ, ref info, IntPtr.Zero);
			
		}
		
		public void Dispose() {
			if(sndfile != IntPtr.Zero) {
				LibSndFile.sf_close(sndfile);
				sndfile = IntPtr.Zero;
			}
		}
		
		private LibSndFile.SfVirtualIO sfvirtual;
		private LibSndFile.SfInfo info;
		private IntPtr sndfile;
		protected Stream input;
		
		public int Channels {
			get { return info.channels; }
		}
		
		public unsafe long ReadFrames(int[] buff, int frames) {
			if(buff.Length < frames * info.channels) {
				throw new IndexOutOfRangeException();
			}
			
			fixed(int* ptrbuff = buff) {
				IntPtr ptr = new IntPtr(ptrbuff);
				return LibSndFile.sf_readf_int(sndfile, ptr, frames);
			}
		}
		
		private long VioGetFileLength(IntPtr userData) {
			return input.Length;
		}
		
		private long VioSeek(long offset, int whence, IntPtr user_data) {
			SeekOrigin origin;
			switch(whence) {
				case 0: // SEEK_SET:
					origin = SeekOrigin.Begin;
					break;
					
				case 1:
					origin = SeekOrigin.Current;
					break;
					
				case 2:
					origin = SeekOrigin.End;
					break;
						
				default:
					return -1;
			}
			
			return input.Seek(offset, origin);
		}
		
		private long VioRead(IntPtr ptr, long count, IntPtr user_data) {
			byte[] buff = new byte[count];
			int len = input.Read(buff, 0, (int)count);
			Marshal.Copy(buff, 0, ptr, (int)count);
			return len;
		}
		
		private long VioWrite(IntPtr ptr, long count, IntPtr user_data) {
			byte[] buff = new byte[count];
			Marshal.Copy(ptr, buff, 0, (int)count);
			input.Write(buff, 0, (int)count);
			return count;
		}
		
		private long VioTell(IntPtr userData) {
			return input.Position;
		}
		
	}
}

