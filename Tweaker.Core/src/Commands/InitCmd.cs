using strange.extensions.context.api;
using strange.extensions.command.impl;
using strange.extensions.dispatcher.eventdispatcher.impl;
using System.Reflection;
using System.Collections.Generic;
using strange.extensions.dispatcher.eventdispatcher.api;
using Ghostbit.Tweaker.Core.Events;
using NLog;

namespace Ghostbit.Tweaker.Core.Commands
{
    public class InitCmd : TweakerCmd
    {
        [Inject]
        public TweakerOptions options { get; set; }

        [Inject]
        public Scanner scanner { get; set; }

        public InitCmd() : base(LogManager.GetCurrentClassLogger()) { }

        protected override void DoExecute()
        {
            TweakerOptionFlags flags = options.Flags;
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

            if ((flags & TweakerOptionFlags.NoServer) == 0)
            {
                dispatcher.AddListener(CoreEvent.SERVER_STARTED, OnServerStarted);
                dispatcher.Dispatch(CoreEvent.START_SERVER, null);
            }
            else
            {
                dispatcher.Dispatch(CoreEvent.INITIALIZED, null);
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
            options.Assemblies.NameRegex = @"^(?!(System\.)|System$|mscorlib$|Microsoft\.|vshost).+";
            ScanWithOptions(options);
        }

        private void OnServerStarted()
        {
            dispatcher.RemoveListener(CoreEvent.SERVER_STARTED, OnServerStarted);
            dispatcher.Dispatch(CoreEvent.INITIALIZED, null);
        }
    }
}