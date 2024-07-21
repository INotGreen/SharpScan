using System;
using System.Collections;
using System.Reflection;
using Tamir.SharpSsh.jsch;

/* 
 * SshBase.cs
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
    /// <summary>
    /// A wrapper class for JSch's SSH channel
    /// </summary>
    public abstract class SshBase : IDisposable
    {
        /// <summary>
        /// Default TCP port of SSH protocol
        /// </summary>
        private static int SSH_TCP_PORT = 22;

        protected Channel m_channel;

        protected string m_host;
        protected JSch m_jsch;
        protected string m_pass;
        protected Session m_session;
        protected string m_user;

        /// <summary>
        /// Constructs a new SSH instance
        /// </summary>
        /// <param name="sftpHost">The remote SSH host</param>
        /// <param name="user">The login username</param>
        /// <param name="password">The login password</param>
        public SshBase(string sftpHost, string user, string password)
        {
            m_host = sftpHost;
            m_user = user;
            Password = password;
            m_jsch = new JSch();
        }

        /// <summary>
        /// Constructs a new SSH instance
        /// </summary>
        /// <param name="sftpHost">The remote SSH host</param>
        /// <param name="user">The login username</param>
        public SshBase(string sftpHost, string user)
            : this(sftpHost, user, null)
        {
        }

        public SshBase(Session session)
            : this(session.host, session.username, session.password)
        {
            m_session = session;
        }

        protected abstract string ChannelType { get; }

        /// <summary>
        /// Return true if the SSH subsystem is connected
        /// </summary>
        public virtual bool Connected
        {
            get
            {
                if (m_session != null)
                    return m_session.isConnected();
                return false;
            }
        }

        /// <summary>
        /// Gets the Cipher algorithm name used in this SSH connection.
        /// </summary>
        public string Cipher
        {
            get
            {
                CheckConnected();
                return m_session.getCipher();
            }
        }

        /// <summary>
        /// Gets the MAC algorithm name used in this SSH connection.
        /// </summary>
        public string Mac
        {
            get
            {
                CheckConnected();
                return m_session.getMac();
            }
        }

        /// <summary>
        /// Gets the server SSH version string.
        /// </summary>
        public string ServerVersion
        {
            get
            {
                CheckConnected();
                return m_session.getServerVersion();
            }
        }

        /// <summary>
        /// Gets the client SSH version string.
        /// </summary>
        public string ClientVersion
        {
            get
            {
                CheckConnected();
                return m_session.getClientVersion();
            }
        }

        public string Host
        {
            get
            {
                CheckConnected();
                return m_session.getHost();
            }
        }

        public HostKey HostKey
        {
            get
            {
                CheckConnected();
                return m_session.getHostKey();
            }
        }

        public int Port
        {
            get
            {
                CheckConnected();
                return m_session.getPort();
            }
        }

        /// <summary>
        /// Gets the underlying Session
        /// </summary>
        public Session Session
        {
            get { return m_session; }
        }

        /// <summary>
        /// The password string of the SSH subsystem
        /// </summary>
        public string Password
        {
            get { return m_pass; }
            set { m_pass = value; }
        }

        public string Username
        {
            get { return m_user; }
        }

        public static Version Version
        {
            get
            {
                Assembly asm
                    = Assembly.GetAssembly(typeof (SshBase));
                return asm.GetName().Version;
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            Close();
        }

        #endregion

        /// <summary>
        /// Adds identity file for publickey user authentication
        /// </summary>
        /// <param name="privateKeyFile">The path to the private key file</param>
        public virtual void AddIdentityFile(string privateKeyFile)
        {
            m_jsch.addIdentity(privateKeyFile);
        }

        /// <summary>
        /// Adds identity file for publickey user authentication
        /// </summary>
        /// <param name="privateKeyFile">The path to the private key file</param>
        /// <param name="passphrase">A passphrase for decrypting the private key file</param>
        public virtual void AddIdentityFile(string privateKeyFile, string passphrase)
        {
            m_jsch.addIdentity(privateKeyFile, passphrase);
        }

        /// <summary>
        /// Connect to remote SSH server
        /// </summary>
        public virtual void Connect()
        {
            Connect(SSH_TCP_PORT);
        }

        /// <summary>
        /// Connect to remote SSH server
        /// </summary>
        /// <param name="tcpPort">The destination TCP port for this connection</param>
        public virtual void Connect(int tcpPort)
        {
            if (m_session == null || !m_session.isConnected())
                ConnectSession(tcpPort);

            ConnectChannel();
        }

        protected virtual void ConnectSession(int tcpPort)
        {
            m_session = m_jsch.getSession(m_user, m_host, tcpPort);
            if (Password != null)
                m_session.setUserInfo(new KeyboardInteractiveUserInfo(Password));
            var config = new Hashtable();
            config.Add("StrictHostKeyChecking", "no");
            m_session.setConfig(config);
            m_session.connect();
        }


        protected virtual void ConnectChannel()
        {
            m_channel = m_session.openChannel(ChannelType);
            OnChannelReceived();
            m_channel.connect();
            OnConnected();
        }

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnChannelReceived()
        {
        }

        public void SendIgnore()
        {
            if (Connected)
                m_session.sendIgnore();
        }

        /// <summary>
        /// Closes the SSH subsystem
        /// </summary>
        public virtual void Close()
        {
            Close(true);
        }

        /// <summary>
        /// Closes the SSH subsystem
        /// </summary>
        /// <param name="closeSession">True if the session should be closed</param>
        public virtual void Close(bool closeSession)
        {
            if (m_channel != null)
            {
                m_channel.disconnect();
                m_channel = null;
            }
            if (closeSession && m_session != null)
            {
                m_session.disconnect();
                m_session = null;
            }
        }

        private void CheckConnected()
        {
            if (!Connected)
            {
                throw new Exception("SSH session is not connected.");
            }
        }

        #region Nested type: KeyboardInteractiveUserInfo

        /// <summary>
        /// For password and KI auth modes
        /// </summary>
        protected class KeyboardInteractiveUserInfo : UserInfo, UIKeyboardInteractive
        {
            private readonly string _password;

            public KeyboardInteractiveUserInfo(string password)
            {
                _password = password;
            }

            #region UIKeyboardInteractive Members

            public string[] promptKeyboardInteractive(string destination, string name, string instruction,
                                                      string[] prompt, bool[] echo)
            {
                return new[] {_password};
            }

            #endregion

            #region UserInfo Members

            public bool promptYesNo(string message)
            {
                return true;
            }

            public bool promptPassword(string message)
            {
                return true;
            }

            public string getPassword()
            {
                return _password;
            }

            public bool promptPassphrase(string message)
            {
                return true;
            }

            public string getPassphrase()
            {
                return null;
            }

            public void showMessage(string message)
            {
            }

            #endregion
        }

        #endregion
    }
}