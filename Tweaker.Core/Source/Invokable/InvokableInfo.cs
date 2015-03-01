using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
    public class InvokableInfo : TweakerObjectInfo
    {
        public InvokableInfo(string name, uint instanceId = 0)
            : base(name, instanceId)
        {
        }
    }
}
