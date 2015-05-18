using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	public class VirtualProperty<T>
	{
		public WeakReference WeakInstance { get { return weakReference; } }
		public Action<T> Setter { get { return setter; } }
		public Func<T> Getter { get { return getter; } }

		private readonly FieldInfo fieldInfo;
		private readonly WeakReference weakReference;
		private readonly Action<T> setter;
		private readonly Func<T> getter;

		public object StrongInstance
		{
			get
			{
				if (weakReference == null)
				{
					return null;
				}

				object strongRef = null;
				WeakInstance.TryGetTarget(out strongRef);
				return strongRef;
			}
		}

		public bool IsValid
		{
			get
			{
				return WeakInstance == null || StrongInstance != null;
			}
		}

		public VirtualProperty(FieldInfo field, WeakReference instance)
		{
			fieldInfo = field;
			weakReference = instance;
			setter = SetValue;
			getter = GetValue;
		}

		private void SetValue(T value)
		{
			fieldInfo.SetValue(StrongInstance, value);
		}

		private T GetValue()
		{
			return (T)fieldInfo.GetValue(StrongInstance);
		}
	}
}
