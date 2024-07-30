using Tamir.SharpSsh.Sharp;
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