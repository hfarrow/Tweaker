using Ghostbit.strange;
using Ghostbit.Tweaker.Core.Commands;
using Ghostbit.Tweaker.Core.Events;
using NLog;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class TweakerContext : ViewlessContext
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private TweakerOptions options;
        private Scanner scanner;

        public TweakerContext(TweakerOptions options, Scanner scanner) :
            base(ContextStartupFlags.MANUAL_MAPPING)
        {
            logger.Info("Constructed...");
            logger.Info("\t options = {0}", options);
            logger.Info("\t scanner = {0}", scanner);
            this.options = options;
            this.scanner = scanner != null ? scanner : Scanner.Global;
        }

        protected override void mapBindings()
        {
            logger.Debug("mapBindings");
            injectionBinder.Bind<TweakerOptions>().ToValue(options);
            injectionBinder.Bind<Scanner>().ToValue(scanner);
            injectionBinder.Bind<IServer>().To<Server>().ToSingleton();

            MapManagers();

            commandBinder.Bind(ContextEvent.START).To<InitCmd>().Once();
            commandBinder.Bind(CoreEvent.START_SERVER).To<StartServerCmd>();
            commandBinder.Bind(CoreEvent.CONNECT_CLIENT).To<ConnectClientCmd>();
            commandBinder.Bind(CoreEvent.DICONNECT_CLIENT).To<DisconnectClientCmd>();
            commandBinder.Bind(CoreEvent.START_REMOTE_CLIENT_LISTENER).To<ListenForRemoteClientsCmd>();
            commandBinder.Bind(CoreEvent.SHUTDOWN).To<ShutdownCmd>();

            ////Injection binding.
            ////Map a mock model and a mock service, both as Singletons
            //injectionBinder.Bind<IExampleModel>().To<ExampleModel>().ToSingleton();
            //injectionBinder.Bind<IExampleService>().To<ExampleService>().ToSingleton();

            ////View/Mediator binding
            ////This Binding instantiates a new ExampleMediator whenever as ExampleView
            ////Fires its Awake method. The Mediator communicates to/from the View
            ////and to/from the App. This keeps dependencies between the view and the app
            ////separated.
            //mediationBinder.Bind<ExampleView>().To<ExampleMediator>();

            ////Event/Command binding
            //commandBinder.Bind(ExampleEvent.REQUEST_WEB_SERVICE).To<CallWebServiceCommand>();
            ////The START event is fired as soon as mappings are complete.
            ////Note how we've bound it "Once". This means that the mapping goes away as soon as the command fires.
            //commandBinder.Bind(ContextEvent.START).To<StartCommand>().Once();
        }

        private void MapManagers()
        {
            logger.Debug("MapManagers");
            IInvokableManager invokables;
            if ((options.Flags & TweakerOptionFlags.ScanForInvokables) != 0)
                invokables = new InvokableManager(this.scanner);
            else
                invokables = new InvokableManager(null);

            ITweakableManager tweakables;
            if ((options.Flags & TweakerOptionFlags.ScanForTweakables) != 0)
                tweakables = new TweakableManager(this.scanner);
            else
                tweakables = new TweakableManager(null);

            IWatchableManager watchables;
            if ((options.Flags & TweakerOptionFlags.ScanForWatchables) != 0)
                watchables = new WatchableManager(this.scanner);
            else
                watchables = new WatchableManager();

            injectionBinder.Bind<IInvokableManager>().ToValue(invokables);
            injectionBinder.Bind<ITweakableManager>().ToValue(tweakables);
            injectionBinder.Bind<IWatchableManager>().ToValue(watchables);
        }
    }
}
