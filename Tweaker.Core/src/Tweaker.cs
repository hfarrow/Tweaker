using System.Collections.Generic;
using System.Reflection;
using System;
using Ghostbit.Tweaker.Core.Events;

namespace Ghostbit.Tweaker.Core
{
    public interface ITweakerObject
    {
        string Name { get; }
        bool IsPublic { get; }
        Assembly Assembly { get; }
    }

    public interface ITweakerAttribute
    {
        string Name { get; }
        Guid Guid { get; }
    }

    public class TweakerObjectInfo
    {
        public string Name { get; private set; }

        public TweakerObjectInfo(string name)
        {
            Name = name;
        }
    }

    public class TweakerObject : ITweakerObject
    {
        private TweakerObjectInfo Info { get; set; }

        private readonly bool isPublic;
        private readonly object instance;
        private readonly Assembly assembly;

        public string Name
        {
            get { return Info.Name; }
        }

        public bool IsPublic
        {
            get { return isPublic; }
        }

        public Assembly Assembly
        {
            get { return assembly; }
        }

        public object Instance
        {
            get { return instance; }
        }

        public TweakerObject(TweakerObjectInfo info, Assembly assembly, object instance, bool isPublic)
        {
            Info = info;
            this.assembly = assembly;
            this.instance = instance;
            this.isPublic = isPublic;
        }
    }

    [Flags]
    public enum TweakerOptionFlags
    {
        None = 0,
        Default = 1,
        ScanForInvokables = 2,
        ScanForTweakables = 4,
        ScanForWatchables = 8,
        ScanInEverything = 16,
        ScanInEntryAssembly = 32,
        ScanInExecutingAssembly = 64,
        ScanInNonSystemAssemblies = 128,
        NoServer = 256,
        NoRemoteClients = 512
    }

    public class TweakerOptions
    {
        public TweakerOptionFlags Flags = TweakerOptionFlags.Default;
    }

    public class Tweaker
    {
        public event Action Initialized;

        private TweakerContext context;
        private Scanner scanner;

        public bool IsRunning { get { return context != null; } }

        public void Init(TweakerOptions options = null, Scanner scanner = null)
        {
            if(options == null)
            {
                options = new TweakerOptions();
            }

            this.scanner = scanner != null ? scanner : Scanner.Global;

            if (options.Flags == TweakerOptionFlags.None || (options.Flags & TweakerOptionFlags.Default) != 0)
            {
                options.Flags = (TweakerOptionFlags)int.MaxValue;
                options.Flags &= ~TweakerOptionFlags.ScanInEverything;
                options.Flags &= ~TweakerOptionFlags.NoServer;
                options.Flags &= ~TweakerOptionFlags.NoRemoteClients;
            }

            context = new TweakerContext(options, this.scanner);
            context.autoStartup = false;
            context.Start();
            context.dispatcher.AddListener(CoreEvent.INITIALIZED, () => { if (Initialized != null) Initialized(); });
            context.Launch();
        }

        public void ConnectClient(IClient client)
        {
            context.dispatcher.Dispatch(CoreEvent.CONNECT_CLIENT, client);
        }

        public void DisconnectClient(IClient client)
        {
            context.dispatcher.Dispatch(CoreEvent.DICONNECT_CLIENT, client);
        }

        public void Shutdown()
        {
            context.dispatcher.Dispatch(CoreEvent.SHUTDOWN);
            context = null;
            scanner = null;
        }
    }
}