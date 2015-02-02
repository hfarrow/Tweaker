using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = false)]
    public class Invokable : BaseTweakerAttribute, ITweakerAttribute
    {
        public Invokable(string name) :
            base(name)
        {

        }
    }
}
