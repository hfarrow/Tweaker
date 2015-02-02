using System.Collections.Generic;
using System.Reflection;
using System;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
    public class TweakableManager :
        ITweakableManager
    {
        private BaseTweakerManager<ITweakable> baseManager;

        public TweakableManager(IScanner scanner)
        {
            baseManager = new BaseTweakerManager<ITweakable>(scanner);
            if (scanner != null)
            {
                scanner.AddProcessor(new TweakableProcessor());
            }
        }

        public ITweakable RegisterTweakable<T>(TweakableInfo<T> info, PropertyInfo tweakable, object instance = null)
        {
            return TweakableFactory.MakeTweakableFromInfo(info, tweakable, instance);
        }

        public ITweakable RegisterTweakable<T>(TweakableInfo<T> info, FieldInfo tweakable, object instance = null)
        {
            return TweakableFactory.MakeTweakableFromInfo(info, tweakable, instance);
        }

        public void RegisterTweakable(ITweakable tweakable)
        {
            baseManager.RegisterObject(tweakable);
        }

        public void UnregisterTweakable(ITweakable tweakable)
        {
            baseManager.UnregisterObject(tweakable);
        }

        public void UnregisterTweakable(string name)
        {
            baseManager.UnregisterObject(name);
        }

        public TweakerDictionary<ITweakable> GetTweakables(SearchOptions options)
        {
            return baseManager.GetObjects(options);
        }

        public ITweakable GetTweakable(SearchOptions options)
        {
            return baseManager.GetObject(options);
        }

        public ITweakable GetTweakable(string name)
        {
            return baseManager.GetObject(name);
        }

        public void SetTweakableValue<T>(ITweakable tweakable, T value)
        {
            tweakable.SetValue(value);
        }

        public void SetTweakableValue<T>(string name, T value)
        {
            SetTweakableValue(baseManager.GetObject(name), value);
        }

        public T GetTweakableValue<T>(ITweakable tweakable)
        {
            return (T)tweakable.GetValue();
        }

        public T GetTweakableValue<T>(string name)
        {
            return GetTweakableValue<T>(baseManager.GetObject(name));
        }
    }
}