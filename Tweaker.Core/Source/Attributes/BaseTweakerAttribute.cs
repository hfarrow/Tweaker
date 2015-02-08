using System;
using System.Collections.Generic;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public abstract class BaseTweakerAttribute : Attribute, ITweakerAttribute
    {
        public string Name { get; private set; }
        public Guid Guid { get; private set; }

        public BaseTweakerAttribute(string name)
        {
            Name = name;
            Guid = Guid.NewGuid();
        }
    }
}
