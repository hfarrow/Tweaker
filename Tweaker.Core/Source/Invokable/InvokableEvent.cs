using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
	public class InvokableEvent : BaseInvokable
	{
		private readonly FieldInfo fieldInfo;
		private readonly MulticastDelegate eventDelegate;

		public FieldInfo FieldInfo
		{
			get { return fieldInfo; }
		}

		public InvokableEvent(InvokableInfo info, FieldInfo fieldInfo, WeakReference<object> instance)
			: base(info, fieldInfo.ReflectedType.Assembly, instance, fieldInfo.IsPublic)
		{
			this.fieldInfo = fieldInfo;

			var value = fieldInfo.GetValue(StrongInstance);
			// value will be null if no listeners added.
			if (value != null)
			{
				eventDelegate = (MulticastDelegate)value;
				if (eventDelegate == null)
				{
					throw new Exception("Could not bind invokable '" + Name + "' to it's event Delegate.");
				}
				SetParameters(eventDelegate.Method.GetParameters());
			}
		}

		protected override object DoInvoke(object[] args)
		{
			object ret = default(object);
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
