namespace CoreFtp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Components.DirectoryListing;
    using Components.DnsResolution;
    using Enum;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Infrastructure.Stream;
    using Microsoft.Extensions.Logging;

    public class FtpClient : IFtpClient
    {
        private IDirectoryProvider directoryProvider;
        private ILogger logger;
        private Stream dataStream;
        internal readonly SemaphoreSlim dataSocketSemaphore = new SemaphoreSlim( 1, 1 );
        public FtpClientConfiguration Configuration { get; private set; }

        internal IEnumerable<string> Features { get; private set; }
        internal FtpControlStream ControlStream { get; private set; }
        public bool IsConnected => ControlStream != null && ControlStream.IsConnected;
        public bool IsEncrypted => ControlStream != null && ControlStream.IsEncrypted;
        public bool IsAuthenticated { get; private set; }
        public string WorkingDirectory { get; private set; } = "/";

        public ILogger Logger
        {
            private get { return logger; }
            set
            {
                logger = value;
                ControlStream.Logger = value;
            }
        }

        public FtpClient() { }

        public FtpClient( FtpClientConfiguration configuration )
        {
            Configure( configuration );
        }

        public void Configure( FtpClientConfiguration configuration )
        {
            Configuration = configuration;

            if ( configuration.Host == null )
                throw new ArgumentNullException( nameof( configuration.Host ) );

            if ( Uri.IsWellFormedUriString( configuration.Host, UriKind.Absolute ) )
            {
                configuration.Host = new Uri( configuration.Host ).Host;
            }


            ControlStream = new FtpControlStream( Configuration, new DnsResolver() );
            Configuration.BaseDirectory = $"/{Configuration.BaseDirectory.TrimStart( '/' )}";
        }

        /// <summary>
        ///     Attempts to log the user in to the FTP Server
        /// </summary>
        /// <returns></returns>
        public async Task LoginAsync()
        {
            if ( IsConnected )
                await LogOutAsync();

            string username = Configuration.Username.IsNullOrWhiteSpace()
                ? Constants.ANONYMOUS_USER
                : Configuration.Username;

            await ControlStream.ConnectAsync();

            var usrResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.USER,
                Data = username
            } );

            await BailIfResponseNotAsync( usrResponse, FtpStatusCode.SendUserCommand, FtpStatusCode.SendPasswordCommand, FtpStatusCode.LoggedInProceed );

            var passResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.PASS,
                Data = username != Constants.ANONYMOUS_USER ? Configuration.Password : string.Empty
            } );

            await BailIfResponseNotAsync( passResponse, FtpStatusCode.LoggedInProceed );
            IsAuthenticated = true;

            if ( ControlStream.IsEncrypted )
            {
                await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.PBSZ,
                    Data = "0"
                } );

                await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.PROT,
                    Data = "P"
                } );
            }

            Features = await DetermineFeaturesAsync();
            directoryProvider = DetermineDirectoryProvider();
            await EnableUTF8IfPossible();
            await SetTransferMode( Configuration.Mode, Configuration.ModeSecondType );

            if ( Configuration.BaseDirectory != "/" )
            {
                await CreateDirectoryAsync( Configuration.BaseDirectory );
            }

            await ChangeWorkingDirectoryAsync( Configuration.BaseDirectory );
        }

        /// <summary>
        ///     Attemps to log the user out asynchronously, sends the QUIT command and terminates the command socket.
        /// </summary>
        public async Task LogOutAsync()
        {
            await IgnoreStaleData();
            if ( !IsConnected )
                return;

            Logger?.LogTrace( "[FtpClient] Logging out" );
            await ControlStream.SendCommandAsync( FtpCommand.QUIT );
            ControlStream.Disconnect();
            IsAuthenticated = false;
        }

        /// <summary>
        /// Changes the working directory to the given value for the current session
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task ChangeWorkingDirectoryAsync( string directory )
        {
            Logger?.LogTrace( $"[FtpClient] changing directory to {directory}" );
            if ( directory.IsNullOrWhiteSpace() || directory.Equals( "." ) )
                throw new ArgumentOutOfRangeException( nameof( directory ), "Directory supplied was incorrect" );

            EnsureLoggedIn();

            var response = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.CWD,
                Data = directory
            } );

            if ( !response.IsSuccess )
                throw new FtpException( response.ResponseMessage );

            var pwdResponse = await ControlStream.SendCommandAsync( FtpCommand.PWD );

            if ( !response.IsSuccess )
                throw new FtpException( response.ResponseMessage );

            WorkingDirectory = pwdResponse.ResponseMessage.Split( '"' )[ 1 ];
        }

        /// <summary>
        /// Creates a directory on the FTP Server
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task CreateDirectoryAsync( string directory )
        {
            if ( directory.IsNullOrWhiteSpace() || directory.Equals( "." ) )
                throw new ArgumentOutOfRangeException( nameof( directory ), "Directory supplied was not valid" );

            Logger?.LogDebug( $"[FtpClient] Creating directory {directory}" );

            EnsureLoggedIn();

            await CreateDirectoryStructureRecursively( directory.Split( '/' ), directory.StartsWith( "/" ) );
        }

        /// <summary>
        /// Renames a file on the FTP server
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task RenameAsync( string from, string to )
        {
            EnsureLoggedIn();
            Logger?.LogDebug( $"[FtpClient] Renaming from {from}, to {to}" );
            var renameFromResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.RNFR,
                Data = from
            } );

            if ( renameFromResponse.FtpStatusCode != FtpStatusCode.FileCommandPending )
                throw new FtpException( renameFromResponse.ResponseMessage );

            var renameToResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.RNTO,
                Data = to
            } );

            if ( renameToResponse.FtpStatusCode != FtpStatusCode.FileActionOK && renameToResponse.FtpStatusCode != FtpStatusCode.ClosingData )
                throw new FtpException( renameFromResponse.ResponseMessage );
        }

        /// <summary>
        /// Deletes the given directory from the FTP server
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public async Task DeleteDirectoryAsync( string directory )
        {
            if ( directory.IsNullOrWhiteSpace() || directory.Equals( "." ) )
                throw new ArgumentOutOfRangeException( nameof( directory ), "Directory supplied was not valid" );

            if ( directory == "/" )
                return;

            Logger?.LogDebug( $"[FtpClient] Deleting directory {directory}" );

            EnsureLoggedIn();

            var rmdResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.RMD,
                Data = directory
            } );

            switch ( rmdResponse.FtpStatusCode )
            {
                case FtpStatusCode.CommandOK:
                case FtpStatusCode.FileActionOK:
                    return;

                case FtpStatusCode.ActionNotTakenFileUnavailable:
                    await DeleteNonEmptyDirectory( directory );
                    return;

                default:
                    throw new FtpException( rmdResponse.ResponseMessage );
            }
        }

        /// <summary>
        /// Deletes the given directory from the FTP server
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private async Task DeleteNonEmptyDirectory( string directory )
        {
            await ChangeWorkingDirectoryAsync( directory );

            var allNodes = await ListAllAsync();

            foreach ( var file in allNodes.Where( x => x.NodeType == FtpNodeType.File ) )
            {
                await DeleteFileAsync( file.Name );
            }

            foreach ( var dir in allNodes.Where( x => x.NodeType == FtpNodeType.Directory ) )
            {
                await DeleteDirectoryAsync( dir.Name );
            }

            await ChangeWorkingDirectoryAsync( ".." );
            await DeleteDirectoryAsync( directory );
        }

        /// <summary>
        /// Informs the FTP server of the client being used
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public async Task<FtpResponse> SetClientName( string clientName )
        {
            EnsureLoggedIn();
            Logger?.LogDebug( $"[FtpClient] Setting client name to {clientName}" );

            return await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.CLNT,
                Data = clientName
            } );
        }

        /// <summary>
        /// Provides a stream which contains the data of the given filename on the FTP server
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Stream> OpenFileReadStreamAsync( string fileName )
        {
            Logger?.LogDebug( $"[FtpClient] Opening file read stream for {fileName}" );

            return new FtpDataStream( await OpenFileStreamAsync( fileName, FtpCommand.RETR ), this, Logger );
        }

        /// <summary>
        /// Provides a stream which can be written to
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Stream> OpenFileWriteStreamAsync( string fileName )
        {
            string filePath = WorkingDirectory.CombineAsUriWith( fileName );
            Logger?.LogDebug( $"[FtpClient] Opening file read stream for {filePath}" );
            var segments = filePath.Split( '/' )
                                   .Where( x => !x.IsNullOrWhiteSpace() )
                                   .ToList();
            await CreateDirectoryStructureRecursively( segments.Take( segments.Count - 1 ).ToArray(), filePath.StartsWith( "/" ) );
            return new FtpDataStream( await OpenFileStreamAsync( filePath, FtpCommand.STOR ), this, Logger );
        }

        /// <summary>
        /// Closes the write stream and associated socket (if open), 
        /// </summary>
        /// <param name="ctsToken"></param>
        /// <returns></returns>
        public async Task CloseFileDataStreamAsync( CancellationToken ctsToken = default( CancellationToken ) )
        {
            Logger?.LogTrace( "[FtpClient] Closing write file stream" );
            dataStream.Dispose();

            if ( ControlStream != null )
                await ControlStream.GetResponseAsync( ctsToken );
        }

        /// <summary>
        /// Lists all files in the current working directory
        /// </summary>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<FtpNodeInformation>> ListAllAsync()
        {
            try
            {
                EnsureLoggedIn();
                Logger?.LogDebug( $"[FtpClient] Listing files in {WorkingDirectory}" );
                return await directoryProvider.ListAllAsync();
            }
            finally
            {
                await ControlStream.GetResponseAsync();
            }
        }

        /// <summary>
        /// Lists all files in the current working directory
        /// </summary>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<FtpNodeInformation>> ListFilesAsync()
        {
            try
            {
                EnsureLoggedIn();
                Logger?.LogDebug( $"[FtpClient] Listing files in {WorkingDirectory}" );
                return await directoryProvider.ListFilesAsync();
            }
            finally
            {
                await ControlStream.GetResponseAsync();
            }
        }

        /// <summary>
        /// Lists all directories in the current working directory
        /// </summary>
        /// <returns></returns>
        public async Task<ReadOnlyCollection<FtpNodeInformation>> ListDirectoriesAsync()
        {
            try
            {
                EnsureLoggedIn();
                Logger?.LogDebug( $"[FtpClient] Listing directories in {WorkingDirectory}" );
                return await directoryProvider.ListDirectoriesAsync();
            }
            finally
            {
                await ControlStream.GetResponseAsync();
            }
        }


        /// <summary>
        /// Lists all directories in the current working directory
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeleteFileAsync( string fileName )
        {
            EnsureLoggedIn();
            Logger?.LogDebug( $"[FtpClient] Deleting file {fileName}" );
            var response = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.DELE,
                Data = fileName
            } );

            if ( !response.IsSuccess )
                throw new FtpException( response.ResponseMessage );
        }

        /// <summary>
        /// Determines the file size of the given file
        /// </summary>
        /// <param name="transferMode"></param>
        /// <param name="secondType"></param>
        /// <returns></returns>
        public async Task SetTransferMode( FtpTransferMode transferMode, char secondType = '\0' )
        {
            EnsureLoggedIn();
            Logger?.LogTrace( $"[FtpClient] Setting transfer mode {transferMode}, {secondType}" );
            var response = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.TYPE,
                Data = secondType != '\0'
                    ? $"{(char) transferMode} {secondType}"
                    : $"{(char) transferMode}"
            } );

            if ( !response.IsSuccess )
                throw new FtpException( response.ResponseMessage );
        }

        /// <summary>
        /// Determines the file size of the given file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<long> GetFileSizeAsync( string fileName )
        {
            EnsureLoggedIn();
            Logger?.LogDebug( $"[FtpClient] Getting file size for {fileName}" );
            var sizeResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = FtpCommand.SIZE,
                Data = fileName
            } );

            if ( sizeResponse.FtpStatusCode != FtpStatusCode.FileStatus )
                throw new FtpException( sizeResponse.ResponseMessage );

            long fileSize = long.Parse( sizeResponse.ResponseMessage );
            return fileSize;
        }

        /// <summary>
        /// Determines the type of directory listing the FTP server will return, and set the appropriate parser
        /// </summary>
        /// <returns></returns>
        private IDirectoryProvider DetermineDirectoryProvider()
        {
            Logger?.LogTrace( "[FtpClient] Determining directory provider" );
            if ( this.UsesMlsd() )
                return new MlsdDirectoryProvider( this, Logger, Configuration );

            return new ListDirectoryProvider( this, Logger, Configuration );
        }

        private async Task<IEnumerable<string>> DetermineFeaturesAsync()
        {
            EnsureLoggedIn();
            Logger?.LogTrace( "[FtpClient] Determining features" );
            var response = await ControlStream.SendCommandAsync( FtpCommand.FEAT );

            if ( response.FtpStatusCode == FtpStatusCode.CommandSyntaxError || response.FtpStatusCode == FtpStatusCode.CommandNotImplemented )
                return Enumerable.Empty<string>();

            var features = response.Data.Where( x => !x.StartsWith( ( (int) FtpStatusCode.SystemHelpReply ).ToString() ) && !x.IsNullOrWhiteSpace() )
                                   .Select( x => x.Replace( Constants.CARRIAGE_RETURN, string.Empty ).Trim() )
                                   .ToList();

            return features;
        }

        /// <summary>
        /// Creates a directory structure recursively given a path
        /// </summary>
        /// <param name="directories"></param>
        /// <param name="isRootedPath"></param>
        /// <returns></returns>
        private async Task CreateDirectoryStructureRecursively( IReadOnlyCollection<string> directories, bool isRootedPath )
        {
            Logger?.LogDebug( $"[FtpClient] Creating directory structure recursively {string.Join( "/", directories )}" );
            string originalPath = WorkingDirectory;

            if ( isRootedPath && directories.Any() )
                await ChangeWorkingDirectoryAsync( "/" );

            if ( !directories.Any() )
                return;

            if ( directories.Count == 1 )
            {
                await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.MKD,
                    Data = directories.First()
                } );

                await ChangeWorkingDirectoryAsync( originalPath );
                return;
            }

            foreach ( string directory in directories )
            {
                if ( directory.IsNullOrWhiteSpace() )
                    continue;

                var response = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.CWD,
                    Data = directory
                } );

                if ( response.FtpStatusCode != FtpStatusCode.ActionNotTakenFileUnavailable )
                    continue;

                await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.MKD,
                    Data = directory
                } );
                await ControlStream.SendCommandAsync( new FtpCommandEnvelope
                {
                    FtpCommand = FtpCommand.CWD,
                    Data = directory
                } );
            }

            await ChangeWorkingDirectoryAsync( originalPath );
        }


        /// <summary>
        /// Opens a filestream to the given filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task<Stream> OpenFileStreamAsync( string fileName, FtpCommand command )
        {
            EnsureLoggedIn();
            Logger?.LogDebug( $"[FtpClient] Opening filestream for {fileName}, {command}" );
            dataStream = await ConnectDataStreamAsync();

            var retrResponse = await ControlStream.SendCommandAsync( new FtpCommandEnvelope
            {
                FtpCommand = command,
                Data = fileName
            } );

            if ( ( retrResponse.FtpStatusCode != FtpStatusCode.DataAlreadyOpen ) &&
                 ( retrResponse.FtpStatusCode != FtpStatusCode.OpeningData ) &&
                 ( retrResponse.FtpStatusCode != FtpStatusCode.ClosingData ) )
                throw new FtpException( retrResponse.ResponseMessage );

            return dataStream;
        }

        /// <summary>
        /// Checks if the command socket is open and that an authenticated session is active
        /// </summary>
        private void EnsureLoggedIn()
        {
            if ( !IsConnected || !IsAuthenticated )
                throw new FtpException( "User must be logged in" );
        }

        /// <summary>
        /// Produces a data socket using Passive (PASV) or Extended Passive (EPSV) mode
        /// </summary>
        /// <returns></returns>
        internal async Task<Stream> ConnectDataStreamAsync()
        {
            Logger?.LogTrace( "[FtpClient] Connecting to a data socket" );

            var epsvResult = await ControlStream.SendCommandAsync( FtpCommand.EPSV );

            int? passivePortNumber;
            if ( epsvResult.FtpStatusCode == FtpStatusCode.EnteringExtendedPassive )
            {
                passivePortNumber = epsvResult.ResponseMessage.ExtractEpsvPortNumber();
            }
            else
            {
                // EPSV failed - try regular PASV
                var pasvResult = await ControlStream.SendCommandAsync( FtpCommand.PASV );
                if ( pasvResult.FtpStatusCode != FtpStatusCode.EnteringPassive )
                    throw new FtpException( pasvResult.ResponseMessage );

                passivePortNumber = pasvResult.ResponseMessage.ExtractPasvPortNumber();
            }

            if ( !passivePortNumber.HasValue )
                throw new FtpException( "Could not determine EPSV/PASV data port" );

            return await ControlStream.OpenDataStreamAsync( Configuration.Host, passivePortNumber.Value, CancellationToken.None );
        }

        /// <summary>
        /// Throws an exception if the server response is not one of the given acceptable codes
        /// </summary>
        /// <param name="response"></param>
        /// <param name="codes"></param>
        /// <returns></returns>
        private async Task BailIfResponseNotAsync( FtpResponse response, params FtpStatusCode[] codes )
        {
            if ( codes.Any( x => x == response.FtpStatusCode ) )
                return;

            Logger?.LogDebug( $"Bailing due to response codes being {response.FtpStatusCode}, which is not one of: [{string.Join( ",", codes )}]" );

            await LogOutAsync();
            throw new FtpException( response.ResponseMessage );
        }

        /// <summary>
        /// Determine if the FTP server supports UTF8 encoding, and set it to the default if possible
        /// </summary>
        /// <returns></returns>
        private async Task EnableUTF8IfPossible()
        {
            if ( Equals( ControlStream.Encoding, Encoding.ASCII ) && Features.Any( x => x == Constants.UTF8 ) )
            {
                ControlStream.Encoding = Encoding.UTF8;
            }

            if ( Equals( ControlStream.Encoding, Encoding.UTF8 ) )
            {
                // If the server supports UTF8 it should already be enabled and this
                // command should not matter however there are conflicting drafts
                // about this so we'll just execute it to be safe. 
                await ControlStream.SendCommandAsync( "OPTS UTF8 ON" );
            }
        }

        public async Task<FtpResponse> SendCommandAsync( FtpCommandEnvelope envelope, CancellationToken token = default( CancellationToken ) )
        {
            return await ControlStream.SendCommandAsync( envelope, token );
        }

        public async Task<FtpResponse> SendCommandAsync( string command, CancellationToken token = default( CancellationToken ) )
        {
            return await ControlStream.SendCommandAsync( command, token );
        }

        /// <summary>
        /// Ignore any stale data we may have waiting on the stream
        /// </summary>
        /// <returns></returns>
        private async Task IgnoreStaleData()
        {
            if ( IsConnected && ControlStream.SocketDataAvailable() )
            {
                var staleData = await ControlStream.GetResponseAsync();
                Logger?.LogWarning( $"Stale data detected: {staleData.ResponseMessage}" );
            }
        }

        public void Dispose()
        {
            Logger?.LogDebug( "Disposing of FtpClient" );
            Task.WaitAny( LogOutAsync() );
            ControlStream?.Dispose();
            dataSocketSemaphore?.Dispose();
        }
    }
}
