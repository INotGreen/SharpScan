using System.Runtime.InteropServices;

namespace SharpRDPCheck
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ABCDStruct
    {
        public uint A;
        public uint B;
        public uint C;
        public uint D;
    }
}