using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	public class StepTweakable<T> : IStepTweakable
	{
		private readonly BaseTweakable<T> baseTweakable;
		public BaseTweakable<T> BaseTweakable { get { return baseTweakable; } }

		private readonly MethodInfo addMethod;
		private readonly MethodInfo subtractMethod;

		public StepTweakable(BaseTweakable<T> baseTweakable)
		{
			this.baseTweakable = baseTweakable;

			if (TweakableType.IsPrimitive)
			{
				addMethod = typeof(PrimitiveHelper).GetMethod("Add", BindingFlags.Static | BindingFlags.Public);
				subtractMethod = typeof(PrimitiveHelper).GetMethod("Subtract", BindingFlags.Static | BindingFlags.Public);
			}
			else
			{
				addMethod = TweakableType.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public);
				subtractMethod = TweakableType.GetMethod("op_Subtraction", BindingFlags.Static | BindingFlags.Public);
			}


			if (addMethod == null)
			{
				throw new StepTweakableInvalidException(baseTweakable.Name, "No 'operator +' could be found on type '" + TweakableType.FullName + "'");
			}
			else if (subtractMethod == null)
			{
				throw new StepTweakableInvalidException(baseTweakable.Name, "No 'operator -' could be found on type '" + TweakableType.FullName + "'");
			}
		}

		public object StepSize
		{
			get { return baseTweakable.TweakableInfo.StepSize.Size; }
		}

		public object StepNext()
		{
			T newValue = (T)addMethod.Invoke(null, new object[] { (T)baseTweakable.GetValue(), StepSize });
			baseTweakable.SetValue(newValue);
			return baseTweakable.GetValue();
		}

		public object StepPrevious()
		{
			T newValue = (T)subtractMethod.Invoke(null, new object[] { (T)baseTweakable.GetValue(), StepSize });
			baseTweakable.SetValue(newValue);
			return baseTweakable.GetValue();
		}

		public Type TweakableType { get { return typeof(T); } }
	}
}
