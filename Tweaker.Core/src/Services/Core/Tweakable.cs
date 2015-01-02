using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using Ghostbit.Tweaker.Core.TweakableAttributes;


namespace Ghostbit.Tweaker.Core
{
    public interface ITweakable : ITweakerObject
    {
        void SetValue(object value);
        object GetValue();
        Type TweakableType { get; }
    }

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

        public TweakableInfo(string name, TweakableRange range, TweakableStepSize stepSize, TweakableNamedToggleValue[] toggleValues) :
            base(name)
        {
            Range = range;
            StepSize = stepSize;
            ToggleValues = toggleValues;
        }
    }

    namespace TweakableAttributes
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

    public class BaseTweakable<T> : TweakerObject, ITweakable
    {
        protected TweakableInfo<T> TweakableInfo { get; set; }
        protected MethodInfo Setter { get; set; }
        protected MethodInfo Getter { get; set; }

        public Type TweakableType { get; private set; }

        private BaseTweakable(TweakableInfo<T> info, Assembly assembly, object instance, bool isPublic) :
            base(info, assembly, instance, isPublic)
        {
            TweakableInfo = info;
            TweakableType = typeof(T);
        }

        private BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, Assembly assembly, object instance, bool isPublic) :
            this(info, assembly, instance, isPublic)
        {
            Setter = setter;
            Getter = getter;
            ValidateTweakableType();
        }

        private BaseTweakable(TweakableInfo<T> info, TweakableVirtualProperty<T> property, Assembly assembly, bool isPublic) :
            this(info, assembly, property, isPublic)
        {
            Setter = property.Setter.Method;
            Getter = property.Getter.Method;
            ValidateTweakableType();
        }

        public BaseTweakable(TweakableInfo<T> info, PropertyInfo property, object instance) :
            this(info, property.GetSetMethod(true), property.GetGetMethod(true),
                 property.ReflectedType.Assembly, instance, property.GetAccessors().Length > 0)
        {

        }

        public BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, object instance) :
            this(info, setter, getter,
                 setter.ReflectedType.Assembly, instance, setter.IsPublic || getter.IsPublic)
        {

        }

        public BaseTweakable(TweakableInfo<T> info, FieldInfo field, object instance) :
            this(info, new TweakableVirtualProperty<T>(field, instance), field.ReflectedType.Assembly, field.IsPublic)
        {

        }

        private void ValidateTweakableType()
        {
            if (Getter == null)
                throw new TweakableGetException(Name, "Getter does not exist.");

            if (Setter == null)
                throw new TweakableSetException(Name, "Setter does not exist.");

            if(Getter.ReturnType != TweakableType)
                throw new TweakableGetException(Name, "Getter returns type '" + Getter.ReturnType.FullName + 
                    "' instead of type '" + TweakableType.FullName + "'.");

            var parameters = Setter.GetParameters();
            if (parameters.Length != 1)
                throw new TweakableSetException(Name, "Setter takes " + parameters.Length + " paremeters instead of 1.");
            if (parameters[0].ParameterType != TweakableType)
                throw new TweakableSetException(Name, "Setter takes type '" + parameters[0].GetType().FullName +
                    "' instead of type '" + TweakableType.FullName + "'.");
        }

        public virtual object GetValue()
        {
            try
            {
                return Getter.Invoke(Instance, null);
            }
            catch (Exception e)
            {
                throw new TweakableGetException(Name, e);
            }
        }

        public virtual void SetValue(object value)
        {
            try
            {
                Setter.Invoke(Instance, new object[] { value });
            }
            catch (Exception e)
            {
                throw new TweakableSetException(Name, value.ToString(), e);
            }
        }
    }

    public class TweakableRange<T> : BaseTweakable<T>
    {
        public TweakableRange(TweakableInfo<T> info, PropertyInfo property, object instance) :
            base(info, property, instance)
        {

        }

        public TweakableRange(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, object instance) :
            base(info, setter, getter, instance)
        {

        }

        public TweakableRange(TweakableInfo<T> info, FieldInfo field, object instance) :
            base(info, field, instance)
        {

        }

        public override void SetValue(object value)
        {
            var comparable = value as IComparable;
            if (comparable == null)
                throw new TweakableSetException(Name, "TweakableRange<" + typeof(T).FullName + "> does not implement IComparable");

            if (comparable.CompareTo(TweakableInfo.Range.MinValue) < 0)
                base.SetValue(TweakableInfo.Range.MinValue);
            else if (comparable.CompareTo(TweakableInfo.Range.MaxValue) > 0)
                base.SetValue(TweakableInfo.Range.MaxValue);
            else
                base.SetValue(value);
        }
    }

    public class TweakableToggle<T> : BaseTweakable<T>
    {
        private int currentIndex = -1;

        public TweakableToggle(TweakableInfo<T> info, PropertyInfo property, object instance) :
            base(info, property, instance)
        {

        }

        public TweakableToggle(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, object instance) :
            base(info, setter, getter, instance)
        {

        }

        public TweakableToggle(TweakableInfo<T> info, FieldInfo field, object instance) :
            base(info, field, instance)
        {

        }

        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        public int GetIndexOfValue(T value)
        {
            for (int i = 0; i < TweakableInfo.ToggleValues.Length; ++i)
            {
                var toggleValue = TweakableInfo.ToggleValues[i];
                if (toggleValue.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetNameByIndex(int index)
        {
            if (index >= 0 && TweakableInfo.ToggleValues.Length > index)
                return TweakableInfo.ToggleValues[index].Name;
            else
                return null;
        }

        public string GetNameByValue(T value)
        {
            return GetNameByIndex(GetIndexOfValue(value));
        }

        public void SetValueByName(string valueName)
        {
            for (int i = 0; i < TweakableInfo.ToggleValues.Length; ++i)
            {
                if (TweakableInfo.ToggleValues[i].Name == valueName)
                {
                    currentIndex = i;
                    SetValue(TweakableInfo.ToggleValues[i].Value);
                    return;
                }
            }

            throw new TweakableSetException(Name, "Invalid toggle value name: '" + valueName + "'");
        }

        public string GetValueName()
        {
            if (currentIndex >= 0 && currentIndex < TweakableInfo.ToggleValues.Length)
            {
                return TweakableInfo.ToggleValues[currentIndex].Name;
            }
            return "Unkown";
        }

        public T NextValue()
        {
            currentIndex++;
            if (currentIndex >= TweakableInfo.ToggleValues.Length)
                currentIndex = 0;

            var nextValue = TweakableInfo.ToggleValues[currentIndex].Value;
            SetValue(nextValue);
            return nextValue;
        }

        public T PreviousValue()
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = TweakableInfo.ToggleValues.Length - 1;

            var nextValue = TweakableInfo.ToggleValues[currentIndex].Value;
            SetValue(nextValue);
            return nextValue;
        }
    }

    public class TweakableVirtualProperty<T>
    {
        private readonly FieldInfo fieldInfo;
        public object Instance { get; private set; }
        public Action<T> Setter { get; private set; }
        public Func<T> Getter { get; private set; }

        public TweakableVirtualProperty(FieldInfo field, object instance)
        {
            fieldInfo = field;
            Instance = instance;
            Setter = SetValue;
            Getter = GetValue;
        }

        private void SetValue(T value)
        {
            fieldInfo.SetValue(Instance, value);
        }

        private T GetValue()
        {
            return (T)fieldInfo.GetValue(Instance);
        }
    }

    public class TweakableProcessor : IAttributeScanProcessor<Tweakable, ITweakable>
    {
        public void ProcessAttribute(Tweakable input, Type type)
        {
            foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                if (memberInfo.MemberType == MemberTypes.Property ||
                    memberInfo.MemberType == MemberTypes.Field)
                {
                    if (memberInfo.GetCustomAttributes(typeof(Tweakable), false).Length == 0)
                    {
                        Tweakable inner = new Tweakable(input.Name + "." + memberInfo.Name);
                        ProcessAttribute(inner, memberInfo);
                    }
                }
            }
        }

        public void ProcessAttribute(Tweakable input, MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                var propertyInfo = (PropertyInfo)memberInfo;
                var tweakable = TweakableFactory.MakeTweakable(input, propertyInfo, null);
                ProvideResult(tweakable);
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                var fieldInfo = (FieldInfo)memberInfo;
                var tweakable = TweakableFactory.MakeTweakable(input, fieldInfo, null);
                ProvideResult(tweakable);
            }
        }

        public event EventHandler<ScanResultArgs<ITweakable>> ResultProvided;

        private void ProvideResult(ITweakable tweakable)
        {
            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<ITweakable>(tweakable));
        }
    }

    public class TweakableFactory
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
                for(int i = 0; i < toggleValueAttributes.Length; ++i)
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