using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ghostbit.Tweaker.Core
{
	public abstract class BaseInvokable : TweakerObject, IInvokable
	{
		public InvokableInfo InvokableInfo { get; private set; }
		public IInvokableManager Manager { get; set; }

		public abstract string MethodSignature { get; }

		private Type[] parameterTypes;
		public Type[] ParameterTypes
		{
			get
			{
				Type[] clone;
				if (parameterTypes != null)
				{
					clone = parameterTypes.Clone() as Type[];
				}
				else
				{
					clone = new Type[0];
				}
				return clone;
			}
		}

		public ParameterInfo[] Parameters { get; private set; }

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
			Parameters = parameters;
			parameterTypes = new Type[Parameters.Length];
			for (var i = 0; i < parameterTypes.Length; ++i)
			{
				parameterTypes[i] = Parameters[i].ParameterType;
			}
		}

		public object Invoke(params object[] args)
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
			if (parameterTypes == null)
			{
				// SetParameters was never called. Skip validation.
				return;
			}

			if (args == null)
			{
				args = new object[0];
			}

			if (args.Length != parameterTypes.Length)
			{
				throw new InvokeArgNumberException(Name, args, parameterTypes);
			}

			Type[] providedArgTypes = new Type[args.Length];
			for (var i = 0; i < args.Length; ++i)
			{
				if (args[i] != null)
				{
					providedArgTypes[i] = args[i].GetType();
				}
			}

			for (var i = 0; i < args.Length; ++i)
			{
				if (args[i] != null && !parameterTypes[i].IsAssignableFrom(providedArgTypes[i]))
				{
					throw new InvokeArgTypeException(Name, args, providedArgTypes, parameterTypes,
						"Target arg is not assignable from the provided arg.");
				}
				else if(args[i] == null)
				{
					if (parameterTypes[i].IsValueType)
					{
						args[i] = Activator.CreateInstance(parameterTypes[i]);
						if (args[i] == null)
						{
							throw new InvokeArgTypeException(Name, args, providedArgTypes, parameterTypes,
								string.Format("Could not construct an instance of value type parameter {0}.", parameterTypes[i].FullName));
						}
					}
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