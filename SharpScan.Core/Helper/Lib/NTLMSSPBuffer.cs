﻿
namespace SharpScan
{
    public class NTLMSSPBuffer
    {
        #region row shellcode smb
        // SMB_COM_NEGOTIATE（0x72）- 版本协商，其实就是强制 SMB1 版本协议
        public static byte[] smb_buffer_v1_1 = {
            0x00, 0x00, 0x00, 0x85, 0xff, 0x53, 0x4d, 0x42,
            0x72, 0x00, 0x00, 0x00, 0x00, 0x18, 0x53, 0xc8,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xfe,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x62, 0x00, 0x02,
            0x50, 0x43, 0x20, 0x4e, 0x45, 0x54, 0x57, 0x4f,
            0x52, 0x4b, 0x20, 0x50, 0x52, 0x4f, 0x47, 0x52,
            0x41, 0x4d, 0x20, 0x31, 0x2e, 0x30, 0x00, 0x02,
            0x4c, 0x41, 0x4e, 0x4d, 0x41, 0x4e, 0x31, 0x2e,
            0x30, 0x00, 0x02, 0x57, 0x69, 0x6e, 0x64, 0x6f,
            0x77, 0x73, 0x20, 0x66, 0x6f, 0x72, 0x20, 0x57,
            0x6f, 0x72, 0x6b, 0x67, 0x72, 0x6f, 0x75, 0x70,
            0x73, 0x20, 0x33, 0x2e, 0x31, 0x61, 0x00, 0x02,
            0x4c, 0x4d, 0x31, 0x2e, 0x32, 0x58, 0x30, 0x30,
            0x32, 0x00, 0x02, 0x4c, 0x41, 0x4e, 0x4d, 0x41,
            0x4e, 0x31, 0x2e, 0x31, 0x00, 0x02, 0x4e, 0x54,
            0x20, 0x4c, 0x4d, 0x20, 0x30, 0x2e, 0x31, 0x32,
            0x00
        };

        // SMB_COM_SESSION_SETUP_ANDX（0x73） 嵌入 NTLMSSP
        public static byte[] smb_buffer_v1_2 = {
            0x00, 0x00, 0x00, 0xfc, 0xff, 0x53, 0x4d, 0x42,
            0x73, 0x00, 0x00, 0x00, 0x00, 0x18, 0x07, 0xc8,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xfe,
            0x00, 0x00, 0x40, 0x00, 0x0c, 0xff, 0x00, 0x0a,
            0x01, 0x04, 0x41, 0x32, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x4c, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xd4, 0x00, 0x00, 0xa0, 0xc1, 0x00, 0x60,
            0x48, 0x06, 0x06, 0x2b, 0x06, 0x01, 0x05, 0x05,
            0x02, 0xa0, 0x3e, 0x30, 0x3c, 0xa0, 0x0e, 0x30,
            0x0c, 0x06, 0x0a, 0x2b, 0x06, 0x01, 0x04, 0x01,
            0x82, 0x37, 0x02, 0x02, 0x0a, 0xa2, 0x2a, 0x04,
            0x28, 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x82, 0x08,
            0xa2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x05, 0x02, 0xce, 0x0e, 0x00, 0x00, 0x00,
            0x0f, 0x00, 0x00, 0x00, 0x57, 0x00, 0x69, 0x00,
            0x6e, 0x00, 0x64, 0x00, 0x6f, 0x00, 0x77, 0x00,
            0x73, 0x00, 0x20, 0x00, 0x53, 0x00, 0x65, 0x00,
            0x72, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00,
            0x20, 0x00, 0x32, 0x00, 0x30, 0x00, 0x30, 0x00,
            0x33, 0x00, 0x20, 0x00, 0x33, 0x00, 0x37, 0x00,
            0x39, 0x00, 0x30, 0x00, 0x20, 0x00, 0x53, 0x00,
            0x65, 0x00, 0x72, 0x00, 0x76, 0x00, 0x69, 0x00,
            0x63, 0x00, 0x65, 0x00, 0x20, 0x00, 0x50, 0x00,
            0x61, 0x00, 0x63, 0x00, 0x6b, 0x00, 0x20, 0x00,
            0x32, 0x00, 0x00, 0x00, 0x57, 0x00, 0x69, 0x00,
            0x6e, 0x00, 0x64, 0x00, 0x6f, 0x00, 0x77, 0x00,
            0x73, 0x00, 0x20, 0x00, 0x32, 0x00, 0x30, 0x00,
            0x30, 0x00, 0x33, 0x00, 0x20, 0x00, 0x35, 0x00,
            0x2e, 0x00, 0x32, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static byte[] smb_buffer_v2_1 = {
            0x00, 0x00, 0x00, 0x45, 0xff, 0x53, 0x4d, 0x42,
            0x72, 0x00, 0x00, 0x00, 0x00, 0x18, 0x01, 0xe8,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x8b,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x22, 0x00, 0x02,
            0x4e, 0x54, 0x20, 0x4c, 0x4d, 0x20, 0x30, 0x2e,
            0x31, 0x32, 0x00, 0x02, 0x53, 0x4d, 0x42, 0x20,
            0x32, 0x2e, 0x30, 0x30, 0x32, 0x00, 0x02, 0x53,
            0x4d, 0x42, 0x20, 0x32, 0x2e, 0x3f, 0x3f, 0x3f,
            0x00
            };

        public static byte[] smb_buffer_v2_2 = { /* Packet 164 */
            0x00, 0x00, 0x00, 0xc8, 0xfe, 0x53, 0x4d, 0x42,
            0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xff, 0xfe, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x05, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00,
            0x9f, 0x03, 0xa2, 0x15, 0xa4, 0x34, 0x4e, 0x64,
            0x2c, 0xa7, 0x95, 0xc1, 0x7c, 0xe2, 0x5f, 0x76,
            0x70, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
            0x02, 0x02, 0x10, 0x02, 0x00, 0x03, 0x02, 0x03,
            0x11, 0x03, 0x00, 0x00, 0x01, 0x00, 0x26, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x20, 0x00,
            0x01, 0x00, 0x50, 0xfd, 0x0a, 0xf0, 0xfe, 0x7e,
            0x71, 0x6a, 0xea, 0xcb, 0x66, 0x1f, 0x49, 0x28,
            0x61, 0xea, 0x98, 0x6b, 0xd9, 0x5e, 0x93, 0xd7,
            0x6f, 0x8e, 0xd0, 0x59, 0x0a, 0x6a, 0x89, 0xfe,
            0x59, 0xde, 0x00, 0x00, 0x02, 0x00, 0x06, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x10, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00,
            0x03, 0x00, 0x04, 0x00
        };

        public static byte[] smb_buffer_v2_3 = {
            0x00, 0x00, 0x00, 0xa6, 0xfe, 0x53, 0x4d, 0x42,
            0x40, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0xff, 0xfe, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x01,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x58, 0x00, 0x4e, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x60, 0x4c, 0x06, 0x06,
            0x2b, 0x06, 0x01, 0x05, 0x05, 0x02, 0xa0, 0x42,
            0x30, 0x40, 0xa0, 0x0e, 0x30, 0x0c, 0x06, 0x0a,
            0x2b, 0x06, 0x01, 0x04, 0x01, 0x82, 0x37, 0x02,
            0x02, 0x0a, 0xa2, 0x2e, 0x04, 0x2c, 0x4e, 0x54,
            0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x35, 0x82, 0x88, 0xe2, 0x01, 0x00,
            0x01, 0x00, 0x20, 0x00, 0x00, 0x00, 0x0b, 0x00,
            0x0b, 0x00, 0x21, 0x00, 0x00, 0x00, 0x2e, 0x57,
            0x4f, 0x52, 0x4b, 0x53, 0x54, 0x41, 0x54, 0x49,
            0x4f, 0x4e
        };

        #endregion

        #region row shellcode dcerpc

        // NTLMSSP 
        public static byte[] dcerpc_buffer_v1 = {
            0x05, 0x00, 0x0b, 0x03, 0x10, 0x00, 0x00, 0x00,
            0x78, 0x00, 0x28, 0x00, 0x03, 0x00, 0x00, 0x00,
            0xb8, 0x10, 0xb8, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
            0xa0, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46,
            0x00, 0x00, 0x00, 0x00, 0x04, 0x5d, 0x88, 0x8a,
            0xeb, 0x1c, 0xc9, 0x11, 0x9f, 0xe8, 0x08, 0x00,
            0x2b, 0x10, 0x48, 0x60, 0x02, 0x00, 0x00, 0x00,
            0x0a, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x07, 0x82, 0x08, 0xa2,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x06, 0x01, 0xb1, 0x1d, 0x00, 0x00, 0x00, 0x0f
        };

        // ('71710533-BEBA-4937-8319-B5DBEF9CCC36', '1.0') -> 包含 syntaxes_not_supported 则是32位
        public static byte[] dcerpc_buffer_v2 = {
            0x05, 0x00, 0x0b, 0x03, 0x10, 0x00, 0x00, 0x00,
            0x48, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0xb8, 0x10, 0xb8, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x08, 0x83, 0xaf, 0xe1, 0x1f, 0x5d, 0xc9, 0x11,
            0x91, 0xa4, 0x08, 0x00, 0x2b, 0x14, 0xa0, 0xfa,
            0x03, 0x00, 0x00, 0x00, 0x33, 0x05, 0x71, 0x71,
            0xba, 0xbe, 0x37, 0x49, 0x83, 0x19, 0xb5, 0xdb,
            0xef, 0x9c, 0xcc, 0x36, 0x01, 0x00, 0x00, 0x00
        };


        #endregion
    }
}
