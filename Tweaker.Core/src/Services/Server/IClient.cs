using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public interface IClient
    {
        string Name { get; }
        void OnDisconnected();
        void OnConnected();
    }
}
