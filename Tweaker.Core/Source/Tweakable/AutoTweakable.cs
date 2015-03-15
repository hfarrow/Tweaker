using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
	/// <summary>
	/// A generic wrapper type that automatically registers and unregisters a tweakable
	/// when the objects are bound and finalized.
	/// </summary>
	/// <remarks>
	/// To bind the auto tweakable, the parent class must call AutoTweakable.Bind(this). 
	/// Calling Bind will use a IScanner + AutoTweakableProcessor to register all the 
	/// AutoTweakble feilds in the object class passed to Bind. When an AutoTweakable is
	/// finalized by the garbage collector, it will unregister the tweakable from the
	/// manager. All AutoTweakables use the same manager set statically via AutoTweakable.Manager.
	/// 
	/// Use Tweakble.value to get or set the value directly. Use Tweakable.SetTweakableValue(T) to
	/// set the value through the ITweakable instance which will enforce constraints and return
	/// the final value.
	/// </remarks>
	public class AutoTweakable : IDisposable
	{
		public static ITweakableManager Manager { get; set; }
		private static AutoTweakableProcessor s_processor;

		public ITweakable tweakable;
		public uint UniqueId { get; private set; }

		static AutoTweakable()
		{
			s_processor = new AutoTweakableProcessor();
		}

		~AutoTweakable()
		{
			if (tweakable != null)
			{
				Dispose();
			}
		}

		public static void Bind<TContainer>(TContainer container)
		{
			if (CheckForManager())
			{
				IScanner scanner = new Scanner();
				scanner.AddProcessor(s_processor);
				var provider = scanner.GetResultProvider<AutoTweakableResult>();
				provider.ResultProvided += OnResultProvided;
				scanner.ScanInstance(container);
				provider.ResultProvided -= OnResultProvided;
			}
		}

		private static void OnResultProvided(object sender, ScanResultArgs<AutoTweakableResult> e)
		{
			if (CheckForManager())
			{
				ITweakable tweakable = e.result.tweakble;
				Manager.RegisterTweakable(tweakable);
				e.result.autoTweakable.tweakable = tweakable;
				e.result.autoTweakable.UniqueId = e.result.uniqueId;
			}
		}

		public void Dispose()
		{
			if (tweakable.Manager != null && CheckValidTweakable())
			{
				if (tweakable != null)
				{
					Manager.UnregisterTweakable(tweakable);
				}
			}
			tweakable = null;
		}

		protected bool CheckValidTweakable()
		{
			if (tweakable == null)
			{
				throw new AutoTweakableException("AutoTweakable has been disposed and can no longer be used.");
			}

			return true;
		}

		protected static bool CheckForManager()
		{
			if (Manager == null)
			{
				throw new AutoTweakableException("No manager has been set. Set a manager through AutoTweakableBase.Manager before creating auto tweakable instance.");
			}
			return true;
		}
	}

	public class Tweakable<T> : AutoTweakable
	{
		public T value;

		/// <summary>
		/// Set the value through an ITweakable instance. This will
		/// enforce any constraints such as a Range. To ignore constaints,
		/// set the 'value' field directly.
		/// </summary>
		/// <remarks>
		/// Please note that the value returned may not be the same as the value passed in
		/// if the input value is invalid for the given tweakable constraints such as a 
		/// Range contraint.
		/// </remarks>
		public T SetTweakableValue(T value)
		{
			if (CheckValidTweakable())
			{
				tweakable.SetValue(value);
			}
			return (T)tweakable.GetValue();
		}

		public Tweakable(T value)
		{
			this.value = value;
		}

		public Tweakable() :
			this(default(T))
		{

		}
	}

	public class AutoTweakableException : Exception, ISerializable
	{
		public AutoTweakableException(string message)
			: base(message)
		{
		}
	}
}
