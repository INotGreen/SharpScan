namespace Tamir.SharpSsh.java.util
{
    /// <summary>
    /// Summary description for Hashtable.
    /// </summary>
    public class Hashtable
    {
        internal global::System.Collections.Hashtable h;

        public Hashtable()
        {
            h = new global::System.Collections.Hashtable();
        }

        public Hashtable(global::System.Collections.Hashtable h)
        {
            this.h = h;
        }

        public object this[object key]
        {
            get { return get(key); }
            set { h[key] = value; }
        }

        public void put(object key, object item)
        {
            h.Add(key, item);
        }

        public object get(object key)
        {
            return h[key];
        }

        public Enumeration keys()
        {
            return new Enumeration(h.Keys.GetEnumerator());
        }
    }
}