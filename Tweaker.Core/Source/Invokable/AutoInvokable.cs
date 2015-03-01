using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
    public class AutoInvokableBase
    {
        public static IInvokableManager Manager { get; set; }
    }

    public class AutoInvokable : AutoInvokableBase, IDisposable
    {
        public IInvokable invokable;

        public AutoInvokable(string invokableName, string methodName, IBoundInstance instance)
        {
            if (CheckForManager())
            {
                MethodInfo[] methods = instance.Type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < methods.Length; ++i)
                {
                    MethodInfo method = methods[i];
                    if (method.Name == methodName)
                    {
                        uint id = instance.UniqueId;
                        string finalName = string.Format("{0}#{1}", invokableName, id);
                        invokable = Manager.RegisterInvokable(new InvokableInfo(finalName, id),
                            method, instance.Instance);
                    }
                }
            }
        }

        public AutoInvokable(string name, Delegate del, IBoundInstance instance = null)
        {
            if (CheckForManager())
            {
                uint id = instance != null ? instance.UniqueId : 0;
                string finalName = string.Format("{0}#{1}", name, id);
                invokable = Manager.RegisterInvokable(new InvokableInfo(finalName, id), del);
            }
        }

        private bool CheckForManager()
        {
            if (Manager == null)
            {
                throw new AutoInvokableException("No manager has been set. Set a manager through AutoInvokableBase.Manager before creating auto invokable instance.");
            }
            return true;
        }

        public void Dispose()
        {
            if(CheckForManager())
            {
                Manager.UnregisterInvokable(invokable);
            }
        }
    }

    public class AutoInvokableException : Exception, ISerializable
    {
        public AutoInvokableException(string message)
            : base(message)
        {
        }
    }
}
