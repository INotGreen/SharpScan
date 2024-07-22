/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using SMBLibrary.RPC;

namespace SMBLibrary.Services
{
    public class NetrWkstaGetInfoResponse
    {
        public WorkstationInfo WkstaInfo;
        public Win32Error Result;

        public NetrWkstaGetInfoResponse()
        {
        }

        public NetrWkstaGetInfoResponse(byte[] buffer)
        {
            NDRParser parser = new NDRParser(buffer);
            WkstaInfo = new WorkstationInfo(parser);
            // 14.4 - If an operation returns a result, the representation of the result appears after all parameters in
            Result = (Win32Error)parser.ReadUInt32();
        }

        public byte[] GetBytes()
        {
            NDRWriter writer = new NDRWriter();
            writer.WriteStructure(WkstaInfo);
            writer.WriteUInt32((uint)Result);

            return writer.GetBytes();
        }
    }
}
