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
    public class AutoTweakable : IDisposable
    {
        public static ITweakableManager Manager { get; set; }
        private static AutoTweakableProcessor s_processor;

        public ITweakable tweakable;       

        static AutoTweakable()
        {
            s_processor = new AutoTweakableProcessor();
        }

        public static void Bind<TContainer>(TContainer container)
        {
            if (CheckForManager())
            {
                IScanner scanner = new Scanner();
                scanner.AddProcessor(s_processor);
                var provider = scanner.GetResultProvider<AutoTweakableResult>();
                provider.ResultProvided += OnResultProvided;
                scanner.ScanInstance(container);
                provider.ResultProvided -= OnResultProvided;
            }
        }

        private static void OnResultProvided(object sender, ScanResultArgs<AutoTweakableResult> e)
        {
            if (CheckForManager())
            {
                ITweakable tweakable = e.result.tweakble;
                Manager.RegisterTweakable(tweakable);
                e.result.autoTweakable.tweakable = tweakable;
            }
        }

        public void Dispose()
        {
            if (CheckForManager())
            {
                Manager.UnregisterTweakable(tweakable);
            }
        }

        private static bool CheckForManager()
        {
            if (Manager == null)
            {
                throw new AutoTweakableException("No manager has been set. Set a manager through AutoTweakableBase.Manager before creating auto tweakable instance.");
            }
            return true;
        }
    }

    public class Tweakable<T> : AutoTweakable
    {
        public T value;

        /// <summary>
        /// Set the value through an ITweakable instance. This will
        /// enforce any constraints such as a Range. To ignore constaints,
        /// set the 'value' field directly.
        /// </summary>
        public T Value
        {
            set { tweakable.SetValue(value); }
        }

        public Tweakable(T value)
        {
            this.value = value;
        }

        public Tweakable() :
            this(default(T))
        {

        }
    }

    public class AutoTweakableException : Exception, ISerializable
    {
        public AutoTweakableException(string message)
            : base(message)
        {
        }
    }
}
