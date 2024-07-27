using System.Text;
using Str = System.String;

namespace Tamir.SharpSsh.java
{
    /// <summary>
    /// Summary description for String.
    /// </summary>
    public class String
    {
        private readonly string s;

        public String(string s)
        {
            this.s = s;
        }

        public String(object o) : this(o.ToString())
        {
        }

        public String(byte[] arr) : this(getString(arr))
        {
        }

        public String(byte[] arr, int offset, int len) : this(getString(arr, offset, len))
        {
        }

        public static implicit operator String(string s1)
        {
            if (s1 == null) return null;
            return new String(s1);
        }

        public static implicit operator Str(String s1)
        {
            if (s1 == null) return null;
            return s1.ToString();
        }

        public static String operator +(String s1, String s2)
        {
            return new String(s1.ToString() + s2.ToString());
        }

        public byte[] getBytes()
        {
            return getBytes(this);
        }

        public override string ToString()
        {
            return s;
        }

        public String toLowerCase()
        {
            return ToString().ToLower();
        }

        public bool startsWith(string prefix)
        {
            return ToString().StartsWith(prefix);
        }

        public int indexOf(string sub)
        {
            return ToString().IndexOf(sub);
        }

        public int indexOf(char sub)
        {
            return ToString().IndexOf(sub);
        }

        public int indexOf(char sub, int i)
        {
            return ToString().IndexOf(sub, i);
        }

        public char charAt(int i)
        {
            return s[i];
        }

        public String substring(int start, int end)
        {
            int len = end - start;
            return ToString().Substring(start, len);
        }

        public String subString(int start, int len)
        {
            return substring(start, len);
        }

        public String substring(int len)
        {
            return ToString().Substring(len);
        }

        public String subString(int len)
        {
            return substring(len);
        }

        public int Length()
        {
            return ToString().Length;
        }

        public int length()
        {
            return Length();
        }

        public bool endsWith(string str)
        {
            return s.EndsWith(str);
        }

        public int lastIndexOf(string str)
        {
            return s.LastIndexOf(str);
        }

        public int lastIndexOf(char c)
        {
            return s.LastIndexOf(c);
        }

        public bool equals(object o)
        {
            return ToString().Equals(o.ToString());
        }

        public override bool Equals(object obj)
        {
            return @equals(obj);
        }

        public override int GetHashCode()
        {
            return s.GetHashCode();
        }

        public static string getString(byte[] arr)
        {
            return getString(arr, 0, arr.Length);
        }

        public static string getString(byte[] arr, int offset, int len)
        {
            return Encoding.Default.GetString(arr, offset, len);
        }

        public static string getStringUTF8(byte[] arr)
        {
            return getStringUTF8(arr, 0, arr.Length);
        }

        public static string getStringUTF8(byte[] arr, int offset, int len)
        {
            return Encoding.UTF8.GetString(arr, offset, len);
        }

        public static byte[] getBytes(string str)
        {
            return getBytesUTF8(str);
        }

        public static byte[] getBytesUTF8(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}