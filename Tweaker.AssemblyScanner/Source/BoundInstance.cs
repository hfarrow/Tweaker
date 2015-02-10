using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.AssemblyScanner
{
    public interface IBoundInstance
    {
        object Instance { get; }
        uint UniqueId { get; }
        Type Type { get; }
    }

    public class BoundInstance<T> : IBoundInstance
        where T : class
    {
        public object Instance 
        {
            get 
            {
                T instance = default(T);
                weakReference.TryGetTarget(out instance);
                return instance;
            }
        }
        public uint UniqueId { get { return uniqueId; } }
        public Type Type { get { return typeof(T); } }

        private readonly uint uniqueId;
        private readonly WeakReference<T> weakReference;

        private static uint s_nextId = 1;

        public BoundInstance(T instance)
        {
            weakReference = new WeakReference<T>(instance);
            uniqueId = s_nextId;
            s_nextId++;
        }
    }

    public class BoundInstanceFactory
    {
        public static IBoundInstance Create(object instance)
        {
            Type genericType = typeof(BoundInstance<>).MakeGenericType(instance.GetType());
            return (IBoundInstance)Activator.CreateInstance(genericType, instance);
        }

    }
}
