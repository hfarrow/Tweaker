using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaker.Core
{
	// TODO: does .NET include this functionality somewhere?
	public class PrettyPrinter
	{
		public static string PrintObjectArray(object[] objects)
		{
			if (objects == null || objects.Length == 0)
			{
				return "";
			}

			StringBuilder str = new StringBuilder(objects[0] != null ? objects[0].ToString() : "");
			for (var i = 1; i < objects.Length; ++i)
			{
				str.Append(",");
				str.Append(objects[i] != null ? objects[i].ToString() : "");
			}
			return str.ToString();
		}

		public static string PrintTypeArray(Type[] types)
		{
			if (types == null || types.Length == 0)
			{
				return "";
			}

			StringBuilder str = new StringBuilder(types[0] != null ? types[0].FullName : "");
			for (var i = 1; i < types.Length; ++i)
			{
				str.Append(",");
				str.Append(types[i] != null ? types[i].FullName : "");
			}
			return str.ToString();
		}
	}
}
