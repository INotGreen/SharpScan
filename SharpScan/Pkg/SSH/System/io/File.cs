using System.IO;

namespace Tamir.SharpSsh.Sharp.io
{
    /// <summary>
    /// Summary description for File.
    /// </summary>
    public class File
    {
        private readonly string file;
        internal FileInfo info;

        public File(string file)
        {
            this.file = file;
            info = new FileInfo(file);
        }

        public static string separator
        {
            get { return Path.DirectorySeparatorChar.ToString(); }
        }

        public static char separatorChar
        {
            get { return Path.DirectorySeparatorChar; }
        }

        public string getCanonicalPath()
        {
            return Path.GetFullPath(file);
        }

        public bool isDirectory()
        {
            return Directory.Exists(file);
        }

        public long Length()
        {
            return info.Length;
        }

        public long length()
        {
            return Length();
        }

        public bool isAbsolute()
        {
            return Path.IsPathRooted(file);
        }

        public String[] list()
        {
            string[] dirs = Directory.GetDirectories(file);
            string[] files = Directory.GetFiles(file);
            var _list = new String[dirs.Length + files.Length];
            System.arraycopy(dirs, 0, _list, 0, dirs.Length);
            System.arraycopy(files, 0, _list, dirs.Length, files.Length);
            return _list;
        }
    }
}