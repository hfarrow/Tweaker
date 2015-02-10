using Ghostbit.Tweaker.AssemblyScanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
    public static class InvokableFactory
    {
        private static Dictionary<string, uint> nextIdMap = new Dictionary<string,uint>();

        public static IInvokable MakeInvokable(Invokable attribute, MethodInfo methodInfo, IBoundInstance instance)
        {
            string name = GetFinalName(attribute.Name, instance);
            return MakeInvokable(new InvokableInfo(name), methodInfo, instance != null ? instance.Instance : null);
        }

        public static IInvokable MakeInvokable(Invokable attribute, EventInfo eventInfo, IBoundInstance instance)
        {
            string name = GetFinalName(attribute.Name, instance);
            return MakeInvokable(new InvokableInfo(name), eventInfo, instance != null ? instance.Instance : null);
        }

        public static IInvokable MakeInvokable(InvokableInfo info, Delegate del)
        {
            var invokable = new InvokableMethod(info, del);
            return invokable;
        }
        
        public static IInvokable MakeInvokable(InvokableInfo info, MethodInfo methodInfo, object instance)
        {
            var invokable = new InvokableMethod(info, methodInfo, new WeakReference<object>(instance));
            return invokable;
        }

        public static IInvokable MakeInvokable(InvokableInfo info, EventInfo eventInfo, object instance)
        {
            var type = eventInfo.ReflectedType;
            var fieldInfo = type.GetField(eventInfo.Name, ReflectionUtil.GetBindingFlags(instance) | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new ProcessorException("Could not find backing field for event.");
            }
            return new InvokableEvent(info, fieldInfo, new WeakReference<object>(instance));
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
