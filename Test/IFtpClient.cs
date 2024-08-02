namespace CoreFtp
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Enum;
    using Infrastructure;
    using Microsoft.Extensions.Logging;

    public interface IFtpClient : IDisposable
    {
        ILogger Logger { set; }

        bool IsConnected { get;  }

        bool IsEncrypted { get; }

        bool IsAuthenticated { get; }

        string WorkingDirectory { get; }

        void Configure(FtpClientConfiguration configuration);

        Task LoginAsync();

        Task LogOutAsync();

        Task ChangeWorkingDirectoryAsync(string directory);

        Task CreateDirectoryAsync(string directory);

        Task RenameAsync(string from, string to);

        Task DeleteDirectoryAsync(string directory);

        Task<FtpResponse> SetClientName(string clientName);

        Task<Stream> OpenFileReadStreamAsync(string fileName);

        Task<Stream> OpenFileWriteStreamAsync(string fileName);

        Task CloseFileDataStreamAsync(CancellationToken ctsToken = default(CancellationToken));

        Task<ReadOnlyCollection<FtpNodeInformation>> ListAllAsync();

        Task<ReadOnlyCollection<FtpNodeInformation>> ListFilesAsync();

        Task<ReadOnlyCollection<FtpNodeInformation>> ListDirectoriesAsync();

        Task DeleteFileAsync(string fileName);

        Task SetTransferMode(FtpTransferMode transferMode, char secondType = '\0');

        Task<long> GetFileSizeAsync(string fileName);

        Task<FtpResponse> SendCommandAsync(FtpCommandEnvelope envelope, CancellationToken token = default(CancellationToken));

        Task<FtpResponse> SendCommandAsync(string command, CancellationToken token = default(CancellationToken));
    }
}
