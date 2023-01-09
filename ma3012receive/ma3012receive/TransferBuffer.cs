using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ma3012receive
{
    public class TransferBuffer
    {
        // Fields
        private long highWaterMark;
        private long lowWaterMark;
        private MemoryStream memoryStream = new MemoryStream();
        private long position;
        private long reorganizeIfUnusedBytesGreaterThan = 1048576L;
        

        // Methods
        public byte[] Peek(long index, int length)
        {
            lock (this)
            {
                this.memoryStream.Seek(this.lowWaterMark + index, SeekOrigin.Begin);
                byte[] buffer = new byte[length];
                this.memoryStream.Read(buffer, 0, length);
                return buffer;
            }
        }

        public byte[] Pop(long length)
        {
            lock (this)
            {
                this.position += length;
                byte[] buffer = new byte[length];
                this.memoryStream.Seek(this.lowWaterMark, SeekOrigin.Begin);
                this.memoryStream.Read(buffer, 0, (int)length);
                this.lowWaterMark += buffer.Length;
                return buffer;
            }
        }

        public void Push(byte[] bytes)
        {
            lock (this)
            {
                this.memoryStream.Seek(0L, SeekOrigin.End);
                this.memoryStream.Write(bytes, 0, bytes.Length);
                if (this.lowWaterMark > this.reorganizeIfUnusedBytesGreaterThan)
                {
                    MemoryStream stream = new MemoryStream();
                    byte[] buffer = new byte[this.Length];
                    this.memoryStream.Seek(this.lowWaterMark, SeekOrigin.Begin);
                    this.memoryStream.Read(buffer, 0, (int)this.Length);
                    stream.Write(buffer, 0, buffer.Length);
                    this.lowWaterMark = 0L;
                    this.memoryStream = stream;
                }
            }
        }
        public void Pushs(byte[] bytes)
        {
            lock (this)
            {
                this.memoryStream.Seek(0L, SeekOrigin.End);
                this.memoryStream.Write(bytes, 0, bytes.Length);
                MemoryStream stream = new MemoryStream();
                byte[] buffer = new byte[this.Length];
                this.memoryStream.Seek(this.lowWaterMark, SeekOrigin.Begin);
                this.memoryStream.Read(buffer, 0, (int)this.Length);
                stream.Write(buffer, 0, buffer.Length);
                this.lowWaterMark = 0L;
                this.memoryStream = stream;
                
            }
        }
        public void Reset()
        {
            lock (this)
            {
                this.position = 0L;
                this.highWaterMark = 0L;
                this.lowWaterMark = 0L;
                this.memoryStream.SetLength(0L);
            }
        }

        // Properties
        public long HighWaterMark
        {
            get
            {
                lock (this)
                {
                    return this.highWaterMark;
                }
            }
            set
            {
                lock (this)
                {
                    this.highWaterMark = value;
                }
            }
        }

        public long Length
        {
            get
            {
                lock (this)
                {
                    return (this.memoryStream.Length - this.lowWaterMark);

                }
            }
        }

        public long Position
        {
            get
            {
                lock (this)
                {
                    return this.position;
                }
            }
            set
            {
                lock (this)
                {
                    this.position = value;
                }
            }
        }
    }
}
