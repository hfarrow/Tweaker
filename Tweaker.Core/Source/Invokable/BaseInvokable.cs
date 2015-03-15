using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ghostbit.Tweaker.Core
{
	public abstract class BaseInvokable : TweakerObject, IInvokable
	{
		public InvokableInfo InvokableInfo { get; private set; }

		public IInvokableManager Manager { get; set; }

		private Type[] argTypes;
		public Type[] ArgTypes
		{
			get
			{
				return argTypes.Clone() as Type[];
			}
		}

		public BaseInvokable(InvokableInfo info, Assembly assembly, WeakReference instance, bool isPublic, ParameterInfo[] parameters) :
			this(info, assembly, instance, isPublic)
		{
			SetParameters(parameters);
		}

		public BaseInvokable(InvokableInfo info, Assembly assembly, WeakReference instance, bool isPublic) :
			base(info, assembly, instance, isPublic)
		{
			InvokableInfo = info;
		}

		protected void SetParameters(ParameterInfo[] parameters)
		{
			this.argTypes = new Type[parameters.Length];
			for (var i = 0; i < argTypes.Length; ++i)
			{
				argTypes[i] = parameters[i].ParameterType;
			}
		}

		public object Invoke(object[] args = null)
		{
			if (CheckInstanceIsValid())
			{
				CheckArgsAreValid(args);
				try
				{
					return DoInvoke(args);
				}
				catch (Exception e)
				{
					throw new InvokeException(Name, args, e);
				}
			}
			else
			{
				return null;
			}
		}

		protected abstract object DoInvoke(object[] args);

		private void CheckArgsAreValid(object[] args)
		{
			if (args == null)
			{
				args = new object[0];
			}

			if (args.Length != argTypes.Length)
			{
				throw new InvokeArgNumberException(Name, args, argTypes);
			}

			Type[] providedArgTypes = new Type[args.Length];
			for (var i = 0; i < args.Length; ++i)
			{
				providedArgTypes[i] = args[i].GetType();
			}

			for (var i = 0; i < args.Length; ++i)
			{
				if (!argTypes[i].IsAssignableFrom(providedArgTypes[i]))
				{
					throw new InvokeArgTypeException(Name, args, providedArgTypes, argTypes);
				}
			}
		}

		protected override bool CheckInstanceIsValid()
		{
			if (!base.CheckInstanceIsValid())
			{
				Manager.UnregisterInvokable(this);
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}