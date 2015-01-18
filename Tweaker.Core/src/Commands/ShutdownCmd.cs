using Ghostbit.Tweaker.Core.Commands;
using Ghostbit.Tweaker.Core.Events;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Commands
{
    public class ShutdownCmd : TweakerCmd
    {
        [Inject]
        public IServer server { get; set; }

        public ShutdownCmd() : base(LogManager.GetCurrentClassLogger()) { }

        protected override void DoExecute()
        {
            dispatcher.Dispatch(CoreEvent.STOP_REMOTE_CLIENT_LISTENER);
            server.Shutdown();
        }
    }
}