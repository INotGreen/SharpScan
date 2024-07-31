using System;
using System.IO;
using Tamir.SharpSsh.Sharp.io;

namespace Tamir.SharpSsh.jsch
{
    internal class OutputStreamPut : OutputStream
    {
        private readonly byte[] _data = new byte[1];
        private readonly long[] _offset;
        private readonly int[] ackid = new int[1];
        private readonly byte[] handle;
        private readonly ChannelSftp.Header header = new ChannelSftp.Header();
        private readonly SftpProgressMonitor monitor;
        private readonly ChannelSftp sftp;
        private int _ackid;
        private int ackcount;
        private bool init = true;
        private int startid;

        internal OutputStreamPut
            (ChannelSftp sftp,
             byte[] handle,
             long[] _offset,
             SftpProgressMonitor monitor)
        {
            this.sftp = sftp;
            this.handle = handle;
            this._offset = _offset;
            this.monitor = monitor;
        }

        public override void Write(byte[] d, int s, int len)
        {
            if (init)
            {
                startid = sftp.seq;
                _ackid = sftp.seq;
                init = false;
            }

            try
            {
                int _len = len;
                while (_len > 0)
                {
                    int sent = sftp.sendWRITE(handle, _offset[0], d, s, _len);
                    _offset[0] += sent;
                    s += sent;
                    _len -= sent;
                    if ((sftp.seq - 1) == startid ||
                        sftp.io.ins.available() >= 1024)
                    {
                        while (sftp.io.ins.available() > 0)
                        {
                            if (sftp.checkStatus(ackid, header))
                            {
                                _ackid = ackid[0];
                                if (startid > _ackid || _ackid > sftp.seq - 1)
                                {
                                    throw new SftpException(ChannelSftp.SSH_FX_FAILURE, "");
                                }
                                ackcount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if (monitor != null && !monitor.count(len))
                {
                    close();
                    throw new IOException("canceled");
                }
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOException(e.ToString());
            }
        }

        public void write(int foo)
        {
            _data[0] = (byte) foo;
            Write(_data, 0, 1);
        }

        public override void Close()
        {
            if (!init)
            {
                try
                {
                    int _ackcount = sftp.seq - startid;
                    while (_ackcount > ackcount)
                    {
                        if (!sftp.checkStatus(null, header))
                        {
                            break;
                        }
                        ackcount++;
                    }
                }
                catch (SftpException e)
                {
                    throw new IOException(e.toString());
                }
            }

            if (monitor != null) monitor.end();
            try
            {
                sftp._sendCLOSE(handle, header);
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new IOException(e.ToString());
            }
        }
    }
}