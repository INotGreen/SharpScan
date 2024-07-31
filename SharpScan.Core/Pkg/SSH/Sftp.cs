using System;
using System.Collections;
using System.IO;
using System.Timers;
using Tamir.SharpSsh.jsch;

/* 
 * Sftp.cs
 * 
 * Copyright (c) 2006 Tamir Gal, http://www.tamirgal.com, All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *  	1. Redistributions of source code must retain the above copyright notice,
 *		this list of conditions and the following disclaimer.
 *
 *	    2. Redistributions in binary form must reproduce the above copyright 
 *		notice, this list of conditions and the following disclaimer in 
 *		the documentation and/or other materials provided with the distribution.
 *
 *	    3. The names of the authors may not be used to endorse or promote products
 *		derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR
 *  *OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 **/

namespace Tamir.SharpSsh
{
    public class Sftp : SshTransferProtocolBase
    {
        private bool cancelled;
        private MyProgressMonitor m_monitor;

        public Sftp(string sftpHost, string user, string password)
            : base(sftpHost, user, password)
        {
            Init();
        }

        public Sftp(string sftpHost, string user)
            : base(sftpHost, user)
        {
            Init();
        }

        protected override string ChannelType
        {
            get { return "sftp"; }
        }

        private ChannelSftp SftpChannel
        {
            get { return (ChannelSftp) m_channel; }
        }

        private void Init()
        {
            m_monitor = new MyProgressMonitor(this);
        }

        public override void Cancel()
        {
            cancelled = true;
        }

        //Get

        public void Get(string fromFilePath)
        {
            Get(fromFilePath, ".");
        }

        public void Get(string[] fromFilePaths)
        {
            for (int i = 0; i < fromFilePaths.Length; i++)
            {
                Get(fromFilePaths[i]);
            }
        }

        public void Get(string[] fromFilePaths, string toDirPath)
        {
            for (int i = 0; i < fromFilePaths.Length; i++)
            {
                Get(fromFilePaths[i], toDirPath);
            }
        }

        public override void Get(string fromFilePath, string toFilePath)
        {
            cancelled = false;
            SftpChannel.get(fromFilePath, toFilePath, m_monitor, ChannelSftp.OVERWRITE);
        }

        //Put

        public void Put(string fromFilePath)
        {
            Put(fromFilePath, ".");
        }

        public void Put(string[] fromFilePaths)
        {
            for (int i = 0; i < fromFilePaths.Length; i++)
            {
                Put(fromFilePaths[i]);
            }
        }

        public void Put(string[] fromFilePaths, string toDirPath)
        {
            for (int i = 0; i < fromFilePaths.Length; i++)
            {
                Put(fromFilePaths[i], toDirPath);
            }
        }

        public override void Put(string fromFilePath, string toFilePath)
        {
            cancelled = false;
            SftpChannel.put(fromFilePath, toFilePath, m_monitor, ChannelSftp.OVERWRITE);
        }

        //MkDir

        public override void Mkdir(string directory)
        {
            SftpChannel.mkdir(directory);
        }

        //Ls

        public ArrayList GetFileList(string path)
        {
            var list = new ArrayList();
            foreach (ChannelSftp.LsEntry entry in SftpChannel.ls(path))
            {
                list.Add(entry.getFilename().ToString());
            }
            return list;
        }

        #region ProgressMonitor Implementation

        private class MyProgressMonitor : SftpProgressMonitor
        {
            private readonly Sftp m_sftp;
            private string dest;
            private int elapsed = -1;
            private string src;

            private System.Timers.Timer timer;
            private long total;
            private long transferred;

            public MyProgressMonitor(Sftp sftp)
            {
                m_sftp = sftp;
            }

            public override void init(int op, String src, String dest, long max)
            {
                this.src = src;
                this.dest = dest;
                elapsed = 0;
                total = max;
                timer = new System.Timers.Timer(1000);
                timer.Start();
                timer.Elapsed += timer_Elapsed;

                string note;
                if (op.Equals(GET))
                {
                    note = "Downloading " + Path.GetFileName(src) + "...";
                }
                else
                {
                    note = "Uploading " + Path.GetFileName(src) + "...";
                }
                m_sftp.SendStartMessage(src, dest, (int) total, note);
            }

            public override bool count(long c)
            {
                transferred += c;
                string note = ("Transfering... [Elapsed time: " + elapsed + "]");
                m_sftp.SendProgressMessage(src, dest, (int) transferred, (int) total, note);
                return !m_sftp.cancelled;
            }

            public override void end()
            {
                timer.Stop();
                timer.Dispose();
                string note = ("Done in " + elapsed + " seconds!");
                m_sftp.SendEndMessage(src, dest, (int) transferred, (int) total, note);
                transferred = 0;
                total = 0;
                elapsed = -1;
                src = null;
                dest = null;
            }

            private void timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                elapsed++;
            }
        }

        #endregion ProgressMonitor Implementation
    }
}