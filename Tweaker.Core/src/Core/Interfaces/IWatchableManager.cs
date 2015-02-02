using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public interface IWatchableManager
    {
        IWatchable RegisterWatchable(WatchableInfo info, PropertyInfo watchable);
        //IWatchable RegisterWatchable(WatchableInfo info, FieldInfo watchable);
        void RegisterTweakable(IWatchable watchable);
        void UnregisterWatchable(IWatchable watchable);
        void UnregisterWatchable(string name);
        TweakerDictionary<IWatchable> GetWatchables(SearchOptions options);
        IWatchable GetWatchable(SearchOptions options);
        IWatchable GetWatchable(string name);
    }
}
