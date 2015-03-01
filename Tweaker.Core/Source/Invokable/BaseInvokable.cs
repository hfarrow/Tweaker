using System.Collections.Generic;
using System.Reflection;
using System;

namespace Ghostbit.Tweaker.Core
{
    public abstract class BaseInvokable : TweakerObject, IInvokable
    {
        private InvokableInfo InvokableInfo { get; set; }

        public BaseInvokable(InvokableInfo info, Assembly assembly, WeakReference<object> instance, bool isPublic) :
            base(info, assembly, instance, isPublic)
        {
            InvokableInfo = info;
        }

        public object Invoke(object[] args = null)
        {
            CheckInstanceIsValid();

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
}