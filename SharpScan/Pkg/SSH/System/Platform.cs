using System;

namespace Tamir.SharpSsh.Sharp
{
    /// <summary>
    /// Summary description for Platform.
    /// </summary>
    public class Platform
    {
        public static bool Windows
        {
            get { return Environment.OSVersion.Platform.ToString().StartsWith("Win"); }
        }
    }
}