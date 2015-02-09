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
    {
        public object Instance { get; private set; }
        public uint UniqueId { get; private set; }
        public Type Type { get { return typeof(T); } }

        private static uint nextId = 1;

        public BoundInstance(object instance)
        {
            Instance = instance;
            UniqueId = nextId;
            nextId++;
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
