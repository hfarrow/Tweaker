using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class Tweakable : BaseTweakerAttribute
    {
        public Tweakable(string name) :
            base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class Range : Attribute
    {
        public object MinValue;
        public object MaxValue;

        public Range(object minValue, object maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class StepSize : Attribute
    {
        public object Size;

        public StepSize(object size)
        {
            Size = size;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class NamedToggleValue : Attribute
    {
        public string Name;
        public object Value;
        public uint Order;

        public NamedToggleValue(string name, object value, uint order = 0)
        {
            Name = name;
            Value = value;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class ToggleValue : NamedToggleValue
    {
        public ToggleValue(object value, uint order = 0) :
            base(value.ToString(), value, order)
        {
        }
    }
}
