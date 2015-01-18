using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Events
{
    static public class CoreEvent
    {
        public const string INITIALIZED = "CORE_INITIALIZED";
        public const string START_SERVER = "CORE_START_SERVER";
        public const string SERVER_STARTED = "CORE_SERVER_STARTED";
        public const string LISTEN_FOR_CLIENTS = "CORE_LISTEN_FOR_CLIENTS";
        public const string CLIENT_CONNECTED = "CORE_CLIENT_CONNECTED";
        public const string CLIENT_DISCONNECTED = "CORE_CLIENT_DISCONNECTED";
        public const string CONNECT_CLIENT = "CORE_CONNECT_CLIENT";
        public const string DICONNECT_CLIENT = "CORE_DISCONNECT_CLIENT";
        public const string START_REMOTE_CLIENT_LISTENER = "CORE_START_REMOTE_CLIENT_LISTENER";
        public const string STOP_REMOTE_CLIENT_LISTENER = "CORE_STOP_REMOTE_CLIENT_LISTENER";
        public const string SHUTDOWN = "CORE_SHUTDOWN";
    }
}
