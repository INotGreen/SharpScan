/* Copyright (C) 2014-2024 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using Utilities;

namespace SMBLibrary
{
    /// <summary>
    /// [MS-FSCC] 2.4.37.2 - FileRenameInformation Type 2
    /// </summary>
    /// <remarks>
    /// [MS-FSA] 2.1.5.14.11
    /// FILE_RENAME_INFORMATION_TYPE_1: Used for 32-bit local clients and the SMB1 protocol.
    /// FILE_RENAME_INFORMATION_TYPE_2: Used for 64-bit local clients and the SMB2 protocol.
    /// </remarks>
    public class FileRenameInformationType2 : FileInformation
    {
        public const int FixedLength = 20;

        public bool ReplaceIfExists;
        // 7 reserved bytes
        public ulong RootDirectory;
        private uint FileNameLength;
        public string FileName = String.Empty;
        // Padding - the number of bytes required to make the size of this structure at least 24.

        public FileRenameInformationType2()
        {
        }

        public FileRenameInformationType2(byte[] buffer, int offset)
        {
            ReplaceIfExists = Conversion.ToBoolean(ByteReader.ReadByte(buffer, offset + 0));
            RootDirectory = LittleEndianConverter.ToUInt64(buffer, offset + 8);
            FileNameLength = LittleEndianConverter.ToUInt32(buffer, offset + 16);
            FileName = ByteReader.ReadUTF16String(buffer, offset + 20, (int)FileNameLength / 2);
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            FileNameLength = (uint)(FileName.Length * 2);
            ByteWriter.WriteByte(buffer, offset + 0, Convert.ToByte(ReplaceIfExists));
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, RootDirectory);
            LittleEndianWriter.WriteUInt32(buffer, offset + 16, FileNameLength);
            ByteWriter.WriteUTF16String(buffer, offset + 20, FileName);
        }

        public override FileInformationClass FileInformationClass
        {
            get
            {
                return FileInformationClass.FileRenameInformation;
            }
        }

        public override int Length
        {
            get
            {
                return Math.Max(FixedLength + FileName.Length * 2, 24);
            }
        }
    }
}
