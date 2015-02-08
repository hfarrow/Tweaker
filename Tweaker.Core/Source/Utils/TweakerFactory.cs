using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core
{
    public class TweakerFactory : ITweakerFactory
    {
        private Scanner scanner;

        public TweakerFactory(Scanner scanner)
        {
            this.scanner = scanner;
        }

        public T Create<T>(params object[] constructorArgs)
        {
            Type type = typeof(T);
            Type[] argTypes = new Type[constructorArgs.Length];
            for(int i = 0; i < constructorArgs.Length; ++i)
            {
                argTypes[i] = constructorArgs[i].GetType();
            }

            ConstructorInfo constructor = type.GetConstructor(argTypes);
            if(constructor == null)
            {
                throw new Exception("Could not find constructor that matches input arguments.");
            }

            T instance = (T)Activator.CreateInstance(type, constructorArgs);
            if(instance != null)
            {
                scanner.ScanInstance(instance);
            }
            return instance;
        }
    }
}
