using Ghostbit.Tweaker.Core.Events;
using NLog;
using strange.extensions.command.impl;
using strange.extensions.dispatcher.eventdispatcher.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Commands
{
    class StartServerCmd : TweakerCmd
    {
        public StartServerCmd() : base(LogManager.GetCurrentClassLogger()) {}

        [Inject]
        public TweakerOptions options { get; set; }

        [Inject]
        public IServer server { get; set; }

        protected override void DoExecute()
        {
            server.Start();
            dispatcher.Dispatch(CoreEvent.SERVER_STARTED);
        }
    }
}
