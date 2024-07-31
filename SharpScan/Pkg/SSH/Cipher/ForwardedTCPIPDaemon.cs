using System;
using Tamir.SharpSsh.Sharp.lang;

namespace Tamir.SharpSsh.jsch
{
    public interface ForwardedTCPIPDaemon : Runnable
    {
        void setChannel(ChannelForwardedTCPIP channel);
        void setArg(Object[] arg);
    }
}