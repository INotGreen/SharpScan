namespace CoreFtp.Components.DirectoryListing.Parser
{
    using Infrastructure;

    public interface IListDirectoryParser
    {
        bool Test( string testString );
        FtpNodeInformation Parse( string line );
    }
}