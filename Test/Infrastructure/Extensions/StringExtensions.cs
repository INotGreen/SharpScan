namespace CoreFtp.Infrastructure.Extensions
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Enum;
    using Infrastructure;

    public static class StringExtensions
    {
        internal static bool IsNullOrEmpty( this string operand )
        {
            return string.IsNullOrEmpty( operand );
        }

        internal static bool IsNullOrWhiteSpace( this string operand )
        {
            return string.IsNullOrWhiteSpace( operand );
        }

        internal static string CombineAsUriWith( this string operand, string rightHandSide )
        {
            return string.Format( "{0}/{1}", operand.TrimEnd( '/' ), rightHandSide.Trim( '/' ) );
        }

        internal static int? ExtractPasvPortNumber(this string operand)
        {
            var regex = new Regex( @"([0-9]{1,3}[|,]){1,}[0-9]{1,3}", RegexOptions.Compiled );
            var match = regex.Match( operand );

            if (!match.Success)
                return null;

            var values = match.Groups[0].Value.Split("|,".ToCharArray()).Select(int.Parse).ToArray();

            if (values.Length != 6)
                return null;

            // 5th and 6th values contain the port number
            return values[4]*256 + values[5];
        }

        internal static int? ExtractEpsvPortNumber( this string operand )
        {
            // Added , as separator
            var regex = new Regex( @"(?:[\|,])(?<PortNumber>\d+)(?:[\|,])", RegexOptions.Compiled );

            var match = regex.Match( operand );

            if ( !match.Success )
                return null;

            return int.Parse( match.Groups[ "PortNumber" ].Value );
        }

        private static FtpNodeType ToNodeType( this string operand )
        {
            switch ( operand )
            {
                case "dir":
                    return FtpNodeType.Directory;
                case "file":
                    return FtpNodeType.File;
            }

            return FtpNodeType.SymbolicLink;
        }

        internal static FtpNodeInformation ToFtpNode( this string operand )
        {
            var dictionary = operand.Split( ';' )
                                    .Select( s => s.Split( '=' ) )
                                    .ToDictionary( strings => strings.Length == 2
                                                       ? strings[ 0 ]
                                                       : "name",
                                                   strings => strings.Length == 2
                                                       ? strings[ 1 ]
                                                       : strings[ 0 ] );

            return new FtpNodeInformation
            {
                NodeType = dictionary.GetValueOrDefault( "type" ).Trim().ToNodeType(),
                Name = dictionary.GetValueOrDefault( "name" ).Trim(),
                Size = dictionary.GetValueOrDefault( "size" ).ParseOrDefault(),
                DateModified = dictionary.GetValueOrDefault( "modify" ).ParseExactOrDefault( "yyyyMMddHHmmss" )
            };
        }
    }
}