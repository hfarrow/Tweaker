using System.Collections.Generic;
using System;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
    public class WatchableManager : IWatchableManager
    {
        public WatchableManager()
        {

        }

        public WatchableManager(IScanner scanner)
        {

        }

        public IWatchable RegisterWatchable(WatchableInfo info, System.Reflection.PropertyInfo watchable)
        {
            throw new NotImplementedException();
        }

        public void RegisterTweakable(IWatchable watchable)
        {
            throw new NotImplementedException();
        }

        public void UnregisterWatchable(IWatchable watchable)
        {
            throw new NotImplementedException();
        }

        public void UnregisterWatchable(string name)
        {
            throw new NotImplementedException();
        }

        public TweakerDictionary<IWatchable> GetWatchables(SearchOptions options)
        {
            throw new NotImplementedException();
        }

        public IWatchable GetWatchable(SearchOptions options)
        {
            throw new NotImplementedException();
        }

        public IWatchable GetWatchable(string name)
        {
            throw new NotImplementedException();
        }
    }
}