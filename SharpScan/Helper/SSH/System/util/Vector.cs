using System.Collections;

namespace Tamir.SharpSsh.Sharp.util
{
    /// <summary>
    /// Summary description for Vector.
    /// </summary>
    public class Vector : ArrayList
    {
        public int size()
        {
            return Count;
        }

        public void addElement(object o)
        {
            Add(o);
        }

        public void add(object o)
        {
            addElement(o);
        }

        public void removeElement(object o)
        {
            Remove(o);
        }

        public bool remove(object o)
        {
            Remove(o);
            return true;
        }

        public object elementAt(int i)
        {
            return this[i];
        }

        public object get(int i)
        {
            return elementAt(i);
            ;
        }

        public void clear()
        {
            Clear();
        }

        public string toString()
        {
            return ToString();
        }
    }
}