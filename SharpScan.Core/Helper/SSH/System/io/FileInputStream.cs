using System.IO;

namespace Tamir.SharpSsh.Sharp.io
{
    /// <summary>
    /// Summary description for FileInputStream.
    /// </summary>
    public class FileInputStream : InputStream
    {
        private readonly FileStream fs;

        public FileInputStream(string file)
        {
            fs = global::System.IO.File.OpenRead(file);
        }

        public FileInputStream(File file) : this(file.info.Name)
        {
        }

        public override bool CanSeek
        {
            get { return fs.CanSeek; }
        }

        public override void Close()
        {
            fs.Close();
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            return fs.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return fs.Seek(offset, origin);
        }
    }
}