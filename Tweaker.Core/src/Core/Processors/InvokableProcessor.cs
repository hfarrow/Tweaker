using Ghostbit.Tweaker.AssemblyScanner;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// This is an IScanner processor that processes types or members annotated with Invokable
    /// and produces an IInvokable as a result.
    /// </summary>
    /// <remarks>
    /// Tweaker does not directly enforce registered names to have a path or group separator
    /// but if a Type is annotated with Invokable a period will be used to separate the provided
    /// group name with the name of the invokable members.
    /// </remarks>
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
