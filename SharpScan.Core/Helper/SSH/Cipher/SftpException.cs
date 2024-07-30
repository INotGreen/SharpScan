using Tamir.SharpSsh.Sharp;
using Exception = Tamir.SharpSsh.Sharp.Exception;
using String = System.String;

namespace Tamir.SharpSsh.jsch
{
    public class SftpException : Exception
    {
        public int id;
        public String message;

        public SftpException(int id, String message)
        {
            this.id = id;
            this.message = message;
        }

        public override String toString()
        {
            return message;
        }
    }
}