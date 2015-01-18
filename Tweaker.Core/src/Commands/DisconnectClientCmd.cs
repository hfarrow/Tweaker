﻿using Ghostbit.Tweaker.Core.Commands;
using Ghostbit.Tweaker.Core.Events;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Commands
{
    public class DisconnectClientCmd : TweakerCmd
    {
        [Inject]
        public object Data { get; set; }

        public DisconnectClientCmd() : base(LogManager.GetCurrentClassLogger()) { }

        protected override void DoExecute()
        {
            IClient client = Data as IClient;
            dispatcher.Dispatch(CoreEvent.DICONNECT_CLIENT, client);
        }
    }
}