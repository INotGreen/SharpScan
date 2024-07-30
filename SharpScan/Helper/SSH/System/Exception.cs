using Ex = System.Exception;

namespace Tamir.SharpSsh.Sharp
{
    /// <summary>
    /// Summary description for Exception.
    /// </summary>
    public class Exception : Ex
    {
        public Exception()
        {
        }

        public Exception(string msg) : base(msg)
        {
        }

        public virtual string toString()
        {
            return ToString();
        }
    }
}