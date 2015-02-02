using System.Collections.Generic;
using System.Reflection;
using System;
using Ghostbit.Tweaker.AssemblyScanner;
using Ghostbit.Tweaker.Core;

namespace Ghostbit.Tweaker
{
    public class Tweaker
    {
        /// <summary>
        /// Get the invokable manager created during initialization.
        /// </summary>
        public IInvokableManager Invokables { get; private set; }

        /// <summary>
        /// Get the tweakable manager created during initialization.
        /// </summary>
        public ITweakableManager Tweakables { get; private set; }

        /// <summary>
        /// Get the watchable manager created during initialization.
        /// </summary>
        public IWatchableManager Watchables { get; private set; }

        /// <summary>
        /// Get the options that were used during initialization.
        /// </summary>
        public TweakerOptions Options { get; private set; }

        private IScanner scanner;

        /// <summary>
        /// Initialize Tweaker with the provided options and scanner.
        /// </summary>
        /// <param name="options">Options to initialize Tweaker with. Default options will be used if null.</param>
        /// <param name="scanner">Scanner to use for automatically registering Tweaker objects. Scanner.Global will be used if null.</param>
        public void Init(TweakerOptions options = null, IScanner scanner = null)
        {
            if(options == null)
            {
                options = new TweakerOptions();
            }
            Options = options;

            TweakerOptionFlags flags = Options.Flags;
            this.scanner = scanner != null ? scanner : Scanner.Global;

            if (flags == TweakerOptionFlags.None || (flags & TweakerOptionFlags.Default) != 0)
            {
                flags = (TweakerOptionFlags)int.MaxValue;
                flags &= ~TweakerOptionFlags.ScanInEverything;
                flags &= ~TweakerOptionFlags.DoNotScan;
                Options.Flags = flags;
            }

            CreateManagers();

            if ((flags & TweakerOptionFlags.DoNotScan) == 0)
            {
                PerformScan();
            }
        }

        private void CreateManagers()
        {
            TweakerOptionFlags flags = Options.Flags;

            if ((flags & TweakerOptionFlags.ScanForInvokables) != 0)
            {
                Invokables = new InvokableManager(this.scanner);
            }
            else
            {
                Invokables = new InvokableManager(null);
            }

            if ((flags & TweakerOptionFlags.ScanForTweakables) != 0)
            {
                Tweakables = new TweakableManager(this.scanner);
            }
            else
            {
                Tweakables = new TweakableManager(null);
            }

            if ((flags & TweakerOptionFlags.ScanForWatchables) != 0)
            {
                Watchables = new WatchableManager(this.scanner);
            }
            else
            {
                Watchables = new WatchableManager();
            }
        }

        private void PerformScan()
        {
            TweakerOptionFlags flags = Options.Flags;

            if ((flags & TweakerOptionFlags.ScanInEverything) != 0)
            {
                ScanEverything();
            }
            else if ((flags & TweakerOptionFlags.ScanInNonSystemAssemblies) != 0)
            {
                ScanNonSystemAssemblies();
            }
            else
            {
                List<Assembly> assemblies = new List<Assembly>();
                if ((flags & TweakerOptionFlags.ScanInExecutingAssembly) != 0)
                {
                    assemblies.Add(Assembly.GetCallingAssembly());
                }

                if ((flags & TweakerOptionFlags.ScanInEntryAssembly) != 0)
                {
                    assemblies.Add(Assembly.GetEntryAssembly());
                }

                ScanOptions scanOptions = new ScanOptions();
                scanOptions.Assemblies.ScannableRefs = assemblies.ToArray();
                ScanWithOptions(scanOptions);
            }
        }

        private void ScanWithOptions(ScanOptions options)
        {
            scanner.Scan(options);
        }

        private void ScanEverything()
        {
            ScanWithOptions(null);
        }

        private void ScanEntryAssembly()
        {
            ScanOptions options = new ScanOptions();
            options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetEntryAssembly() };
            ScanWithOptions(options);
        }

        private void ScanNonSystemAssemblies()
        {
            ScanOptions options = new ScanOptions();
            options.Assemblies.NameRegex = @"^(?!(System\.)|System$|mscorlib$|Microsoft\.|vshost|Unity|Accessibility|Mono\.).+";
            ScanWithOptions(options);
        }
    }

    /// <summary>
    /// Flags used to configure and initialize Tweaker.
    /// </summary>
    [Flags]
    public enum TweakerOptionFlags
    {
        /// <summary>
        /// None will result in the same behaivor as Default.
        /// </summary>
        None = 0,

        /// <summary>
        /// All flags will be turned on except for ScanInEverything and DoNotScan.
        /// </summary>
        Default = 1,

        /// <summary>
        /// Invokables will automatically be registered with the invokable manager.
        /// </summary>
        ScanForInvokables = 2,

        /// <summary>
        /// Tweakables will automatically be registered with the tweakable manager.
        /// </summary>
        ScanForTweakables = 4,

        /// <summary>
        /// Watchables will automatically be registered with the watchable manager.
        /// </summary>
        ScanForWatchables = 8,

        /// <summary>
        /// Scan all loaded assemblies.
        /// </summary>
        /// <remarks>
        /// Takes precedence over ScanInNonSystemAssemblies, ScanInEntryAssembly, and ScanInExecutingAssembly.
        /// </remarks>
        ScanInEverything = 16,

        /// <summary>
        /// Scan the entry assembly. Can be combined with ScanInExecutingAssembly.
        /// </summary>
        ScanInEntryAssembly = 32,

        /// <summary>
        /// Scan the executing assembly. Can be combined with ScanInEntryAssembly.
        /// </summary>
        ScanInExecutingAssembly = 64,

        /// <summary>
        /// Only scan in non system assemblies.
        /// </summary>
        /// <remarks>See ScanNonSystemAssemblies for what is considered a system assembly.</remarks>
        ScanInNonSystemAssemblies = 128,

        /// <summary>
        /// Do not perform a scan. All tweaker objects must be registered manually.
        /// </summary>
        /// <remarks>Takes precedence oper ScanInEntryAssembly and ScanInExecutingAssembly</remarks>
        DoNotScan = 256
    }

    /// <summary>
    /// Options for configuring Tweaker.
    /// </summary>
    public class TweakerOptions
    {
        public TweakerOptionFlags Flags = TweakerOptionFlags.Default;
    }
}