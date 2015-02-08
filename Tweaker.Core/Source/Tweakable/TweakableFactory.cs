using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public static class TweakableFactory
    {

        public static ITweakable MakeTweakable(Tweakable attribute, PropertyInfo propertyInfo, object instance)
        {
            return MakeTweakable(attribute, propertyInfo.PropertyType, propertyInfo, instance);
        }

        public static ITweakable MakeTweakable(Tweakable attribute, FieldInfo fieldInfo, object instance)
        {
            return MakeTweakable(attribute, fieldInfo.FieldType, fieldInfo, instance);
        }

        public static ITweakable MakeTweakable(Tweakable attribute, Type type, MemberInfo memberInfo, object instance)
        {
            Type infoType = typeof(TweakableInfo<>).MakeGenericType(new Type[] { type });

            var rangeAttribute = memberInfo.GetCustomAttributes(typeof(Range), false).ElementAtOrDefault(0) as Range;
            var stepSizeAttribute = memberInfo.GetCustomAttributes(typeof(StepSize), false).ElementAtOrDefault(0) as StepSize;
            var toggleValueAttributes = memberInfo.GetCustomAttributes(typeof(NamedToggleValue), false) as NamedToggleValue[];
            toggleValueAttributes = toggleValueAttributes.OrderBy(toggle => toggle.Order).ToArray();

            object range = null;
            if (rangeAttribute != null)
            {
                Type rangeType = typeof(TweakableInfo<>.TweakableRange);
                rangeType = rangeType.MakeGenericType(new Type[] { type });
                range = Activator.CreateInstance(rangeType, new object[] { rangeAttribute.MinValue, rangeAttribute.MaxValue });
            }

            object stepSize = null;
            if (stepSizeAttribute != null)
            {
                Type stepSizeType = typeof(TweakableInfo<>.TweakableStepSize);
                stepSizeType = stepSizeType.MakeGenericType(new Type[] { type });
                stepSize = Activator.CreateInstance(stepSizeType, new object[] { stepSizeAttribute.Size });
            }

            Array toggleValues = null;
            if (toggleValueAttributes.Length > 0)
            {
                Type toggleValueType = typeof(TweakableInfo<>.TweakableNamedToggleValue);
                toggleValueType = toggleValueType.MakeGenericType(new Type[] { type });
                toggleValues = Array.CreateInstance(toggleValueType, toggleValueAttributes.Length);
                for (int i = 0; i < toggleValueAttributes.Length; ++i)
                {
                    toggleValues.SetValue(Activator.CreateInstance(toggleValueType, new object[] { toggleValueAttributes[i].Name, toggleValueAttributes[i].Value }), i);
                }
            }

            object info = Activator.CreateInstance(infoType, new object[] { attribute.Name, range, stepSize, toggleValues });
            if (range != null)
            {
                Type tweakableType = typeof(TweakableRange<>).MakeGenericType(new Type[] { type });
                return Activator.CreateInstance(tweakableType, new object[] { info, memberInfo, instance }) as ITweakable;
            }
            else if (toggleValues != null)
            {
                Type tweakableType = typeof(TweakableToggle<>).MakeGenericType(new Type[] { type });
                return Activator.CreateInstance(tweakableType, new object[] { info, memberInfo, instance }) as ITweakable;
            }
            else
            {
                Type tweakableType = typeof(BaseTweakable<>).MakeGenericType(new Type[] { type });
                return Activator.CreateInstance(tweakableType, new object[] { info, memberInfo, instance }) as ITweakable;
            }
        }

        public static ITweakable MakeTweakableFromInfo<T>(TweakableInfo<T> info, PropertyInfo propertyInfo, object instance)
        {
            if (typeof(T) != propertyInfo.PropertyType)
                return null; // T must match type of property

            if (info.Range != null)
            {
                return new TweakableRange<T>(info, propertyInfo, instance);
            }
            else if (info.ToggleValues != null)
            {
                return new TweakableToggle<T>(info, propertyInfo, instance);
            }
            else
            {
                return new BaseTweakable<T>(info, propertyInfo, instance);
            }
        }

        public static ITweakable MakeTweakableFromInfo<T>(TweakableInfo<T> info, FieldInfo fieldInfo, object instance)
        {
            if (typeof(T) != fieldInfo.FieldType)
                return null; // T must match type of property

            if (info.Range != null)
            {
                return new TweakableRange<T>(info, fieldInfo, instance);
            }
            else if (info.ToggleValues != null)
            {
                return new TweakableToggle<T>(info, fieldInfo, instance);
            }
            else
            {
                return new BaseTweakable<T>(info, fieldInfo, instance);
            }
        }
    }
}
