using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	public class InvokableEvent : BaseInvokable
	{
		private readonly FieldInfo fieldInfo;

		private string methodSignature;
		public override string MethodSignature
		{
			get { return methodSignature; }
		}

		public FieldInfo FieldInfo
		{
			get { return fieldInfo; }
		}

		public InvokableEvent(InvokableInfo info, EventInfo eventInfo, FieldInfo fieldInfo, WeakReference instance)
			: base(info, fieldInfo.ReflectedType.Assembly, instance, fieldInfo.IsPublic)
		{
			this.fieldInfo = fieldInfo;
			methodSignature = "[Unknown]";

			var invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
			SetParameters(invokeMethod.GetParameters());
			methodSignature = invokeMethod.GetSignature();
		}

		private MulticastDelegate GetEventDelegate()
		{
			MulticastDelegate eventDelegate = null;
			var value = fieldInfo.GetValue(StrongInstance);
			// value will be null if no listeners added.
			if (value != null)
			{
				eventDelegate = (MulticastDelegate)value;
				if (eventDelegate == null)
				{
					throw new Exception("Could not retrieve the event delegate for invokable '" + Name + "'.");
				}
			}
			return eventDelegate;
		}

		protected override object DoInvoke(object[] args)
		{
			object ret = default(object);
			MulticastDelegate eventDelegate = GetEventDelegate();
			if (eventDelegate != null)
			{
				foreach (var handler in eventDelegate.GetInvocationList())
				{
					ret = handler.Method.Invoke(handler.Target, args);
				}
			}
			return ret;
		}
	}
}
