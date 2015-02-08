using System.Collections.Generic;
using System;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
    public class BaseTweakerManager<T>
        where T : ITweakerObject
    {
        private TweakerDictionary<T> objects;
        private IScanner scanner;

        public BaseTweakerManager(IScanner scanner)
        {
            objects = new TweakerDictionary<T>();

            this.scanner = scanner;
            if (this.scanner != null)
            {
                this.scanner.GetResultProvider<T>().ResultProvided += OnObjectFound;
            }
        }

        private void OnObjectFound(object sender, ScanResultArgs<T> e)
        {
            RegisterObject(e.result);
        }

        public void RegisterObject(T t)
        {
            if (objects.ContainsKey(t.Name))
            {
                throw new NameAlreadyUsedException(t.Name);
            }
            objects.Add(t.Name, t);
        }

        public void UnregisterObject(T t)
        {
            UnregisterObject(t.Name);
        }

        public void UnregisterObject(string name)
        {
            if (!objects.ContainsKey(name))
            {
                throw new NotFoundException(name);
            }

            objects.Remove(name);
        }

        public TweakerDictionary<T> GetObjects(SearchOptions options = null)
        {
            TweakerDictionary<T> filteredObjects = new TweakerDictionary<T>();
            foreach (var obj in objects.Values)
            {
                var scope = obj.IsPublic ?
                    SearchOptions.ScopeType.Public : SearchOptions.ScopeType.NonPublic;
                if (options == null || options.CheckMatch(obj.Name, obj.Assembly, scope))
                {
                    filteredObjects.Add(obj.Name, obj);
                }
            }
            return filteredObjects;
        }

        public T GetObject(SearchOptions options)
        {
            foreach (var obj in objects.Values)
            {
                var scope = obj.IsPublic ?
                    SearchOptions.ScopeType.Public : SearchOptions.ScopeType.NonPublic;
                if (options != null && options.CheckMatch(obj.Name, obj.Assembly, scope))
                {
                    return obj;
                }
            }
            return default(T);
        }

        public T GetObject(string name)
        {
            T obj = default(T);
            objects.TryGetValue(name, out obj);
            return obj;
        }
    }
}