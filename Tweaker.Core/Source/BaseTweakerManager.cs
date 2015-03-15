using System.Collections.Generic;
using System;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
	/// <summary>
	/// Base class for tweaker managers that wraps a thread safe collection of ITweakerObject.
	/// </summary>
	/// <typeparam name="T">The tweaker object type that implements ITweakerObject.</typeparam>
	public class BaseTweakerManager<T>
		where T : ITweakerObject
	{
		private TweakerDictionary<T> objects;
		private IScanner scanner;
		private object lockObj = new object();

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
			lock (lockObj)
			{
				if (objects.ContainsKey(t.Name))
				{
					throw new NameAlreadyRegisteredException(t.Name);
				}

				if (t.StrongInstance != null)
				{
					T existingObj = GetObject(new SearchOptions(t.Name.Split('#')[0],
						null, SearchOptions.ScopeType.All, SearchOptions.BindingType.Instance, t.StrongInstance));

					if (existingObj != null)
					{
						throw new InstanceAlreadyRegisteredException(existingObj);
					}
				}

				objects.Add(t.Name, t);
			}
		}

		public void UnregisterObject(T t)
		{
			UnregisterObject(t.Name);
		}

		public void UnregisterObject(string name)
		{
			lock (lockObj)
			{
				if (!objects.ContainsKey(name))
				{
					throw new NotFoundException(name);
				}

				objects.Remove(name);
			}
		}

		public TweakerDictionary<T> GetObjects(SearchOptions options = null)
		{
			PruneDeadInstances();
			TweakerDictionary<T> filteredObjects = new TweakerDictionary<T>();
			lock (lockObj)
			{
				foreach (var obj in objects.Values)
				{
					if (options == null || options.CheckMatch(obj))
					{
						filteredObjects.Add(obj.Name, obj);
					}
				}
			}
			return filteredObjects;
		}

		public T GetObject(SearchOptions options)
		{
			lock (lockObj)
			{
				foreach (var obj in objects.Values)
				{
					if (options != null && options.CheckMatch(obj))
					{
						return ValidateObjectToReturn(obj);
					}
				}
				return default(T);
			}
		}

		public T GetObject(string name)
		{
			lock (lockObj)
			{
				T obj = default(T);
				objects.TryGetValue(name, out obj);
				return ValidateObjectToReturn(obj);
			}
		}

		public void PruneDeadInstances()
		{
			lock (lockObj)
			{
				List<string> keysToRemove = new List<string>();
				foreach (string name in objects.Keys)
				{
					T obj = objects[name];
					if (!obj.IsValid)
					{
						keysToRemove.Add(name);
					}
				}
				foreach (string name in keysToRemove)
				{
					objects.Remove(name);
				}
			}
		}

		// If the instance that a tweakable is bound to is garbage collected
		// it will no longer be valid and should be unregistered.
		private T ValidateObjectToReturn(T obj)
		{
			if(obj != null && !obj.IsValid)
			{
				UnregisterObject(obj);
				return default(T);
			}
			else
			{
				return obj;
			}
		}
	}
}