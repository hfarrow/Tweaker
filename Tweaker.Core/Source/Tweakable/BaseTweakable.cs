using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
	public class BaseTweakable<T> : TweakerObject, ITweakable
	{
		public TweakableInfo<T> TweakableInfo { get; private set; }
		public Type TweakableType { get; private set; }
		public ITweakableManager Manager { get; set; }
		public event Action<object, object> ValueChanged;

		protected MethodInfo Setter { get; set; }
		protected MethodInfo Getter { get; set; }
		protected VirtualProperty<T> virtualProperty;

		private IStepTweakable stepTweakable;
		private IToggleTweakable toggleTweakable;

		public override bool IsValid
		{
			get
			{
				if (virtualProperty != null)
				{
					return virtualProperty.IsValid;
				}
				return base.IsValid;
			}
		}

		public override WeakReference WeakInstance
		{
			get
			{
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
			get { return TweakableInfo.StepSize != null || HasToggle; }
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

		public object MinValue
		{
			get
			{
				if(HasRange)
				{
					return TweakableInfo.Range.MinValue;
				}
				return null;
			}
		}

		public object MaxValue
		{
			get
			{
				if (HasRange)
				{
					return TweakableInfo.Range.MaxValue;
				}
				return null;
			}
		}

		private BaseTweakable(TweakableInfo<T> info, Assembly assembly, WeakReference instance, bool isPublic) :
			base(info, assembly, instance, isPublic)
		{
			TweakableInfo = info;
			TweakableType = typeof(T);
		}

		private BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, Assembly assembly, WeakReference instance, bool isPublic) :
			this(info, assembly, instance, isPublic)
		{
			Setter = setter;
			Getter = getter;
			ValidateTweakableType();
			CreateComponents();
		}

		private BaseTweakable(TweakableInfo<T> info, VirtualProperty<T> property, Assembly assembly, bool isPublic) :
			this(info, assembly, new WeakReference(property), isPublic)
		{
			virtualProperty = property;
			Setter = property.Setter.Method;
			Getter = property.Getter.Method;
			ValidateTweakableType();
			CreateComponents();
		}

		public BaseTweakable(TweakableInfo<T> info, PropertyInfo property, WeakReference instance) :
			this(info, property.GetSetMethod(true), property.GetGetMethod(true),
				 property.ReflectedType.Assembly, instance, property.GetAccessors().Length > 0)
		{

		}

		public BaseTweakable(TweakableInfo<T> info, MethodInfo setter, MethodInfo getter, WeakReference instance) :
			this(info, setter, getter,
				 setter.ReflectedType.Assembly, instance, setter.IsPublic || getter.IsPublic)
		{

		}

		public BaseTweakable(TweakableInfo<T> info, FieldInfo field, WeakReference instance) :
			this(info, new VirtualProperty<T>(field, instance), field.ReflectedType.Assembly, field.IsPublic)
		{

		}

		public BaseTweakable(TweakableInfo<T> info, VirtualField<T> field)
			: this(info, Assembly.GetCallingAssembly(), new WeakReference(field), false)
		{
			Setter = field.Setter.Method;
			Getter = field.Getter.Method;
			ValidateTweakableType();
			CreateComponents();
		}

		private void CreateComponents()
		{
			if (TweakableType.IsEnum)
			{
				string[] enumNames = Enum.GetNames(TweakableType);
				Array enumValues = Enum.GetValues(TweakableType);
				int numValues = enumNames.Length;
				TweakableInfo.ToggleValues = new TweakableInfo<T>.TweakableNamedToggleValue[numValues];

				for(int i = 0; i < numValues; ++i)
				{
					TweakableInfo.ToggleValues[i] = new TweakableInfo<T>.TweakableNamedToggleValue(enumNames[i], (T)enumValues.GetValue(i));
				}
			}

			if (HasStep && !HasToggle)
			{
				stepTweakable = new StepTweakable<T>(this);
			}
			else if (HasToggle)
			{
				toggleTweakable = new ToggleTweakable<T>(this);
				stepTweakable = toggleTweakable;
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
			if (CheckInstanceIsValid())
			{
				try
				{
					return Getter.Invoke(GetInternalStrongInstance(), null);
				}
				catch (Exception e)
				{
					throw new TweakableGetException(Name, e);
				}
			}
			else
			{
				return null;
			}
		}

		public virtual void SetValue(object value)
		{
			if (CheckInstanceIsValid())
			{
				CheckValueType(value);
				object oldValue = Getter.Invoke(GetInternalStrongInstance(), null);
				value = CheckRange((T)value);

				if (oldValue == value)
				{
					return;
				}

				try
				{
					Setter.Invoke(GetInternalStrongInstance(), new object[] { value });
					if (ValueChanged != null)
					{
						ValueChanged(oldValue, value);
					}
				}
				catch (Exception e)
				{
					throw new TweakableSetException(Name, value.ToString(), e);
				}
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

			if (TweakableInfo.Range.MinValue != null && comparable.CompareTo(TweakableInfo.Range.MinValue) < 0)
			{
				return TweakableInfo.Range.MinValue;
			}
			else if (TweakableInfo.Range.MaxValue != null && comparable.CompareTo(TweakableInfo.Range.MaxValue) > 0)
			{
				return TweakableInfo.Range.MaxValue;
			}
			else
			{
				return value;
			}
		}

		protected override bool CheckInstanceIsValid()
		{
			if (!base.CheckInstanceIsValid())
			{
				Manager.UnregisterTweakable(this);
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}