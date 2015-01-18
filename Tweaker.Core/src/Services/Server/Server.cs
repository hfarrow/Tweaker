using Ghostbit.Tweaker.Core.Events;
using NLog;
using strange.extensions.context.api;
using strange.extensions.dispatcher.eventdispatcher.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class Server : IServer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Inject(ContextKeys.CONTEXT_DISPATCHER)]
        public IEventDispatcher Dispatcher { get; set; }

        [Inject]
        public TweakerOptions Options { get; set; }

        [Inject]
        public IInvokableManager Invokables { get; set; }

        [Inject]
        public ITweakableManager Tweakables { get; set; }

        [Inject]
        public IWatchableManager Watchables { get; set; }

        public bool IsStarted { get; private set; }
        private List<IClient> clients;

        public Server()
        {
            logger.Debug("Constructed");
            IsStarted = false;
            clients = new List<IClient>();
        }

        public void Start()
        {
            if (IsStarted)
                return;

            IsStarted = true;
            AddListeners();
            logger.Debug("Started");

            if(AllowRemoteClients)
            {
                Dispatcher.Dispatch(CoreEvent.START_REMOTE_CLIENT_LISTENER);
            }
        }

        public void Shutdown()
        {
            if (!IsStarted)
                return;

            IsStarted = false;
            RemoveListeners();
            foreach(IClient client in clients)
            {
                client.OnDisconnected();
                Dispatcher.Dispatch(CoreEvent.CLIENT_DISCONNECTED, client);
            }
            clients.Clear();
            
            logger.Debug("Shutdown");
        }

        private bool AllowRemoteClients
        { 
            get
            { 
                return (Options.Flags & TweakerOptionFlags.NoServer) == 0;
            }
        }

        private void AddListeners()
        {
            Dispatcher.AddListener(CoreEvent.CONNECT_CLIENT, OnConnectClient);
            Dispatcher.AddListener(CoreEvent.DICONNECT_CLIENT, OnDisconnectClient);
        }

        private void RemoveListeners()
        {
            Dispatcher.RemoveListener(CoreEvent.CONNECT_CLIENT, OnConnectClient);
            Dispatcher.RemoveListener(CoreEvent.DICONNECT_CLIENT, OnDisconnectClient);
        }

        private void OnConnectClient(IEvent payload)
        {
            try
            {
                IClient client = (IClient)payload.data;
                if (!clients.Contains(client))
                {
                    logger.Info("Connecting client named '{0}'.", client.Name);
                    clients.Add(client);
                    client.OnConnected();
                    Dispatcher.Dispatch(CoreEvent.CLIENT_CONNECTED, client);
                }
                else
                {
                    logger.Warn("Client named '{0}' has already been connected.");
                }
            }
            catch(Exception ex)
            {
                logger.Error("Failed to connect client.", ex);
            }
        }

        private void OnDisconnectClient(IEvent payload)
        {
            try
            {
                IClient client = (IClient)payload.data;
                if (clients.Contains(client))
                {
                    logger.Info("Disconnecting client named '{0}'.", client.Name);
                    clients.Remove(client);
                    client.OnDisconnected();
                    Dispatcher.Dispatch(CoreEvent.CLIENT_DISCONNECTED, client);
                }
                else
                {
                    logger.Warn("Client named '{0}' was not previously connected.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to disconnect client.", ex);
            }
        }
    }
}
