using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
	public static class TweakableFactory
	{

		public static ITweakable MakeTweakable(TweakableAttribute attribute, PropertyInfo propertyInfo, IBoundInstance instance, MemberInfo containerMemberInfo = null)
		{
			return MakeTweakable(attribute, propertyInfo.PropertyType, propertyInfo, instance, containerMemberInfo);
		}

		public static ITweakable MakeTweakable(TweakableAttribute attribute, FieldInfo fieldInfo, IBoundInstance instance, MemberInfo containerMemberInfo = null)
		{
			return MakeTweakable(attribute, fieldInfo.FieldType, fieldInfo, instance, containerMemberInfo);
		}

		public static ITweakable MakeTweakable(TweakableAttribute attribute, Type type, MemberInfo memberInfo, IBoundInstance instance, MemberInfo containerMemberInfo = null)
		{
			Type infoType = typeof(TweakableInfo<>).MakeGenericType(new Type[] { type });
			uint instanceId = instance != null ? instance.UniqueId : 0;

			MemberInfo memberInfoWithAttributes = containerMemberInfo != null ? containerMemberInfo : memberInfo;
			var rangeAttribute = memberInfoWithAttributes.GetCustomAttributes(typeof(TweakerRangeAttribute), false).ElementAtOrDefault(0) as TweakerRangeAttribute;
			var stepSizeAttribute = memberInfoWithAttributes.GetCustomAttributes(typeof(StepSizeAttribute), false).ElementAtOrDefault(0) as StepSizeAttribute;
			var toggleValueAttributes = memberInfoWithAttributes.GetCustomAttributes(typeof(NamedToggleValueAttribute), false) as NamedToggleValueAttribute[];
			var customAttributes = memberInfoWithAttributes.GetCustomAttributes(typeof(ICustomTweakerAttribute), true) as ICustomTweakerAttribute[];
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
				if (type.IsPrimitive)
				{
					stepSize = Activator.CreateInstance(stepSizeType, new object[] { stepSizeAttribute.Size });
				}
				else
				{
					object customTypeInstance = null;
					try
					{
						customTypeInstance = Activator.CreateInstance(type, new object[] { stepSizeAttribute.Size });
					}
					catch (MissingMethodException ex)
					{
						throw new StepTweakableInvalidException(attribute.Name, "The type '" + type.FullName +
							"' must have a constructor that takes a single argument of type '" +
							stepSizeAttribute.Size.GetType().FullName + "'", ex);
					}
					catch (MethodAccessException ex)
					{
						throw new StepTweakableInvalidException(attribute.Name, "The type '" + type.FullName +
							"' has the a constructor that takes a single argument of type '" +
							stepSizeAttribute.Size.GetType().FullName + " but it is not public.", ex);
					}

					if (customTypeInstance != null)
					{
						stepSize = Activator.CreateInstance(stepSizeType, new object[] { customTypeInstance });
					}
				}
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

			WeakReference weakRef = null;
			if (instance != null)
			{
				weakRef = new WeakReference(instance.Instance);
			}

			string name = GetFinalName(attribute.Name, instance);
			object info = Activator.CreateInstance(infoType, new object[] { name, range, stepSize, toggleValues, instanceId, customAttributes, attribute.Description });
			Type tweakableType = typeof(BaseTweakable<>).MakeGenericType(new Type[] { type });
			return Activator.CreateInstance(tweakableType, new object[] { info, memberInfo, weakRef }) as ITweakable;
		}

		public static ITweakable MakeTweakableFromInfo<T>(TweakableInfo<T> info, PropertyInfo propertyInfo, object instance)
		{
			if (typeof(T) != propertyInfo.PropertyType)
				return null; // T must match type of property

			WeakReference weakRef;
			if (instance == null)
			{
				weakRef = null;
			}
			else
			{
				weakRef = new WeakReference(instance);
			}

			return new BaseTweakable<T>(info, propertyInfo, weakRef);
		}

		public static ITweakable MakeTweakableFromInfo<T>(TweakableInfo<T> info, FieldInfo fieldInfo, object instance)
		{
			if (typeof(T) != fieldInfo.FieldType)
				return null; // T must match type of property

			WeakReference weakRef;
			if (instance == null)
			{
				weakRef = null;
			}
			else
			{
				weakRef = new WeakReference(instance);
			}

			return new BaseTweakable<T>(info, fieldInfo, weakRef);
		}

		public static ITweakable MakeTweakableFromInfo<T>(TweakableInfo<T> info, out object virtualFieldRef)
		{
			VirtualField<T> field = new VirtualField<T>();
			ITweakable tweakable = new BaseTweakable<T>(info, field);
			virtualFieldRef = field;
			return tweakable;
		}

		/// <summary>
		/// This will create a virtual field bound to a tweakable.
		/// TODO: allow range, step, toggles, and CustomAttributes to be passed in.
		/// </summary>
		/// <param name="tweakableType"></param>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="virtualFieldRef">A strong reference to the internal virtual field. 
		/// If you do not maintain a strong reference to this opaque object, the tweakable will become invalid
		/// when the virtual field is garbage collected. The tweakable created by this method
		/// is not bound to a class member defined in code which is the reason for this param.</param>
		/// <returns></returns>
		public static ITweakable MakeTweakable(Type tweakableType, string name, string description, out object virtualFieldRef)
		{
			Type infoType = typeof(TweakableInfo<>).MakeGenericType(tweakableType);
			Type fieldType = typeof(VirtualField<>).MakeGenericType(tweakableType);
			Type baseTweakableType = typeof(BaseTweakable<>).MakeGenericType(tweakableType);
			object info = Activator.CreateInstance(infoType, name, new ICustomTweakerAttribute[0], description);
			object field = Activator.CreateInstance(fieldType);
			ITweakable tweakable = Activator.CreateInstance(
				baseTweakableType,
				Convert.ChangeType(info, infoType),
				Convert.ChangeType(field, fieldType)) as ITweakable;

			virtualFieldRef = field;
			return tweakable;
		}

		private static string GetFinalName(string name, IBoundInstance instance)
		{
			if (instance == null)
			{
				return name;
			}
			else
			{
				return string.Format("{0}#{1}", name, instance.UniqueId);
			}
		}
	}
}
