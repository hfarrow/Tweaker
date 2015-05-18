using Tweaker.AssemblyScanner;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	/// <summary>
	/// This is an IScanner processor that processes types or members annotated with Invokable
	/// and produces an IInvokable as a result.
	/// </summary>
	/// <remarks>
	/// Tweaker does not directly enforce registered names to have a path or group separator
	/// but if a Type is annotated with Invokable a period will be used to separate the provided
	/// group name with the name of the invokable members.
	/// </remarks>
	public class InvokableProcessor : IAttributeScanProcessor<InvokableAttribute, IInvokable>
	{
		public void ProcessAttribute(InvokableAttribute input, Type type, IBoundInstance instance = null)
		{
			foreach (MemberInfo memberInfo in type.GetMembers(ReflectionUtil.GetBindingFlags(instance)))
			{
				if (memberInfo.MemberType == MemberTypes.Method ||
					memberInfo.MemberType == MemberTypes.Event)
				{
					if (memberInfo.GetCustomAttributes(typeof(InvokableAttribute), false).Length == 0)
					{
						InvokableAttribute inner = new InvokableAttribute(input.Name + "." + memberInfo.Name);
						ProcessAttribute(inner, memberInfo, instance);
					}
				}
			}
		}

		public void ProcessAttribute(InvokableAttribute input, MemberInfo memberInfo, IBoundInstance instance = null)
		{
			IInvokable invokable = null;
			if (memberInfo.MemberType == MemberTypes.Method)
			{
				var methodInfo = (MethodInfo)memberInfo;
				invokable = InvokableFactory.MakeInvokable(input, methodInfo, instance);
			}
			else if (memberInfo.MemberType == MemberTypes.Event)
			{
				var eventInfo = (EventInfo)memberInfo;
				invokable = InvokableFactory.MakeInvokable(input, eventInfo, instance);
			}
			else
			{
				throw new ProcessorException("InvokableProcessor cannot process non MethodInfo or EventInfo types");
			}

			if (invokable != null)
			{
				ProvideResult(invokable);
			}
		}

		public event EventHandler<ScanResultArgs<IInvokable>> ResultProvided;

		private void ProvideResult(IInvokable invokable)
		{
			if (ResultProvided != null)
				ResultProvided(this, new ScanResultArgs<IInvokable>(invokable));
		}
	}
}
