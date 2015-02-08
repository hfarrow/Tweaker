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
        public static IInvokable MakeInvokable(Invokable attribute, MethodInfo methodInfo, object instance)
        {
            return MakeInvokable(new InvokableInfo(attribute.Name), methodInfo, instance);
        }

        public static IInvokable MakeInvokable(Invokable attribute, EventInfo eventInfo, object instance)
        {
            return MakeInvokable(new InvokableInfo(attribute.Name), eventInfo, instance);
        }

        public static IInvokable MakeInvokable(InvokableInfo info, Delegate del)
        {
            var invokable = new InvokableMethod(info, del);
            return invokable;
        }
        
        public static IInvokable MakeInvokable(InvokableInfo info, MethodInfo methodInfo, object instance)
        {
            var invokable = new InvokableMethod(info, methodInfo, instance);
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
            return new InvokableEvent(info, fieldInfo, instance);
        }
    }
}
