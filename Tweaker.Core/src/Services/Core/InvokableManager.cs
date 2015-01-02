using System.Collections.Generic;
using System;

namespace Ghostbit.Tweaker.Core
{
    public class InvokableManager :
        IInvokableManager
    {
        private BaseTweakerManager<IInvokable> baseManager;
        public InvokableManager(IScanner scanner)
        {
            baseManager = new BaseTweakerManager<IInvokable>(scanner);
            if (scanner != null)
            {
                scanner.AddProcessor(new InvokableProcessor());
            }
        }

        public IInvokable RegisterInvokable(InvokableInfo info, Delegate del)
        {
            var invokable = new InvokableMethod(info, del);
            RegisterInvokable(invokable);
            return invokable;
        }

        public void RegisterInvokable(IInvokable invokable)
        {
            baseManager.RegisterObject(invokable);
        }

        public void UnregisterInvokable(IInvokable invokable)
        {
            baseManager.UnregisterObject(invokable);
        }

        public void UnregisterInvokable(string name)
        {
            baseManager.UnregisterObject(name);
        }

        public TweakerDictionary<IInvokable> GetInvokables(SearchOptions options = null)
        {
            return baseManager.GetObjects(options);
        }

        public IInvokable GetInvokable(SearchOptions options)
        {
            return baseManager.GetObject(options);
        }

        public IInvokable GetInvokable(string name)
        {
            return baseManager.GetObject(name);
        }

        public object Invoke(IInvokable invokable, object[] args)
        {
            return invokable.Invoke(args);
        }

        public object Invoke(string name, object[] args)
        {
            IInvokable invokable = baseManager.GetObject(name);
            if (invokable == null)
            {
                throw new NotFoundException(name);
            }

            return Invoke(invokable, args);
        }
    }
}