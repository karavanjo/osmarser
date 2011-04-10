using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Osm;
using ProtoBuf;
using OSMPBF;
using zlib;

namespace Osm
{
    public class LowLevelReader
    {
        // 4-byte number
        public static int IntLittleEndianToBigEndian(uint i)
        {
            return (int)(((i & 0xff) << 24) + ((i & 0xff00) << 8) + ((i & 0xff0000) >> 8) + ((i >> 24) & 0xff));
        }
    }

    abstract class InputStream : Stream
    {
        protected abstract int ReadNextBlock(byte[] buffer, int offset, int count);
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead, totalRead = 0;
            while (count > 0 && (bytesRead = ReadNextBlock(buffer, offset, count)) > 0)
            {
                count -= bytesRead;
                offset += bytesRead;
                totalRead += bytesRead;
                pos += bytesRead;
            }
            return totalRead;
        }
        long pos;
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override long Position
        {
            get
            {
                return pos;
            }
            set
            {
                if (pos != value) throw new NotImplementedException();
            }
        }
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }
        public override void Flush()
        {
            throw new NotImplementedException();
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return false; }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
    }
    class ZLibStream : InputStream
    {   // uses ZLIB.NET: http://www.componentace.com/download/download.php?editionid=25
        private ZInputStream reader; // seriously, why isn't this a stream?
        public ZLibStream(Stream stream)
        {
            reader = new ZInputStream(stream);
        }
        public override void Close()
        {
            reader.Close();
            base.Close();
        }
        protected override int ReadNextBlock(byte[] buffer, int offset, int count)
        {
            // OMG! reader.Read is the base-stream, reader.read is decompressed! yeuch
            return reader.read(buffer, offset, count);
        }

    }
    class LimitedStream : InputStream
    {
        private Stream stream;
        private long remaining;
        public LimitedStream(Stream stream, long length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length");
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanRead) throw new ArgumentException("stream");
            this.stream = stream;
            this.remaining = length;
        }
        protected override int ReadNextBlock(byte[] buffer, int offset, int count)
        {
            if (count > remaining) count = (int)remaining;
            int bytesRead = stream.Read(buffer, offset, count);
            if (bytesRead > 0) remaining -= bytesRead;
            return bytesRead;
        }
    }
}

