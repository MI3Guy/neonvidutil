using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace NeonVidUtil.Core {
		public class CircularStream : Stream {
		public CircularStream() {
			bufferList = new ConcurrentQueue<byte[]>();
			emptySem = new Semaphore(numItems, numItems);
			fullSem = new Semaphore(0, numItems);
			doneWriting = false;
		}
		const int numItems = 20;
		
		protected byte[] readBuffer;
		protected int readBufferOffset;
		protected ConcurrentQueue<byte[]> bufferList;
		protected Semaphore emptySem;
		protected Semaphore fullSem;
		protected bool doneWriting;
		long writePosition;
		long readPosition;
		
		public override int Read (byte[] buffer, int offset, int count)
		{
			if(readBuffer != null) {
				return HandleRead(readBuffer, buffer, readBufferOffset, offset, count);
			}
			else if(doneWriting) {
				byte[] data;
				if(bufferList.TryDequeue(out data)) {
					return HandleRead(data, buffer, 0, offset, count);
				}
				else {
					return 0;
				}
			}
			else {
				//NeAPI.Output("fillSem.WaitOne()");
				fullSem.WaitOne();
				
				byte[] data;
				while(!bufferList.TryDequeue(out data)) {
					if(doneWriting) {
						break;
					}
					else {
						Thread.Sleep(0);
					}
				}
				
				//NeAPI.Output("emptySem.Release()");
				emptySem.Release();
				
				if(data == null) {
					return 0;
				}
				else {
					return HandleRead(data, buffer, 0, offset, count);
				}
			}
		}
		
		private int HandleRead(byte[] source, byte[] dest, int srcOffset, int destOffset, int destCount) {
			int copyAmount;
			if((source.Length - srcOffset) > destCount)
			{
				copyAmount = destCount;
				Buffer.BlockCopy(source, srcOffset, dest, destOffset, copyAmount);
				
				readBufferOffset = srcOffset + destCount;
				readBuffer = source;
			}
			else {
				copyAmount = source.Length - srcOffset;
				Buffer.BlockCopy(source, srcOffset, dest, destOffset, copyAmount);
				
				readBufferOffset = 0;
				readBuffer = null;
			}
			readPosition += copyAmount;
			return copyAmount;
		}
		
		public override void Write (byte[] buffer, int offset, int count) {
			if(count == 0) {
				return;
			}
			
			byte[] buff2 = new byte[count];
			Buffer.BlockCopy(buffer, offset, buff2, 0, count);
			
			//NeAPI.Output("emptySem.WaitOne()");
			emptySem.WaitOne();
			
			bufferList.Enqueue(buff2);
			
			//NeAPI.Output("fillSem.Release()");
			fullSem.Release();
			
			writePosition += count;
		}
		
		public void MarkEnd() {
			emptySem.WaitOne();
			doneWriting = true;
			fullSem.Release();
		}
		
		public override void Flush ()
		{
			
		}
		
		public override bool CanRead {
			get {
				return true;
			}
		}
		
		public override bool CanWrite {
			get {
				return true;
			}
		}
		
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}
		
		public override long Position {
			get {
				return readPosition;
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		
		public override void SetLength (long value)
		{
			throw new NotSupportedException();
		}
	}
#if OLD_CIRCULAR_STREAM
	public class CircularStream : Stream
	{
		public CircularStream() :
			this(DefaultBuffCount) {
		}
		
		public CircularStream(int num) {
			key = new object();
			eof = false;
			emptySem = new Semaphore(num, num);
			fillSem = new Semaphore(0, num);
			buffs = new byte[num][];
			buffSizes = new int[num];
			for(int i = 0; i < buffs.Length; ++i) {
				buffs[i] = new byte[BuffSize];
				buffSizes[i] = 0;
			}
			
			writeBuff = 0;
			readBuff = 0;
			emptySemValue = num;
			fillSemValue = 0;
		}
		
		public const int DefaultBuffCount = 20;
		public const int BuffSize = 1024*4;
		
		private byte[][] buffs;
		private int[] buffSizes;
		private Semaphore emptySem; public int emptySemValue;
		private Semaphore fillSem; public int fillSemValue;
		private int writeBuff;
		private int readBuff;
		private int readIndex;
		
		
		private object key;
		private bool eof;
		private bool beforeDoneWriting;
		private bool doneWriting;
		
		// Public methods
		public void MarkEnd() {
			emptySem.WaitOne();
			emptySem.WaitOne();
			lock(key) {
				doneWriting = true;
				fillSem.Release();
			}
			NextWriteBuff(GetNext: false);
			emptySem.Release();
		}
		
		// Private methods
		private void NextWriteBuff(bool GetNext = true/*, bool BlockNext = false*/) {
			if(GetNext) {
				emptySem.WaitOne();
				lock(key) { --emptySemValue; } 
				fillSem.Release();
				lock(key) { ++fillSemValue; }
			}
			++writeBuff;
			if(writeBuff >= buffs.Length) {
				writeBuff = 0;
			}
			buffSizes[writeBuff] = 0;
		}
		
		private bool NextReadBuff(bool BlockNext = false, bool ReleaseOther = true) {
			bool ret = true;
			if(!BlockNext) {
				ret = fillSem.WaitOne(0);
			}
			if(ret) {
				if(BlockNext) {
					fillSem.WaitOne();
				}
				lock(key) { --fillSemValue; }
				++readBuff;
				if(readBuff >= buffs.Length) {
					readBuff = 0;
				}
				readIndex = 0;
				//if(ReleaseOther) {
					emptySem.Release();
					lock(key) { ++emptySemValue; }
				//}
			}
			return ret;
		}
		
		// Stream methods
		public override bool CanRead {
			get {
				return true;
			}
		}
		
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		public override bool CanWrite {
			get {
				return true;
			}
		}
		
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}
		
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		
		public override void Flush() {
			if(buffSizes[writeBuff] == 0) {
				return;
			}
			
			emptySem.WaitOne();
			NextWriteBuff();
			fillSem.Release();
		}
		
		public override int Read(byte[] buffer, int offset, int count) {
			//Console.WriteLine("emptySemValue = {0}, fillSemValue = {1}", emptySemValue, fillSemValue);
			//if(emptySemValue + fillSemValue != 20 && emptySemValue + fillSemValue != 19) throw new ApplicationException("Out of sync!!");
			if(eof) {
				
				return 0;
			}
			
			bool readDoneWriting;
			lock(key) {
				readDoneWriting = doneWriting;
			}
			
			int ret;
			fillSem.WaitOne();
			lock(key) { --fillSemValue; }
			
			if(readIndex >= buffSizes[readBuff]) {
				if(!NextReadBuff(BlockNext: !readDoneWriting, ReleaseOther: false)) {
					//Console.WriteLine("Final: emptySemValue = {0}, fillSemValue = {1}", emptySemValue, fillSemValue);
					eof = true; // Continue with function to output data.
				}
			}
			
			if(count < buffSizes[readBuff] - readIndex) {
				Buffer.BlockCopy(buffs[readBuff], readIndex, buffer, offset, count);
				readIndex += count;
				ret = count;
			}
			else {
				int total = 0;
				while(count > 0) {
					int next = buffSizes[readBuff] - readIndex;
					if(next > count) next = count;
					
					Buffer.BlockCopy(buffs[readBuff], readIndex, buffer, offset, next);
					readIndex += next;
					
					total += next;
					offset += next;
					count -= next;
					
					if(readIndex >= buffSizes[readBuff]) {
						if(!NextReadBuff(BlockNext: total == 0 && !readDoneWriting)) {
							if(total == 0) {
								//Console.WriteLine("Final: emptySemValue = {0}, fillSemValue = {1}", emptySemValue, fillSemValue);
								eof = true;
							}
							break;
						}
					}
				}
				
				ret = total;
			}
			fillSem.Release();
			lock(key) { ++fillSemValue; }
			return ret;
		}
		
		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}
		
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}
		
		public override void Write(byte[] buffer, int offset, int count) {
			//Console.WriteLine("emptySemValue = {0}, fillSemValue = {1}", emptySemValue, fillSemValue);
			//if(emptySemValue + fillSemValue != 20 && emptySemValue + fillSemValue != 19) throw new ApplicationException("Out of sync!!");
			//lock(key) {
				emptySem.WaitOne();
				lock(key) { --emptySemValue; }
				if(count < buffs[writeBuff].Length - buffSizes[writeBuff]) {
					Buffer.BlockCopy(buffer, offset, buffs[writeBuff], buffSizes[writeBuff], count);
					buffSizes[writeBuff] += count;
				}
				else {
					while(count > 0) {
						int next = buffs[writeBuff].Length - buffSizes[writeBuff];
						if(next > count) next = count;
						
						Buffer.BlockCopy(buffer, offset, buffs[writeBuff], buffSizes[writeBuff], next);
						buffSizes[writeBuff] += next;
						
						offset += next;
						count -= next;
						
						if(buffSizes[writeBuff] >= buffs[writeBuff].Length) {
							NextWriteBuff();
						}
					}
				}
				emptySem.Release();
				lock(key) { ++emptySemValue; }
			//}
		}
		
		
	}
#endif
}

