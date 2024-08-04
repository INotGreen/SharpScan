// Copyright © 2004, 2013, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySql.Data.MySqlClient
{
    public class MySqlTrace
    {
        private static string qaHost;
        private static bool qaEnabled = false;

        public static TraceListenerCollection Listeners
        {
            get { throw new NotImplementedException(); }
        }

        public static SourceSwitch Switch
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public static bool QueryAnalysisEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public static void EnableQueryAnalyzer(string host, int postInterval)
        {
            throw new NotImplementedException();
        }

        public static void DisableQueryAnalyzer()
        {
            throw new NotImplementedException();
        }

        public static TraceSource Source
        {
            get { throw new NotImplementedException(); }
        }

        public static void LogInformation(int id, string msg)
        {
          //TODO implement
        }

        public static void LogWarning(int id, string msg)
        {
          //TODO implement
        }

        public static void LogError(int id, string msg)
        {
            //TODO implement
        }

        public static void TraceEvent(TraceEventType eventType, MySqlTraceEventType mysqlEventType, string msgFormat, params object[] args)
        {
            throw new NotImplementedException();
        }
    }

    // These types are just placeholders to keep same api for MySqlTrace in both .NET & RT (ie. they supply missing types in .NET for RT).
    public enum TraceEventType
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Information = 4,
        Verbose = 5
    }

    public class TraceListenerCollection
    {
    }

    public class TraceSource
    {
        public void TraceEvent(TraceEventType type, int id, string msg, MySqlTraceEventType type2, int code)
        {
            throw new NotImplementedException();
        }

        public void TraceEvent(TraceEventType type, int id, string msg, params object[] args)
        {
            throw new NotImplementedException();
        }
    }

    public class SourceSwitch
    {

    }

    internal static class Trace
    {
        internal static void TraceInformation(string msg)
        {
            throw new NotImplementedException();
        }

        internal static void TraceWarning(string msg)
        {
            throw new NotImplementedException();
        }

        internal static void TraceError(string msg)
        {
            throw new NotImplementedException();
        }

        internal static void TraceEven(string msg)
        {
            throw new NotImplementedException();
        }
    }

    public enum UsageAdvisorWarningFlags
    {
        NoIndex = 1,
        BadIndex,
        SkippedRows,
        SkippedColumns,
        FieldConversion
    }

    public enum MySqlTraceEventType : int
    {
        ConnectionOpened = 1,
        ConnectionClosed,
        QueryOpened,
        ResultOpened,
        ResultClosed,
        QueryClosed,
        StatementPrepared,
        StatementExecuted,
        StatementClosed,
        NonQuery,
        UsageAdvisorWarning,
        Warning,
        Error,
        QueryNormalized
    }
}
