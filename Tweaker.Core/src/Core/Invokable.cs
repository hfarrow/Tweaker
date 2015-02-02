using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ghostbit.Tweaker.Core
{
    public class InvokableInfo : TweakerObjectInfo
    {
        public InvokableInfo(string name)
            : base(name)
        {
        }
    }

    public abstract class BaseInvokable : TweakerObject, IInvokable
    {
        private InvokableInfo InvokableInfo { get; set; }

        public BaseInvokable(InvokableInfo info, Assembly assembly, object instance, bool isPublic) :
            base(info, assembly, instance, isPublic)
        {
            InvokableInfo = info;
        }

        public object Invoke(object[] args = null)
        {
            try
            {
                return DoInvoke(args);
            }
            catch(Exception e)
            {
                throw new InvokeException(Name, args, e);
            }
        }

        protected abstract object DoInvoke(object[] args);
    }

    public class InvokableMethod : BaseInvokable
    {
        private readonly MethodInfo methodInfo;

        public MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        public InvokableMethod(InvokableInfo info, MethodInfo methodInfo, object instance)
            : base(info, methodInfo.ReflectedType.Assembly, instance, methodInfo.IsPublic)
        {
            this.methodInfo = methodInfo;
        }

        public InvokableMethod(InvokableInfo info, Delegate methodDelegate)
            : base(info, methodDelegate.Method.ReflectedType.Assembly, methodDelegate.Target, methodDelegate.Method.IsPublic)
        {
            this.methodInfo = methodDelegate.Method;
        }

        protected override object DoInvoke(object[] args)
        {
            return MethodInfo.Invoke(Instance, args);
        }
    }

    public class InvokableEvent : BaseInvokable
    {
        private readonly FieldInfo fieldInfo;

        public FieldInfo FieldInfo
        {
            get { return fieldInfo; }
        }

        public InvokableEvent(InvokableInfo info, FieldInfo fieldInfo, object instance)
            : base(info, fieldInfo.ReflectedType.Assembly, instance, fieldInfo.IsPublic)
        {
            this.fieldInfo = fieldInfo;
        }

        protected override object DoInvoke(object[] args)
        {
            object ret = default(object);
            var value = fieldInfo.GetValue(Instance);
            // value will be null if no listeners added.
            if (value != null)
            {
                var eventDelegate = (MulticastDelegate)value;
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        ret = handler.Method.Invoke(handler.Target, args);
                    }
                }
            }
            return ret;
        }
    }
}