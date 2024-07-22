using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpScan.Helper.SMB
{
    public class SmbClientFile
    {
        public SmbClientFile(string name, bool isDirectory, DateTime? lastModifiedTime, DateTime? creationTime)
        {
            Name = name;
            IsDirectory = isDirectory;
            if (lastModifiedTime != null)
            {
                if (lastModifiedTime.Value.Kind == DateTimeKind.Utc)
                {
                    lastModifiedTime = lastModifiedTime.Value.ToLocalTime();
                }
            }
            LastModifiedTime = lastModifiedTime;

            if (creationTime != null)
            {
                if (creationTime.Value.Kind == DateTimeKind.Utc)
                {
                    creationTime = creationTime.Value.ToLocalTime();
                }
            }
            CreationTime = creationTime;
        }

        public string Name { get; }
        public bool IsDirectory { get; }
        public DateTime? LastModifiedTime { get; }
        public DateTime? CreationTime { get; }
    }
}
