namespace CoreFtp.Components.DirectoryListing.Parser
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Enum;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Microsoft.Extensions.Logging;

    public class DosDirectoryParser : IListDirectoryParser
    {
        private readonly Regex dosDirectoryRegex = new Regex( @"(?<modify>\d+-\d+-\d+\s+\d+:\d+\w+)\s+<DIR>\s+(?<name>.*)$", RegexOptions.Compiled );
        private readonly Regex dosFileRegex = new Regex( @"(?<modify>\d+-\d+-\d+\s+\d+:\d+\w+)\s+(?<size>\d+)\s+(?<name>.*)$", RegexOptions.Compiled );
        private ILogger logger;

        public DosDirectoryParser( ILogger logger )
        {
            this.logger = logger;
        }

        public bool Test( string testString )
        {
            return dosDirectoryRegex.Match( testString ).Success ||
                   dosFileRegex.Match( testString ).Success;
        }

        public FtpNodeInformation Parse( string line )
        {
            var directoryMatch = dosDirectoryRegex.Match( line );
            if ( directoryMatch.Success )
            {
                return ParseDirectory( directoryMatch );
            }


            var fileMatch = dosFileRegex.Match( line );
            if ( fileMatch.Success )
            {
                return ParseFile( fileMatch );
            }

            return null;
        }

        public FtpNodeInformation ParseDirectory( Match match )
        {
            return new FtpNodeInformation
            {
                NodeType = FtpNodeType.Directory,
                Name = match.Groups[ "name" ].Value,
                DateModified = match.Groups[ "modify" ].Value.ExtractFtpDate( DateTimeStyles.AssumeLocal ),
                Size = 0
            };
        }

        public FtpNodeInformation ParseFile( Match match )
        {
            return new FtpNodeInformation
            {
                NodeType = FtpNodeType.File,
                Name = match.Groups[ "name" ].Value,
                DateModified = match.Groups[ "modify" ].Value.ExtractFtpDate( DateTimeStyles.AssumeLocal ),
                Size = match.Groups[ "size" ].Value.ParseOrDefault()
            };
        }
    }
}