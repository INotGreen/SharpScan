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
using Socks5.Core.Socks;

namespace Socks5.Core.ExamplePlugins
{

    public class LoginHandlerExample : LoginHandler
    {
        //Username/Password Table? Endless possiblities for the login system.

        public override bool Enabled { get; set; } = false;

        public override bool OnStart()
        {
            return true;
        }

        public override LoginStatus HandleLogin(User user)
        {
            return user.Username == "thrdev" && user.Password == "testing1234" ? LoginStatus.Correct : LoginStatus.Denied;
        }
    }
}
