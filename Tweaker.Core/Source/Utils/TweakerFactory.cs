using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Tweaker.AssemblyScanner;

namespace Tweaker.Core
{
	/// <summary>
	/// An example factory for creating object instances and automatically scanning them.
	/// The resutling tweaker objects will not be automatically unregistered when the
	/// instances created by the factory are finalized.
	/// </summary>
	public class TweakerFactory : ITweakerFactory
	{
		private IScanner scanner;

		public TweakerFactory(IScanner scanner)
		{
			this.scanner = scanner;
		}

		public T Create<T>(params object[] constructorArgs)
		{
			Type type = typeof(T);
			Type[] argTypes = new Type[constructorArgs.Length];
			for (int i = 0; i < constructorArgs.Length; ++i)
			{
				argTypes[i] = constructorArgs[i].GetType();
			}

			ConstructorInfo constructor = type.GetConstructor(argTypes);
			if (constructor == null)
			{
				throw new Exception("Could not find constructor that matches input arguments.");
			}

			T instance = (T)Activator.CreateInstance(type, constructorArgs);
			if (instance != null)
			{
				scanner.ScanInstance(instance);
			}
			return instance;
		}
	}
}
