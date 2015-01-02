using Ghostbit.Tweaker.Core.Events;
using strange.extensions.command.impl;
using strange.extensions.dispatcher.eventdispatcher.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Commands
{
    class StartServerCmd : EventCommand
    {
        [Inject]
        public TweakerOptions options { get; set; }
        [Inject]
        public IServer server { get; set; }

        public override void Execute()
        {
            server.Start();
            dispatcher.Dispatch(CoreEvent.SERVER_STARTED, null);
        }
    }
}
