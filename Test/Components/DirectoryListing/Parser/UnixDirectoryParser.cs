using System.Collections.Generic;

namespace CoreFtp.Components.DirectoryListing.Parser
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using Enum;
    using Infrastructure;
    using Microsoft.Extensions.Logging;

    public class UnixDirectoryParser : IListDirectoryParser
    {
        private readonly List<Regex> unixRegexList = new List<Regex>()
        {
            new Regex(@"(?<permissions>.+)\s+" +
                      @"(?<objectcount>\d+)\s+" +
                      @"(?<user>.+)\s+" +
                      @"(?<group>.+)\s+" +
                      @"(?<size>\d+)\s+" +
                      @"(?<date>\w+\s+\d+\s+\d+:\d+|\w+\s+\d+\s+\d+)\s+" +
                      @"(?<name>.*)$", RegexOptions.Compiled),

            // Non-standard StingRay FTP Server

            new Regex(@"(?<permissions>.+)\s+" +
                      @"(?<objectcount>\d+)\s+" +
                      @"(?<size>\d+)\s+" +
                      @"(?<date>\w+\s+\d+\s+\d+:\d+|\w+\s+\d+\s+\d+)\s+" +
                      @"(?<name>.*)$", RegexOptions.Compiled)

        };

        private ILogger logger;

        public UnixDirectoryParser(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Test( string testString )
        {
            foreach (var expression in unixRegexList)
            {
                if (expression.Match(testString).Success) return true;
            }

            return false;
        }

        public FtpNodeInformation Parse( string line )
        {
            var matches = Match.Empty;
            foreach (var expression in unixRegexList)
            {
                if (!expression.Match(line).Success) continue;
                matches = expression.Match(line);
                break;
            }

            if ( !matches.Success )
                return null;

            var node = new FtpNodeInformation
            {
                NodeType = DetermineNodeType( matches.Groups[ "permissions" ] ),
                Name = DetermineName( matches.Groups[ "name" ] ),
                DateModified = DetermineDateModified( matches.Groups[ "date" ] ),
                Size = DetermineSize( matches.Groups[ "size" ] )
            };


            return node;
        }


        private FtpNodeType DetermineNodeType( Capture permissions )
        {
            // No permissions means we can't determine the node type
            if ( permissions.Value.Length == 0 )
                throw new InvalidDataException( "No permissions found" );

            switch ( permissions.Value[ 0 ] )
            {
                case 'd':
                    return FtpNodeType.Directory;
                case '-':
                case 's':
                    return FtpNodeType.File;
                case 'l':
                    return FtpNodeType.SymbolicLink;
                default:
                    throw new InvalidDataException( "Unexpected data format" );
            }
        }

        private string DetermineName( Capture name )
        {
            if ( name.Value.Length == 0 )
                throw new InvalidDataException( "No name found" );

            return name.Value;
        }

        private DateTime DetermineDateModified( Capture name )
        {
            return name.Value.Length == 0
                ? DateTime.MinValue
                : name.Value.ExtractFtpDate( DateTimeStyles.AssumeLocal );
        }

        private long DetermineSize( Capture sizeGroup )
        {
            if ( sizeGroup.Value.Length == 0 )
                return 0;

            long size;

            return long.TryParse( sizeGroup.Value, out size )
                ? size
                : 0;
        }
    }
}
