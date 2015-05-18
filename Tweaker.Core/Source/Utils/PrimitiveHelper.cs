using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaker.Core
{
	/// <summary>
	/// Help find the correct operators for types that have built in operators that
	/// cannot be found through reflection. Also helps because generic type constraints
	/// do not include constraints for specific operators such as Addition and Subtraction.
	/// </summary>
	/// <remarks>
	/// If targeting a newer .NET version, use of dynamic could replace this class.
	/// </remarks>
	public static class PrimitiveHelper
	{
		public static object Add(object a, object b)
		{
			Type type = a.GetType();
			if (type != b.GetType() || !type.IsPrimitive)
			{
				return null;
			}

			// Could make this quicker by creating map of Type to Action<a, b>?
			if (type == typeof(Int16))
			{
				return (Int16)a + (Int16)b;
			}
			else if (type == typeof(UInt16))
			{
				return (UInt16)a + (UInt16)b;
			}
			else if (type == typeof(Int32))
			{
				return (Int32)a + (Int32)b;
			}
			else if (type == typeof(UInt32))
			{
				return (UInt32)a + (UInt32)b;
			}
			else if (type == typeof(Int64))
			{
				return (Int64)a + (Int64)b;
			}
			else if (type == typeof(UInt64))
			{
				return (UInt64)a + (UInt64)b;
			}
			else if (type == typeof(Single))
			{
				return (Single)a + (Single)b;
			}
			else if (type == typeof(Double))
			{
				return (Double)a + (Double)b;
			}
			else if (type == typeof(Byte))
			{
				return (Byte)a + (Byte)b;
			}
			else if (type == typeof(SByte))
			{
				return (SByte)a + (SByte)b;
			}
			else if (type == typeof(Char))
			{
				return (Char)a + (Char)b;
			}
			else
			{
				return null;
			}
		}

		public static object Subtract(object a, object b)
		{
			Type type = a.GetType();
			if (type != b.GetType() || !type.IsPrimitive)
			{
				return null;
			}

			if (type == typeof(Int16))
			{
				return (Int16)a - (Int16)b;
			}
			else if (type == typeof(UInt16))
			{
				return (UInt16)a - (UInt16)b;
			}
			else if (type == typeof(Int32))
			{
				return (Int32)a - (Int32)b;
			}
			else if (type == typeof(UInt32))
			{
				return (UInt32)a - (UInt32)b;
			}
			else if (type == typeof(Int64))
			{
				return (Int64)a - (Int64)b;
			}
			else if (type == typeof(UInt64))
			{
				return (UInt64)a - (UInt64)b;
			}
			else if (type == typeof(Single))
			{
				return (Single)a - (Single)b;
			}
			else if (type == typeof(Double))
			{
				return (Double)a - (Double)b;
			}
			else if (type == typeof(Byte))
			{
				return (Byte)a - (Byte)b;
			}
			else if (type == typeof(SByte))
			{
				return (SByte)a - (SByte)b;
			}
			else if (type == typeof(Char))
			{
				return (Char)a - (Char)b;
			}
			else
			{
				return null;
			}
		}
	}
}
