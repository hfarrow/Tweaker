using Ghostbit.Tweaker.Core.Commands;
using Ghostbit.Tweaker.Core.Events;
using NLog;
using strange.extensions.dispatcher.eventdispatcher.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core.Commands
{
    public class ListenForRemoteClientsCmd : TweakerCmd
    {
        public ListenForRemoteClientsCmd() : base(LogManager.GetCurrentClassLogger()) { }

        protected override void DoExecute()
        {
            logger.Info("Starting remote client listener.");
            Retain();
            dispatcher.AddListener(CoreEvent.STOP_REMOTE_CLIENT_LISTENER, OnStopListening);

            // TODO: listen for connections on thread. RemoteClient:IClient will handle communication through the socket.
        }

        private void OnStopListening(IEvent payload)
        {
            logger.Info("Stopping remote client listener.");
            // TODO: kill listener thread
            Release();
        }
    }
}