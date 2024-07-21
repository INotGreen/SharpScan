using System;
using System.Security.Cryptography;

namespace Org.Mentalis.Security.Cryptography
{
    public sealed class HMAC : KeyedHashAlgorithm
    {
        // Fields
        private readonly HashAlgorithm m_HashAlgorithm;
        private readonly byte[] m_KeyBuffer;
        private readonly byte[] m_Padded;
        private bool m_IsDisposed;
        private bool m_IsHashing;

        // Methods
        public HMAC(HashAlgorithm hash)
            : this(hash, null)
        {
        }

        public HMAC(HashAlgorithm hash, byte[] rgbKey)
        {
            if (hash == null)
            {
                throw new ArgumentNullException();
            }
            if (rgbKey == null)
            {
                rgbKey = new byte[hash.HashSize/8];
                new RNGCryptoServiceProvider().GetBytes(rgbKey);
            }
            m_HashAlgorithm = hash;
            Key = (byte[]) rgbKey.Clone();
            m_IsDisposed = false;
            m_KeyBuffer = new byte[0x40];
            m_Padded = new byte[0x40];
            Initialize();
        }

        public override int HashSize
        {
            get { return m_HashAlgorithm.HashSize; }
        }

        protected override void Dispose(bool disposing)
        {
            m_IsDisposed = true;
            base.Dispose(true);
            m_HashAlgorithm.Clear();
            try
            {
                GC.SuppressFinalize(this);
            }
            catch
            {
            }
        }

        ~HMAC()
        {
            m_HashAlgorithm.Clear();
        }

        protected override void HashCore(byte[] rgb, int ib, int cb)
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (!m_IsHashing)
            {
                byte[] key;
                if (Key.Length > 0x40)
                {
                    key = m_HashAlgorithm.ComputeHash(Key);
                }
                else
                {
                    key = Key;
                }
                Array.Copy(key, 0, m_KeyBuffer, 0, key.Length);
                for (int i = 0; i < 0x40; i++)
                {
                    m_Padded[i] = (byte) (m_KeyBuffer[i] ^ 0x36);
                }
                m_HashAlgorithm.TransformBlock(m_Padded, 0, m_Padded.Length, m_Padded, 0);
                m_IsHashing = true;
            }
            m_HashAlgorithm.TransformBlock(rgb, ib, cb, rgb, ib);
        }

        protected override byte[] HashFinal()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            m_HashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hash = m_HashAlgorithm.Hash;
            for (int i = 0; i < 0x40; i++)
            {
                m_Padded[i] = (byte) (m_KeyBuffer[i] ^ 0x5c);
            }
            m_HashAlgorithm.Initialize();
            m_HashAlgorithm.TransformBlock(m_Padded, 0, m_Padded.Length, m_Padded, 0);
            m_HashAlgorithm.TransformFinalBlock(hash, 0, hash.Length);
            hash = m_HashAlgorithm.Hash;
            Array.Clear(m_KeyBuffer, 0, m_KeyBuffer.Length);
            m_IsHashing = false;
            return hash;
        }

        public override void Initialize()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            m_HashAlgorithm.Initialize();
            m_IsHashing = false;
            base.State = 0;
            Array.Clear(m_KeyBuffer, 0, m_KeyBuffer.Length);
        }

        // Properties
    }
}