using System.Collections;

namespace Tamir.SharpSsh.Sharp.util
{
    /// <summary>
    /// Summary description for Enumeration.
    /// </summary>
    public class Enumeration
    {
        private readonly IEnumerator e;
        private bool hasMore;

        public Enumeration(IEnumerator e)
        {
            this.e = e;
            hasMore = e.MoveNext();
        }

        public bool hasMoreElements()
        {
            return hasMore;
        }

        public object nextElement()
        {
            object o = e.Current;
            hasMore = e.MoveNext();
            return o;
        }
    }
}