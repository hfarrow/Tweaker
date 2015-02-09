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

        static Tweaker tweaker;
        static void Main(string[] args)
        {

            //////////////////////////////////////////
            //var instance = new TestClass();
            //Scanner.Global.ScanInstance(instance);
            //tweaker.Invokables.Invoke("TestMethod", null);

            Scanner scanner = new Scanner();
            Tweaker tweaker = new Tweaker();
            var options = new TweakerOptions();
            options.Flags =
                TweakerOptionFlags.DoNotScan |
                TweakerOptionFlags.ScanForInvokables |
                TweakerOptionFlags.ScanForTweakables |
                TweakerOptionFlags.ScanForWatchables;
            tweaker.Init(options, scanner);
            ITweakerFactory factory = new TweakerFactory(tweaker.Scanner);

            TestClass instance = factory.Create<TestClass>();
            IInvokable invokable = tweaker.Invokables.GetInvokable("TestMethod#1");

        }
    }
}
