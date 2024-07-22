/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;
using SMBLibrary.RPC;

namespace SMBLibrary.Services
{
    /// <summary>
    /// NetrWkstaGetInfo Request (opnum 0)
    /// </summary>
    public class NetrWkstaGetInfoRequest
    {
        public string ServerName;
        public uint Level;

        public NetrWkstaGetInfoRequest()
        {

        }

        public NetrWkstaGetInfoRequest(byte[] buffer)
        {
            NDRParser parser = new NDRParser(buffer);
            ServerName = parser.ReadTopLevelUnicodeStringPointer();
            Level = parser.ReadUInt32();
        }

        public byte[] GetBytes()
        {
            NDRWriter writer = new NDRWriter();
            writer.WriteTopLevelUnicodeStringPointer(ServerName);
            writer.WriteUInt32(Level);

            return writer.GetBytes();
        }
    }
}
