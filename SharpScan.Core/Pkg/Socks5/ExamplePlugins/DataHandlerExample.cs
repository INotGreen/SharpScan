﻿/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Socks5.Core.Plugin;
using Socks5.Core.TCP;

namespace Socks5.Core.ExamplePlugins
{
    public class DataHandlerExample : DataHandler
    {
        //private string httpString = "HTTP/1.1";

        public override bool Enabled { get; set; } = false;

        public override bool OnStart()
        {
            return true;
        }

        public override void OnServerDataReceived(object sender, DataEventArgs e)
        {
            //throw new NotImplementedException();
            /*//Modify data.
           int Location = e.Buffer.FindString(httpString);
           if (Location != -1)
           {
               //find end of location.
               int EndHTTP = e.Buffer.FindString(" ", Location + 1);
               //replace between these two values.
               if (EndHTTP != -1)
               {
                   e.Buffer = e.Buffer.ReplaceBetween(Location, EndHTTP, Encoding.ASCII.GetBytes("HTTP/1.0"));
                   Console.WriteLine(Encoding.ASCII.GetString(e.Buffer, 0, e.Count));
                   //convert sender.
               }
           }*/
        }

        public override void OnClientDataReceived(object sender, DataEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}

