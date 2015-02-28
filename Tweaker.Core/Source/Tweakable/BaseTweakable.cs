using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
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

        public bool HasStep
        {
            get { return TweakableInfo.StepSize != null; }
        }

        public bool HasToggle
        {
            get { return TweakableInfo.ToggleValues != null; }
        }

        public bool HasRange
        {
            get { return TweakableInfo.Range != null; }
        }

        public IStepTweakable Step
        {
            get { return stepTweakable; }
        }

        public IToggleTweakable Toggle
        {
            get { return toggleTweakable; }
        }

        private VirtualProperty<T> TryGetVirtualProperty()
        {
            if (instance == null)
            {
                return null;
            }

            object strongRef = null;
            instance.TryGetTarget(out strongRef);
            return strongRef as VirtualProperty<T>;
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

        private BaseTweakable(TweakableInfo<T> info, VirtualProperty<T> property, Assembly assembly, bool isPublic) :
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
            this(info, new VirtualProperty<T>(field, instance), field.ReflectedType.Assembly, field.IsPublic)
        {

        }

        private void CreateComponents()
        {
            if (HasStep)
            {
                stepTweakable = new StepTweakable<T>(this);
            }

            if (HasToggle)
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

            if (Getter.ReturnType != TweakableType)
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
            if (!typeof(T).IsAssignableFrom(value.GetType()))
            {
                throw new TweakableGetException(Name, "Cannot assign value of incorrect type '" + value.GetType().FullName + "' to BaseTweakable<" + typeof(T).FullName + ">");
            }
        }

        public virtual T CheckRange(T value)
        {
            if (TweakableInfo.Range == null)
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
}
