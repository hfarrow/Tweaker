using Tweaker.AssemblyScanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	public static class InvokableFactory
	{
		public static IInvokable MakeInvokable(InvokableAttribute attribute, MethodInfo methodInfo, IBoundInstance boundInstance)
		{
			string name = GetFinalName(attribute.Name, boundInstance);
			uint instanceId = boundInstance != null ? boundInstance.UniqueId : 0;
			string[] argDescriptions = GetArgDescriptions(methodInfo.GetParameters());
			string returnDescription = GetReturnDescription(methodInfo);
			return MakeInvokable(new InvokableInfo(name, instanceId, CustomTweakerAttributes.Get(methodInfo), attribute.Description,
				argDescriptions, returnDescription), methodInfo, boundInstance != null ? boundInstance.Instance : null);
		}

		public static IInvokable MakeInvokable(InvokableAttribute attribute, EventInfo eventInfo, IBoundInstance boundInstance)
		{
			string name = GetFinalName(attribute.Name, boundInstance);
			uint instanceId = boundInstance != null ? boundInstance.UniqueId : 0;
			object instance = boundInstance != null ? boundInstance.Instance : null;
			MethodInfo invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
			string[] argDescriptions = GetArgDescriptions(invokeMethod.GetParameters());
			string returnDescription = GetReturnDescription(invokeMethod);
			return MakeInvokable(new InvokableInfo(name, instanceId, CustomTweakerAttributes.Get(eventInfo), attribute.Description, argDescriptions, returnDescription),
				eventInfo, instance);
		}

		public static IInvokable MakeInvokable(InvokableInfo info, Delegate del)
		{
			var invokable = new InvokableMethod(info, del);
			return invokable;
		}

		public static IInvokable MakeInvokable(InvokableInfo info, MethodInfo methodInfo, object instance)
		{
			var invokable = new InvokableMethod(
				info,
				methodInfo,
				instance == null ? null : new WeakReference(instance));
			return invokable;
		}

		public static IInvokable MakeInvokable(InvokableInfo info, EventInfo eventInfo, object instance)
		{
			FieldInfo fieldInfo = GetBackingEventField(eventInfo, instance);
			return MakeInvokableFromBackingEventField(info, eventInfo, fieldInfo, instance);
		}

		public static IInvokable MakeInvokableFromBackingEventField(InvokableInfo info, EventInfo eventInfo, FieldInfo fieldInfo, object instance)
		{
			return new InvokableEvent(
				info,
				eventInfo,
				fieldInfo,
				instance == null ? null : new WeakReference(instance));
		}

		public static string[] GetArgDescriptions(ParameterInfo[] parameters)
		{
			string[] argDescriptions = new string[parameters.Length];
			for (var i = 0; i < parameters.Length; ++i)
			{
				object[] argAttributes = parameters[i].GetCustomAttributes(typeof(ArgDescriptionAttribute), true);
				if (argAttributes != null && argAttributes.Length > 0)
				{
					argDescriptions[i] = (argAttributes[0] as ArgDescriptionAttribute).Description;
				}
				else
				{
					argDescriptions[i] = "";
				}
			}

			return argDescriptions;
		}

		public static string GetReturnDescription(MethodInfo methodInfo)
		{
			object[] attributes = methodInfo.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(ReturnDescriptionAttribute), true);
			if (attributes.Length > 0)
			{
				return (attributes[0] as ReturnDescriptionAttribute).Description;
			}
			else
			{
				return "";
			}
		}

		public static FieldInfo GetBackingEventField(EventInfo eventInfo, object instance = null)
		{
			var type = eventInfo.ReflectedType;
			var fieldInfo = type.GetField(eventInfo.Name, ReflectionUtil.GetBindingFlags(instance) | BindingFlags.NonPublic);
			if (fieldInfo == null)
			{
				throw new ProcessorException("Could not find backing field for event.");
			}

			return fieldInfo;
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