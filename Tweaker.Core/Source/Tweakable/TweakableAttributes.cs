using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaker.Core
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public class TweakableAttribute : BaseTweakerAttribute
	{
		public TweakableAttribute(string name) :
			base(name)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public class TweakerRangeAttribute : Attribute
	{
		public object MinValue;
		public object MaxValue;

		public TweakerRangeAttribute(object minValue, object maxValue)
		{
			MinValue = minValue;
			MaxValue = maxValue;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public class StepSizeAttribute : Attribute
	{
		public object Size;

		public StepSizeAttribute(object size)
		{
			Size = size;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
	public class NamedToggleValueAttribute : Attribute
	{
		public string Name;
		public object Value;
		public uint Order;

		public NamedToggleValueAttribute(string name, object value, uint order = 0)
		{
			Name = name;
			Value = value;
			Order = order;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
	public class ToggleValueAttribute : NamedToggleValueAttribute
	{
		public ToggleValueAttribute(object value, uint order = 0) :
			base(value.ToString(), value, order)
		{
		}
	}
}
