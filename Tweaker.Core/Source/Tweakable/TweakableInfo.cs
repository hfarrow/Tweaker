using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ghostbit.Tweaker.Core
{
    public class TweakableInfo<T> : TweakerObjectInfo
    {
        public class TweakableRange
        {
            public T MinValue { get; private set; }
            public T MaxValue { get; private set; }

            public TweakableRange(T minValue, T maxValue)
            {
                MinValue = minValue;
                MaxValue = maxValue;
            }
        }

        public class TweakableStepSize
        {
            public T Size { get; private set; }

            public TweakableStepSize(T size)
            {
                Size = size;
            }
        }

        public class TweakableNamedToggleValue
        {
            public string Name;
            public T Value { get; private set; }

            public TweakableNamedToggleValue(string name, T value)
            {
                Name = name;
                Value = value;
            }
        }

        public class TweakableToggleValue : TweakableNamedToggleValue
        {
            public TweakableToggleValue(T value) :
                base(value.ToString(), value)
            {

            }
        }

        public TweakableRange Range;
        public TweakableStepSize StepSize;
        public TweakableNamedToggleValue[] ToggleValues;

        public TweakableInfo(string name, TweakableRange range, TweakableStepSize stepSize, TweakableNamedToggleValue[] toggleValues, uint instanceId = 0, string description = "") :
            base(name, instanceId, description)
        {
            Range = range;
            StepSize = stepSize;
            ToggleValues = toggleValues;
        }
    }
}