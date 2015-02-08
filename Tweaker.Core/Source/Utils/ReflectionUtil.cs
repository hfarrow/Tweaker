using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
    public static class ReflectionUtil
    {
        public static BindingFlags GetBindingFlags(object instance)
        {
            BindingFlags flags = BindingFlags.Public;
            if (instance == null)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }
            return flags;
        }
    }
}
