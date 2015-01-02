using System.Collections.Generic;
using System.Reflection;
using System;
using Ghostbit.Tweaker.Core.InvokableAttributes;

namespace Ghostbit.Tweaker.Core
{
    public interface IInvokable : ITweakerObject
    {
        object Invoke(object[] args);
    }

    public class InvokableInfo : TweakerObjectInfo
    {
        public InvokableInfo(string name)
            : base(name)
        {
        }
    }

    namespace InvokableAttributes
    {
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Class, AllowMultiple = false)]
        public class Invokable : Attribute, ITweakerAttribute
        {
            public string Name { get; private set; }
            public Guid Guid { get; private set; }

            public Invokable(string name)
            {
                Name = name;
                Guid = Guid.NewGuid();
            }
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

        public object Invoke(object[] args)
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

    public class InvokableProcessor : IAttributeScanProcessor<Invokable, IInvokable>
    {
        public void ProcessAttribute(Invokable input, Type type)
        {
            foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
            {
                if (memberInfo.MemberType == MemberTypes.Method ||
                    memberInfo.MemberType == MemberTypes.Event)
                {
                    if (memberInfo.GetCustomAttributes(typeof(Invokable), false).Length == 0)
                    {
                        Invokable inner = new Invokable(input.Name + "." + memberInfo.Name);
                        ProcessAttribute(inner, memberInfo);
                    }
                }
            }
        }

        public void ProcessAttribute(Invokable input, MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Method)
            {
                var methodInfo = (MethodInfo)memberInfo;
                var invokable = new InvokableMethod(new InvokableInfo(input.Name), methodInfo, null);
                ProvideResult(invokable);
            }
            else if (memberInfo.MemberType == MemberTypes.Event)
            {
                var eventInfo = (EventInfo)memberInfo;
                var type = eventInfo.ReflectedType;

                var fieldInfo = type.GetField(eventInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var invokable = new InvokableEvent(new InvokableInfo(input.Name), fieldInfo, null);
                ProvideResult(invokable);
            }
            else
            {
                throw new ScannerException("InvokableProcessor cannot process non MethodInfo or EventInfo types");
            }
        }

        public event EventHandler<ScanResultArgs<IInvokable>> ResultProvided;

        private void ProvideResult(IInvokable invokable)
        {
            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<IInvokable>(invokable));  
        }
    }
}