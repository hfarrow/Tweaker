using System.Collections.Generic;
using System.Reflection;
using System;

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
    public enum TweakerOptions
    {
        Default = 0,
        ScanForInvokables = 1,
        ScanForTweakables = 2,
        ScanForWatchables = 4,
        ScanInEverything = 8,
        ScanInEntryAssembly = 16,
        ScanInExecutingAssembly = 32,
        ScanInNonSystemAssemblies = 64,
        NoServer = 128,
        NoRemoteClients = 256
    }

    public class Tweaker
    {
        TweakerContext context;
        private Scanner scanner;

        public void Init(TweakerOptions options = TweakerOptions.Default, Scanner scanner = null)
        {
            this.scanner = scanner != null ? scanner : Scanner.Global;

            if((options & TweakerOptions.Default) != 0)
            {
                options = (TweakerOptions)int.MaxValue;
                options &= ~TweakerOptions.ScanInEverything;
            }

            context = new TweakerContext(options, this.scanner);
        }
    }
}