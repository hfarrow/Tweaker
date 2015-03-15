using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
	public static class WeakReferenceExtensions
	{
		/// <summary>
		/// Add TryGetTraget to non generic WeakReference. This makes swapping WeakReference with WeakReference<> easier.
		/// </summary>
		/// <param name="weak"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool TryGetTarget(this WeakReference weak, out object target)
		{
			if (weak.IsAlive)
			{
				target = weak.Target;
				return true;
			}
			else
			{
				target = null;
				return false;
			}
		}
	}
}
