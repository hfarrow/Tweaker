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
#pragma warning disable 0067,0649
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

            [Tweakable("TestFieldStatic")]
            public static int TestFieldStatic;

            [Tweakable("TestPropertyStatic")]
            public static int TestPropertyStatic { get; set; }
        }
#pragma warning restore 0067,0649

        static void Main(string[] args)
        {

            //////////////////////////////////////////
            Tweaker tweaker = new Tweaker();
            tweaker.Init();

            var tweakable = tweaker.Tweakables.GetTweakable("TestPropertyStatic");
            var t1 = tweakable.IsValid;
            var t2 = tweakable.WeakInstance;
            var t3 = tweakable.StrongInstance;

            //Assert.AreEqual(name, invokable.Name);
            //Assert.AreEqual(assembly, invokable.Assembly);
            //Assert.AreEqual(methodInfo, invokable.MethodInfo);
            //Assert.AreEqual(testClass, invokable.Instance);

        }
    }
}
