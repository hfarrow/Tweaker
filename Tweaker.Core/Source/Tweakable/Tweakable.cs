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
        public TweakableInfo<T> TweakableInfo { get; private set; }
        protected MethodInfo Setter { get; set; }
        protected MethodInfo Getter { get; set; }
        public Type TweakableType { get; private set; }

        private IStepTweakable stepTweakable;
        private IToggleTweakable toggleTweakable;


        public override bool IsValid
        {
            get
            {
                var virtualProperty = TryGetVirtualProperty();
                if (virtualProperty != null)
                {
                    return virtualProperty.IsValid;
                }
                return base.IsValid;
            }
        }

        public override WeakReference<object> WeakInstance
        {
            get
            {
                var virtualProperty = TryGetVirtualProperty();
                if (virtualProperty != null)
                {
                    return virtualProperty.WeakInstance;
                }
                return base.WeakInstance;
            }
        }

        public override object StrongInstance
        {
            get
            {
                var virtualProperty = TryGetVirtualProperty();
                if (virtualProperty != null)
                {
                    return virtualProperty.StrongInstance;
                }
                return base.StrongInstance;
            }
        }

        private object GetInternalStrongInstance()
        {
            if (instance == null)
            {
                return null;
            }

            object strongRef = null;
            instance.TryGetTarget(out strongRef);
            return strongRef;
        }

        public bool IsSteppable
        {
            get { return TweakableInfo.StepSize != null; }
        }

        public bool IsToggable
        {
            get { return TweakableInfo.ToggleValues != null; }
        }

        public bool HasRange
        {
            get { return TweakableInfo.Range != null; }
        }

        public IStepTweakable AsStep
        {
            get { return stepTweakable; }
        }

        public IToggleTweakable AsToggle
        {
            get { return toggleTweakable; }
        }

        private TweakableVirtualProperty<T> TryGetVirtualProperty()
        {
            if(instance == null)
            {
                return null;
            }

            object strongRef = null;
            instance.TryGetTarget(out strongRef);
            return strongRef as TweakableVirtualProperty<T>;
        }

        private BaseTweakable(TweakableInfo<T> info, Assembly assembly, WeakReference<object> instance, bool isPublic) :
            base(info, assembly, instance, isPublic)
        {
            TweakableInfo = info;
            TweakableType = typeof(T);
            CreateComponents();
        }

        private BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, Assembly assembly, WeakReference<object> instance, bool isPublic) :
            this(info, assembly, instance, isPublic)
        {
            Setter = setter;
            Getter = getter;
            ValidateTweakableType();
            CreateComponents();
        }

        private BaseTweakable(TweakableInfo<T> info, TweakableVirtualProperty<T> property, Assembly assembly, bool isPublic) :
            this(info, assembly, new WeakReference<object>(property), isPublic)
        {
            Setter = property.Setter.Method;
            Getter = property.Getter.Method;
            ValidateTweakableType();
            CreateComponents();
        }

        public BaseTweakable(TweakableInfo<T> info, PropertyInfo property, WeakReference<object> instance) :
            this(info, property.GetSetMethod(true), property.GetGetMethod(true),
                 property.ReflectedType.Assembly, instance, property.GetAccessors().Length > 0)
        {

        }

        public BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, WeakReference<object> instance) :
            this(info, setter, getter,
                 setter.ReflectedType.Assembly, instance, setter.IsPublic || getter.IsPublic)
        {

        }

        public BaseTweakable(TweakableInfo<T> info, FieldInfo field, WeakReference<object> instance) :
            this(info, new TweakableVirtualProperty<T>(field, instance), field.ReflectedType.Assembly, field.IsPublic)
        {

        }

        private void CreateComponents()
        {
            if(IsSteppable)
            {
                stepTweakable = new StepTweakable<T>(this);
            }

            if(IsToggable)
            {
                toggleTweakable = new ToggleTweakable<T>(this);
            }
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
            CheckInstanceIsValid();
            try
            {
                return Getter.Invoke(GetInternalStrongInstance(), null);
            }
            catch (Exception e)
            {
                throw new TweakableGetException(Name, e);
            }
        }

        public virtual void SetValue(object value)
        {
            CheckInstanceIsValid();
            CheckValueType(value);
            value = CheckRange((T)value);
            try
            {
                Setter.Invoke(GetInternalStrongInstance(), new object[] { value });
            }
            catch (Exception e)
            {
                throw new TweakableSetException(Name, value.ToString(), e);
            }
        }

        public virtual void CheckValueType(object value)
        {
            if(!typeof(T).IsAssignableFrom(value.GetType()))
            {
                throw new TweakableGetException(Name, "Cannot assign value of incorrect type '" + value.GetType().FullName + "' to BaseTweakable<" + typeof(T).FullName + ">");
            }
        }

        public virtual T CheckRange(T value)
        {
            if(TweakableInfo.Range == null)
            {
                return value;
            }

            var comparable = value as IComparable;
            if (comparable == null)
            {
                throw new TweakableSetException(Name, "TweakableRange<" + typeof(T).FullName + "> does not implement IComparable");
            }

            if (comparable.CompareTo(TweakableInfo.Range.MinValue) < 0)
            {
                return TweakableInfo.Range.MinValue;
            }
            else if (comparable.CompareTo(TweakableInfo.Range.MaxValue) > 0)
            {
                return TweakableInfo.Range.MaxValue;
            }
            else
            {
                return value;
            }
        }
    }

    public class StepTweakable<T> : IStepTweakable
    {
        private readonly BaseTweakable<T> baseTweakable;
        public BaseTweakable<T> BaseTweakable { get { return baseTweakable; } }

        private readonly MethodInfo addMethod;
        private readonly MethodInfo subtractMethod;

        public StepTweakable(BaseTweakable<T> baseTweakable)
        {
            this.baseTweakable = baseTweakable;

            Type type = typeof(T);
            addMethod = type.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public);
            subtractMethod = type.GetMethod("op_Subtraction", BindingFlags.Static | BindingFlags.Public);

            if(type.IsPrimitive)
            {
                addMethod = typeof(PrimitiveHelper).GetMethod("Add", BindingFlags.Static | BindingFlags.Public);
                subtractMethod = typeof(PrimitiveHelper).GetMethod("Subtract", BindingFlags.Static | BindingFlags.Public);
            }
            else if(addMethod == null)
            {
                throw new StepTweakableInvalidException(baseTweakable.Name, "No 'operator +' could be found on type '" + type.FullName + "'");
            }
            else if (subtractMethod == null)
            {
                throw new StepTweakableInvalidException(baseTweakable.Name, "No 'operator -' could be found on type '" + type.FullName + "'");
            }
        }

        public object StepSize
        {
            get { return baseTweakable.TweakableInfo.StepSize.Size; }
        }

        public object StepNext()
        {
            T newValue = (T)addMethod.Invoke(null, new object[] { (T)baseTweakable.GetValue(), StepSize });
            baseTweakable.SetValue(newValue);
            return baseTweakable.GetValue();
        }

        public object StepPrevious()
        {
            T newValue = (T)subtractMethod.Invoke(null, new object[] { (T)baseTweakable.GetValue(), StepSize });
            baseTweakable.SetValue(newValue);
            return baseTweakable.GetValue();
        }
    }

    public class ToggleTweakable<T> : IToggleTweakable
    {
        private readonly BaseTweakable<T> baseTweakable;
        public BaseTweakable<T> BaseTweakable { get { return baseTweakable; } }

        private int currentIndex = -1;
        private TweakableInfo<T> tweakableInfo;

        public ToggleTweakable(BaseTweakable<T> baseTweakable)
        {
            this.baseTweakable = baseTweakable;
            tweakableInfo = baseTweakable.TweakableInfo;
        }

        public object StepSize
        {
            get { return 1; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        public int GetIndexOfValue(object value)
        {
            for (int i = 0; i < tweakableInfo.ToggleValues.Length; ++i)
            {
                var toggleValue = tweakableInfo.ToggleValues[i];
                if (toggleValue.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetNameByIndex(int index)
        {
            if (index >= 0 && tweakableInfo.ToggleValues.Length > index)
                return tweakableInfo.ToggleValues[index].Name;
            else
                return null;
        }

        public string GetNameByValue(object value)
        {
            return GetNameByIndex(GetIndexOfValue(value));
        }

        public object SetValueByName(string valueName)
        {
            for (int i = 0; i < tweakableInfo.ToggleValues.Length; ++i)
            {
                if (tweakableInfo.ToggleValues[i].Name == valueName)
                {
                    currentIndex = i;
                    baseTweakable.SetValue(tweakableInfo.ToggleValues[i].Value);
                    return baseTweakable.GetValue();
                }
            }

            throw new TweakableSetException(baseTweakable.Name, "Invalid toggle value name: '" + valueName + "'");
        }

        public string GetValueName()
        {
            if (currentIndex >= 0 && currentIndex < tweakableInfo.ToggleValues.Length)
            {
                return tweakableInfo.ToggleValues[currentIndex].Name;
            }
            return "Unkown";
        }

        public object StepNext()
        {
            currentIndex++;
            if (currentIndex >= tweakableInfo.ToggleValues.Length)
                currentIndex = 0;

            var nextValue = tweakableInfo.ToggleValues[currentIndex].Value;
            baseTweakable.SetValue(nextValue);
            return baseTweakable.GetValue();
        }

        public object StepPrevious()
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = tweakableInfo.ToggleValues.Length - 1;

            var nextValue = tweakableInfo.ToggleValues[currentIndex].Value;
            baseTweakable.SetValue(nextValue);
            return baseTweakable.GetValue();
        }
    }

    public class TweakableVirtualProperty<T>
    {
        public WeakReference<object> WeakInstance { get { return weakReference; } }
        public Action<T> Setter { get { return setter; } }
        public Func<T> Getter { get { return getter; } }

        private readonly FieldInfo fieldInfo;
        private readonly WeakReference<object> weakReference;
        private readonly Action<T> setter;
        private readonly Func<T> getter;

        public object StrongInstance
        {
            get
            {
                if(weakReference == null)
                {
                    return null;
                }

                object strongRef = null;
                WeakInstance.TryGetTarget(out strongRef);
                return strongRef;
            }
        }

        public bool IsValid
        {
            get
            {
                return WeakInstance == null || StrongInstance != null;
            }
        }

        public TweakableVirtualProperty(FieldInfo field, WeakReference<object> instance)
        {
            fieldInfo = field;
            weakReference = instance;
            setter = SetValue;
            getter = GetValue;
        }

        private void SetValue(T value)
        {
            fieldInfo.SetValue(StrongInstance, value);
        }

        private T GetValue()
        {
            return (T)fieldInfo.GetValue(StrongInstance);
        }
    }
}