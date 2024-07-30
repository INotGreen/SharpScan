using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Tamir.SharpSsh.Sharp.io;

namespace Tamir.Streams
{
    /*
	 * @(#)PipedOutputStream.Sharp	1.26 03/12/19
	 *
	 * Copyright 2004 Sun Microsystems, Inc. All rights reserved.
	 * SUN PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
	 */

    /**
	 * A piped output stream can be connected to a piped input stream 
	 * to create a communications pipe. The piped output stream is the 
	 * sending end of the pipe. Typically, data is written to a 
	 * <code>PipedOutputStream</code> object by one thread and data is 
	 * read from the connected <code>PipedInputStream</code> by some 
	 * other thread. Attempting to use both objects from a single thread 
	 * is not recommended as it may deadlock the thread.
	 *
	 * @author  James Gosling
	 * @version 1.26, 12/19/03
	 * @see     Sharp.io.PipedInputStream
	 * @since   JDK1.0
	 */

    public class PipedOutputStream : OutputStream
    {
        /* REMIND: identification of the read and write sides needs to be
			more sophisticated.  Either using thread groups (but what about
			pipes within a thread?) or using finalization (but it may be a
			long time until the next GC). */
        private PipedInputStream sink;

        /**
		* Creates a piped output stream connected to the specified piped 
		* input stream. Data bytes written to this stream will then be 
		* available as input from <code>snk</code>.
		*
		* @param      snk   The piped input stream to connect to.
		* @exception  IOException  if an I/O error occurs.
		*/

        public PipedOutputStream(PipedInputStream snk)
        {
            connect(snk);
        }

        /**
		* Creates a piped output stream that is not yet connected to a 
		* piped input stream. It must be connected to a piped input stream, 
		* either by the receiver or the sender, before being used. 
		*
		* @see     Sharp.io.PipedInputStream#connect(Sharp.io.PipedOutputStream)
		* @see     Sharp.io.PipedOutputStream#connect(Sharp.io.PipedInputStream)
		*/

        public PipedOutputStream()
        {
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Length
        {
            get { return sink.Length; }
        }

        public override long Position
        {
            get { return sink.m_in; }
            set { throw new IOException("Setting the position of this stream is not supported"); }
        }

        /**
		 * Connects this piped output stream to a receiver. If this object
		 * is already connected to some other piped input stream, an 
		 * <code>IOException</code> is thrown.
		 * <p>
		 * If <code>snk</code> is an unconnected piped input stream and 
		 * <code>src</code> is an unconnected piped output stream, they may 
		 * be connected by either the call:
		 * <blockquote><pre>
		 * src.connect(snk)</pre></blockquote>
		 * or the call:
		 * <blockquote><pre>
		 * snk.connect(src)</pre></blockquote>
		 * The two calls have the same effect.
		 *
		 * @param      snk   the piped input stream to connect to.
		 * @exception  IOException  if an I/O error occurs.
		 */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void connect(PipedInputStream snk)
        {
            if (snk == null)
            {
                throw new NullReferenceException();
            }
            else if (sink != null || snk.connected)
            {
                throw new IOException("Already connected");
            }
            sink = snk;
            snk.m_in = -1;
            snk.m_out = 0;
            snk.connected = true;
        }

        /**
		 * Writes the specified <code>byte</code> to the piped output stream. 
		 * If a thread was reading data bytes from the connected piped input 
		 * stream, but the thread is no longer alive, then an 
		 * <code>IOException</code> is thrown.
		 * <p>
		 * Implements the <code>write</code> method of <code>OutputStream</code>.
		 *
		 * @param      b   the <code>byte</code> to be written.
		 * @exception  IOException  if an I/O error occurs.
		 */

        public virtual void write(int b)
        {
            if (sink == null)
            {
                throw new IOException("Pipe not connected");
            }
            sink.receive(b);
        }

        /**
		 * Writes <code>len</code> bytes from the specified byte array 
		 * starting at offset <code>off</code> to this piped output stream. 
		 * If a thread was reading data bytes from the connected piped input 
		 * stream, but the thread is no longer alive, then an 
		 * <code>IOException</code> is thrown.
		 *
		 * @param      b     the data.
		 * @param      off   the start offset in the data.
		 * @param      len   the number of bytes to write.
		 * @exception  IOException  if an I/O error occurs.
		 */

        public override void write(byte[] b, int off, int len)
        {
            if (sink == null)
            {
                throw new IOException("Pipe not connected");
            }
            else if (b == null)
            {
                throw new NullReferenceException();
            }
            else if ((off < 0) || (off > b.Length) || (len < 0) ||
                     ((off + len) > b.Length) || ((off + len) < 0))
            {
                throw new IndexOutOfRangeException();
            }
            else if (len == 0)
            {
                return;
            }
            sink.receive(b, off, len);
        }

        public virtual void write(byte[] b)
        {
            write(b, 0, b.Length);
        }

        /**
		 * Flushes this output stream and forces any buffered output bytes 
		 * to be written out. 
		 * This will notify any readers that bytes are waiting in the pipe.
		 *
		 * @exception IOException if an I/O error occurs.
		 */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void flush()
        {
            if (sink != null)
            {
                lock (sink)
                {
                    //sink.notifyAll();
                    Monitor.PulseAll(sink);
                }
            }
        }

        /**
		 * Closes this piped output stream and releases any system resources 
		 * associated with this stream. This stream may no longer be used for 
		 * writing bytes.
		 *
		 * @exception  IOException  if an I/O error occurs.
		 */

        public override void close()
        {
            if (sink != null)
            {
                sink.receivedLast();
            }
        }

        ///////////////////////////////////////


        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override int ReadByte()
        {
            return 0;
        }

        public override void WriteByte(byte value)
        {
            write(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            write(buffer, offset, count);
        }

        public virtual void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public override void Close()
        {
            base.Close();
            close();
        }

        public override void Flush()
        {
            flush();
        }

        public override void SetLength(long value)
        {
            throw new IOException("Setting the length of this stream is not supported");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }
    }
}