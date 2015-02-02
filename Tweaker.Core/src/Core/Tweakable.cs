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

        public TweakableInfo(string name, TweakableRange range, TweakableStepSize stepSize, TweakableNamedToggleValue[] toggleValues) :
            base(name)
        {
            Range = range;
            StepSize = stepSize;
            ToggleValues = toggleValues;
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
}