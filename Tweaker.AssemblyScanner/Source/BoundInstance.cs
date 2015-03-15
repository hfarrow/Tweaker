using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.AssemblyScanner
{
	/// <summary>
	/// Binds an object instance to a unique identifier that can be used for displaying to the user.
	/// </summary>
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
				if (weakReference.IsAlive)
				{
					instance = weakReference.Target as T;
				}
				return instance;
			}
		}
		public uint UniqueId { get { return uniqueId; } }
		public Type Type { get { return typeof(T); } }

		private readonly uint uniqueId;
		private readonly WeakReference weakReference;

		private static uint s_nextId = 1;

		public BoundInstance(T instance) :
			this(instance, s_nextId)
		{
			s_nextId++;
		}

		public BoundInstance(T instance, uint id)
		{
			weakReference = new WeakReference(instance);
			uniqueId = id;
		}

		//public static void ResetNextId()
		//{
		//    s_nextId = 1;
		//}
	}

	public class BoundInstanceFactory
	{
		public static IBoundInstance Create(object instance)
		{
			Type genericType = typeof(BoundInstance<>).MakeGenericType(instance.GetType());
			return (IBoundInstance)Activator.CreateInstance(genericType, instance);
		}

		public static IBoundInstance Create(object instance, uint id)
		{
			Type genericType = typeof(BoundInstance<>).MakeGenericType(instance.GetType());
			return (IBoundInstance)Activator.CreateInstance(genericType, instance, id);
		}
	}
}
