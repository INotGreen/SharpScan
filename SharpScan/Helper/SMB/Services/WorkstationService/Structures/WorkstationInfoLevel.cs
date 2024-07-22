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
using Utilities;

namespace SMBLibrary.Services
{
    public abstract class WorkstationInfoLevel : INDRStructure
    {
        public abstract void Read(NDRParser parser);

        public abstract void Write(NDRWriter writer);

        public abstract uint Level
        {
            get;
        }
    }
}
