using System.Collections.Generic;
using System.IO;

namespace Blamite.Util
{
	public class ConcatenatedStream : Stream
	{
		readonly Queue<Stream> streams;

		public ConcatenatedStream(IEnumerable<Stream> streams)
		{
			this.streams = new Queue<Stream>(streams);
		}
		public override bool CanRead
		{
			get { return true; }
		}
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (streams.Count == 0)
				return 0;

			var bytesRead = streams.Peek().Read(buffer, offset, count);
			if (bytesRead == 0)
			{
				streams.Dequeue().Dispose();
				return Read(buffer, offset, count);
			}
			return bytesRead;
		}

		public override bool CanSeek
		{
			get { throw new System.NotImplementedException(); }
		}

		public override bool CanWrite
		{
			get { throw new System.NotImplementedException(); }
		}

		public override void Flush()
		{
			throw new System.NotImplementedException();
		}

		public override long Length
		{
			get { throw new System.NotImplementedException(); }
		}

		public override long Position
		{
			get
			{
				throw new System.NotImplementedException();
			}
			set
			{
				throw new System.NotImplementedException();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new System.NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new System.NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new System.NotImplementedException();
		}
	}
}
