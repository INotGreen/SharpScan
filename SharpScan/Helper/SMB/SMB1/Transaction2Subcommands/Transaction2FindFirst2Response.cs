/* Copyright (C) 2014 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace SMBLibrary.SMB1
{
    /// <summary>
    /// TRANS2_FIND_FIRST2 Response
    /// </summary>
    public class Transaction2FindFirst2Response : Transaction2Subcommand
    {
        public const int ParametersLength = 10;
        // Parameters:
        public ushort SID; // Search handle
        private ushort SearchCount;
        public bool EndOfSearch;
        public ushort EaErrorOffset;
        public ushort LastNameOffset;
        // Data:
        private byte[] FindInformationListBytes = new byte[0];

        public Transaction2FindFirst2Response() : base()
        {
        }

        public Transaction2FindFirst2Response(byte[] parameters, byte[] data, bool isUnicode) : base()
        {
            SID = LittleEndianConverter.ToUInt16(parameters, 0);
            SearchCount = LittleEndianConverter.ToUInt16(parameters, 2);
            EndOfSearch = LittleEndianConverter.ToUInt16(parameters, 4) != 0;
            EaErrorOffset = LittleEndianConverter.ToUInt16(parameters, 6);
            LastNameOffset = LittleEndianConverter.ToUInt16(parameters, 8);

            FindInformationListBytes = data;
        }

        public override byte[] GetParameters(bool isUnicode)
        {
            byte[] parameters = new byte[ParametersLength];
            LittleEndianWriter.WriteUInt16(parameters, 0, SID);
            LittleEndianWriter.WriteUInt16(parameters, 2, SearchCount);
            LittleEndianWriter.WriteUInt16(parameters, 4, Convert.ToUInt16(EndOfSearch));
            LittleEndianWriter.WriteUInt16(parameters, 6, EaErrorOffset);
            LittleEndianWriter.WriteUInt16(parameters, 8, LastNameOffset);
            return parameters;
        }

        public override byte[] GetData(bool isUnicode)
        {
            return FindInformationListBytes;
        }

        public FindInformationList GetFindInformationList(FindInformationLevel findInformationLevel, bool isUnicode)
        {
            return new FindInformationList(FindInformationListBytes, findInformationLevel, isUnicode);
        }

        public void SetFindInformationList(FindInformationList findInformationList, bool isUnicode)
        {
            SearchCount = (ushort)findInformationList.Count;
            FindInformationListBytes = findInformationList.GetBytes(isUnicode);
        }

        public override Transaction2SubcommandName SubcommandName
        {
            get
            {
                return Transaction2SubcommandName.TRANS2_FIND_FIRST2;
            }
        }
    }
}
