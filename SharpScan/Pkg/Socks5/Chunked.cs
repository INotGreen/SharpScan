using System;
using System.Net.Sockets;

namespace Socks5.Core
{
    public class Chunked
    {
        private readonly byte[] _totalbuff;

        /// <summary>
        ///     Create a new instance of chunked.
        /// </summary>
        /// <param name="f"></param>
        public Chunked(Socket f, byte[] oldbuffer, int size)
        {
            //Find first chunk.
            if (IsChunked(oldbuffer))
            {
                var endofheader = oldbuffer.FindString("\r\n\r\n");
                var endofchunked = oldbuffer.FindString("\r\n", endofheader + 4);
                //
                var chunked = oldbuffer.GetBetween(endofheader + 4, endofchunked);
                //convert chunked data to int.
                var totallen = chunked.FromHex();
                //
                if (totallen > 0)
                {
                    //start a while loop and receive till end of chunk.
                    _totalbuff = new byte[65535];
                    RawData = new byte[size];
                    //remove chunk data before adding.
                    oldbuffer = oldbuffer.ReplaceBetween(endofheader + 4, endofchunked + 2, new byte[] { });
                    Buffer.BlockCopy(oldbuffer, 0, RawData, 0, size);
                    if (f.Connected)
                    {
                        var totalchunksize = 0;
                        var received = f.Receive(_totalbuff, 0, _totalbuff.Length, SocketFlags.None);
                        while ((totalchunksize = GetChunkSize(_totalbuff, received)) != -1)
                        {
                            //add data to final byte buffer.
                            var chunkedData = GetChunkData(_totalbuff, received);
                            var tempData = new byte[chunkedData.Length + RawData.Length];
                            //get data AFTER chunked response.
                            Buffer.BlockCopy(RawData, 0, tempData, 0, RawData.Length);
                            Buffer.BlockCopy(chunkedData, 0, tempData, RawData.Length, chunkedData.Length);
                            //now add to finalbuff.
                            RawData = tempData;
                            //receive again.
                            if (totalchunksize == -2)
                                break;
                            received = f.Receive(_totalbuff, 0, _totalbuff.Length, SocketFlags.None);
                        }

                        //end of chunk.
                        Console.WriteLine("Got chunk! Size: {0}", RawData.Length);
                    }
                }
                else
                {
                    RawData = new byte[size];
                    Buffer.BlockCopy(oldbuffer, 0, RawData, 0, size);
                }
            }
        }

        public byte[] RawData { get; }

        public byte[] ChunkedData
        {
            get
            {
                //get size from \r\n\r\n and past.
                var location = RawData?.FindString("\r\n\r\n") + 4;
                //size
                var size = RawData?.Length - location - 7; //-7 is initial end of chunk data.
                return RawData?.ReplaceString("\r\n\r\n", "\r\n\r\n" + size?.ToHex().Replace("0x", "") + "\r\n");
            }
        }

        public static int GetChunkSize(byte[] buffer, int count)
        {
            //chunk size is first chars till \r\n.
            if (buffer.FindString("\r\n0\r\n\r\n", count - 7) != -1)
                //end of buffer.
                return -2;
            var chunksize = buffer.GetBetween(0, buffer.FindString("\r\n"));
            return chunksize.FromHex();
        }

        public static byte[] GetChunkData(byte[] buffer, int size)
        {
            //parse out the chunk size and return data.
            return buffer.GetInBetween(buffer.FindString("\r\n") + 2, size);
        }

        public static bool IsChunked(byte[] buffer)
        {
            return IsHttp(buffer) && buffer.FindString("Transfer-Encoding: chunked\r\n") != -1;
        }

        public static bool IsHttp(byte[] buffer)
        {
            return buffer.FindString("HTTP/1.1") != -1 && buffer.FindString("\r\n\r\n") != -1;
        }
    }
}

//WARNING: BETA - Doesn't work as well as intended. Use at your own discretion.
