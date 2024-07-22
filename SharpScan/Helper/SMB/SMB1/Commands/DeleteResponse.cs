/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SMBLibrary.SMB1
{
    /// <summary>
    /// SMB_COM_DELETE Response
    /// </summary>
    public class DeleteResponse : SMB1Command
    {
        public DeleteResponse() : base()
        {
        }

        public DeleteResponse(byte[] buffer, int offset) : base(buffer, offset, false)
        {
        }

        public override byte[] GetBytes(bool isUnicode)
        {
            return base.GetBytes(isUnicode);
        }

        public override CommandName CommandName
        {
            get
            {
                return CommandName.SMB_COM_DELETE;
            }
        }
    }
}
