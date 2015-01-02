using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class Server : IServer
    {
        public bool IsStarted { get; private set; }

        public Server()
        {
            IsStarted = false;
        }

        public void Start()
        {
            if (IsStarted)
                return;

            IsStarted = true;
        }

        //private IInvokableManager invokables;
        //private ITweakableManager tweakables;
        //private IWatchableManager watchables;

        //private List<IClient> clients;

        //public Server(IInvokableManager invokables, ITweakableManager tweakables, IWatchableManager watchables)
        //{
        //    this.invokables = invokables;
        //    this.tweakables = tweakables;
        //    this.watchables = watchables;

        //    clients = new List<IClient>();
        //}

        //private void AddClient(IClient client)
        //{
        //    if (clients.Contains(client))
        //        return;

        //    clients.Add(client);
        //    client.OnClosed += OnClientClosed;
        //}

        //private void OnClientClosed(object sender, EventArgs e)
        //{
        //    IClient client = sender as IClient;
        //    client.OnClosed -= OnClientClosed;
        //    clients.Remove(client);
        //}
    }
}
