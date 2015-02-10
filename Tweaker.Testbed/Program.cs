using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Ghostbit.Tweaker.Core;
using Ghostbit.Tweaker.AssemblyScanner;
using System.Reflection;


namespace Ghostbit.Tweaker.Core.Testbed
{
    class Program
    {
        public class TestClass
        {
            [Invokable("TestMethod")]
            public void TestMethod()
            {
                Debug.WriteLine("TestMethod");
            }

            [Invokable("TestMethodStatic")]
            public static void TestMethodStatic()
            {
                Debug.WriteLine("TestMethodStatic");
            }
        }

        static void Main(string[] args)
        {

            //////////////////////////////////////////
            //var instance = new TestClass();
            //Scanner.Global.ScanInstance(instance);
            //tweaker.Invokables.Invoke("TestMethod", null);

            var testClass = new TestClass();
            var name = "TestMethodStatic";
            var assembly = typeof(TestClass).Assembly;
            var methodInfo = typeof(TestClass).GetMethod("TestMethodVoidVoid");
            var invokable = new InvokableMethod(new InvokableInfo(name), methodInfo, new WeakReference<object>(testClass));

            //Assert.AreEqual(name, invokable.Name);
            //Assert.AreEqual(assembly, invokable.Assembly);
            //Assert.AreEqual(methodInfo, invokable.MethodInfo);
            //Assert.AreEqual(testClass, invokable.Instance);

        }
    }
}
