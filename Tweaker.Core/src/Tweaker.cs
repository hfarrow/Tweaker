using System.Collections.Generic;
using System.Reflection;
using System;

namespace Simtastic.Tweaker.Core
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
        Invokables = 1,
        Tweakables = 2,
        Watchables = 4,
        ScanEverything = 8,
        ScanEntryAssembly = 16,
        ScanExecutingAssembly = 32,
        ScanNonSystemAssemblies = 64
    }

    public static class Tweaker
    {
        public static IInvokableManager Invokables { get; private set; }
        public static ITweakableManager Tweakables { get; private set; }
        public static IWatchableManager Watchables { get; private set; }

        public static void Init(TweakerOptions options = TweakerOptions.Default)
        {
            if((options & TweakerOptions.Default) != 0)
            {
                options = (TweakerOptions)int.MaxValue;
                options &= ~TweakerOptions.ScanEverything;
            }

            if((options & TweakerOptions.Invokables) != 0)
            {
                Invokables = new InvokableManager(Scanner.Global);
            }

            if((options & TweakerOptions.Tweakables) != 0)
            {
                Tweakables = new TweakableManager(Scanner.Global);
            }

            if((options & TweakerOptions.Watchables) != 0)
            {
                //Watchables = new WatchableManager(Scanner.Global);
            }

            if ((options & TweakerOptions.ScanEverything) != 0)
            {
                ScanEverything();
            }
            else if ((options & TweakerOptions.ScanNonSystemAssemblies) != 0)
            {
                ScanNonSystemAssemblies();
            }
            else
            {
                List<Assembly> assemblies = new List<Assembly>();
                if ((options & TweakerOptions.ScanExecutingAssembly) != 0)
                {
                    assemblies.Add(Assembly.GetCallingAssembly());
                }

                if ((options & TweakerOptions.ScanEntryAssembly) != 0)
                {
                    assemblies.Add(Assembly.GetEntryAssembly());
                }

                ScanOptions scanOptions = new ScanOptions();
                scanOptions.Assemblies.ScannableRefs = assemblies.ToArray();
                ScanWithOptions(scanOptions);
            }
        }

        public static void ScanWithOptions(ScanOptions options)
        {
            Scanner.Global.Scan(options);
        }

        public static void ScanEverything()
        {
            ScanWithOptions(null);
        }

        public static void ScanEntryAssembly()
        {
            ScanOptions options = new ScanOptions();
            options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetEntryAssembly() };
            ScanWithOptions(options);
        }

        public static void ScanNonSystemAssemblies()
        {
            ScanOptions options = new ScanOptions();
            options.Assemblies.NameRegex = @"^(?!(System\.)|System$).+";
            ScanWithOptions(options);
        }
    }
}