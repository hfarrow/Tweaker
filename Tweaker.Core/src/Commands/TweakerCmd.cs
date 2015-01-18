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
    public class TweakerCmd : EventCommand
    {
        protected Logger logger;

        public TweakerCmd(Logger logger)
        {
            this.logger = logger;
        }

        public override void Execute()
        {
            logger.Debug("Executing");
            DoExecute();
        }
        protected virtual void DoExecute() { }
    }
}