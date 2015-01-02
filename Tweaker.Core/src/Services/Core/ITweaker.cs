using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;

namespace Ghostbit.Tweaker.Core
{
    public class TweakerDictionary<T> : Dictionary<string, T> {}

    public interface IInvokableManager
    {
        IInvokable RegisterInvokable(InvokableInfo info, Delegate del);
        //IInvokable RegisterInvokable(InvokableInfo info, MethodInfo invokable, instance);
        void RegisterInvokable(IInvokable invokable);
        void UnregisterInvokable(IInvokable invokable);
        void UnregisterInvokable(string name);
        TweakerDictionary<IInvokable> GetInvokables(SearchOptions options);
        IInvokable GetInvokable(SearchOptions options);
        IInvokable GetInvokable(string name);
        object Invoke(IInvokable invokable, object[] args);
        object Invoke(string name, object[] args);
    }

    public interface ITweakableManager
    {
        ITweakable RegisterTweakable<T>(TweakableInfo<T> info, PropertyInfo tweakable, object instance);
        ITweakable RegisterTweakable<T>(TweakableInfo<T> info, FieldInfo tweakable, object instance);
        void RegisterTweakable(ITweakable tweakable);
        void UnregisterTweakable(ITweakable tweakable);
        void UnregisterTweakable(string name);
        TweakerDictionary<ITweakable> GetTweakables(SearchOptions options);
        ITweakable GetTweakable(SearchOptions options);
        ITweakable GetTweakable(string name);
        void SetTweakableValue<T>(ITweakable tweakable, T value);
        void SetTweakableValue<T>(string name, T value);
        T GetTweakableValue<T>(ITweakable tweakable);
        T GetTweakableValue<T>(string name);
    }

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

    public class NameAlreadyUsedException : Exception, ISerializable
    {
        public NameAlreadyUsedException(string name)
            : base("The name '" + name + "' is already in use.")
        {
        }
    }

    public class NotFoundException : Exception, ISerializable
    {
        public NotFoundException(string name)
            : base("The name '" + name + "' is not currently in use.")
        {
        }
    }

    public class InvokeException : Exception, ISerializable
    {
        public InvokeException(string name, object[] args, Exception inner)
            : base("Invocation of '" + name + "(" + args.ToString() + ")' failed. See inner exception.", inner)
        {
        }
    }

    public class TweakableSetException : Exception, ISerializable
    {
        public TweakableSetException(string name, object value, Exception inner)
            : base("Failed to set tweakable '" + name + "' to value '" + value.ToString() + "'. See inner exception.", inner)
        {
        }

        public TweakableSetException(string name, string message)
            : base("Failed to set tweakable '" + name + "'. " + message)
        {
        }
    }

    public class TweakableGetException : Exception, ISerializable
    {
        public TweakableGetException(string name, Exception inner)
            : base("Failed to get tweakable '" + name + "'. See inner exception.", inner)
        {
        }

        public TweakableGetException(string name, string message)
            : base("Failed to get tweakable '" + name + "'. " + message)
        {
        }
    }
}