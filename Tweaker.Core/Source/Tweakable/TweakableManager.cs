using System.Collections.Generic;
using System.Reflection;
using System;
using Tweaker.AssemblyScanner;

namespace Tweaker.Core
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

		public ITweakable RegisterTweakable<T>(TweakableInfo<T> info, PropertyInfo propertyInfo, object instance = null)
		{
			ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo(info, propertyInfo, instance);
			RegisterTweakable(tweakable);
			return tweakable;
		}

		public ITweakable RegisterTweakable<T>(TweakableInfo<T> info, FieldInfo fieldInfo, object instance = null)
		{
			ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo(info, fieldInfo, instance);
			RegisterTweakable(tweakable);
			return tweakable;
		}

		public void RegisterTweakable(ITweakable tweakable)
		{
			// Transfer ownership from previous manager to this manager.
			if (tweakable.Manager != null && tweakable.Manager != this)
			{
				tweakable.Manager.UnregisterTweakable(tweakable);
			}
			tweakable.Manager = this;

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

		public TweakerDictionary<ITweakable> GetTweakables(SearchOptions options = null)
		{
			return baseManager.GetObjects(options);
		}

		public ITweakable GetTweakable(SearchOptions options = null)
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