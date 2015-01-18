using strange.extensions.dispatcher.eventdispatcher.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public interface IServer
    {
        void Start();
        void Shutdown();

        IInvokableManager Invokables { get; }
        ITweakableManager Tweakables { get; }
        IWatchableManager Watchables { get; }
    }
}
