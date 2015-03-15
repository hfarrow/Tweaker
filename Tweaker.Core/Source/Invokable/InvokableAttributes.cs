using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = false)]
	public class InvokableAttribute : BaseTweakerAttribute
	{
		public InvokableAttribute(string name) :
			base(name)
		{

		}
	}

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class ArgDescriptionAttribute : Attribute
	{
		public string Description { get; private set; }
		public ArgDescriptionAttribute(string description)
		{
			Description = description;
		}
	}

	[AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false)]
	public class ReturnDescriptionAttribute : Attribute
	{
		public string Description { get; private set; }
		public ReturnDescriptionAttribute(string description)
		{
			Description = description;
		}
	}
}
