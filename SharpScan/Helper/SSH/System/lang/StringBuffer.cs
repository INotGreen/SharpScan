using System.Text;

namespace Tamir.SharpSsh.Sharp.lang
{
    /// <summary>
    /// Summary description for StringBuffer.
    /// </summary>
    public class StringBuffer
    {
        private readonly StringBuilder sb;

        public StringBuffer()
        {
            sb = new StringBuilder();
        }

        public StringBuffer(string s)
        {
            sb = new StringBuilder(s);
        }

        public StringBuffer(StringBuilder sb) : this(sb.ToString())
        {
        }

        public StringBuffer(String s) : this(s.ToString())
        {
        }

        public StringBuffer append(string s)
        {
            sb.Append(s);
            return this;
        }

        public StringBuffer append(char s)
        {
            sb.Append(s);
            return this;
        }

        public StringBuffer append(String s)
        {
            return append(s.ToString());
        }

        public StringBuffer delete(int start, int end)
        {
            sb.Remove(start, end - start);
            return this;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public string toString()
        {
            return ToString();
        }
    }
}