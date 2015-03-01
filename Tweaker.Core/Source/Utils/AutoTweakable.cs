using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
    public class AutoTweakableBase
    {
        public static ITweakableManager Manager { get; set; }
    }

    public class AutoTweakable : AutoTweakableBase
    {
    }
}
