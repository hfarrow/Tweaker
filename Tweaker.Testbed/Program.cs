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
#pragma warning disable 0067,0649,0219
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

            [Tweakable("InstanceProperty")]
            public int InstanceProperty { get; set; }
        }

        public class AutoIvokableTest : IDisposable
        {
            private AutoInvokable invokable;
            private void Invokable()
            {

            }

            private AutoInvokable invokableDel = new AutoInvokable("InvokableDel",
                new Action(delegate {
                    int i = 0;
                }
                ));

            public AutoIvokableTest()
            {
                invokable = new AutoInvokable("Invokable", "Invokable", BoundInstanceFactory.Create(this));

                //invokableDel = new AutoInvokable("InvokableDel",
                //     new Action(
                //         delegate
                //         {
                //             int i = 0; 
                //         }
                //         ),
                //    boundSelf);
            }

            public void Dispose()
            {
                invokable.Dispose();
                invokableDel.Dispose();
            }
        }

        public class AutoTweakableTest : IDisposable
        {
            [Tweakable("AutoTweakableTest.int"),
            Range(0, 10)]
            private Tweakable<int> tweakable = new Tweakable<int>();

            public AutoTweakableTest()
            {
                AutoTweakable.Bind(this);
            }

            ~AutoTweakableTest()
            {
                
            }

            public void Dispose()
            {
                tweakable = null;
            }
        }
#pragma warning restore 0067,0649,0219

        //private static AutoScan<TestClass> testClass;

        static void Main(string[] args)
        {
            Tweaker tweaker = new Tweaker();
            tweaker.Init();
            IScanner scanner = tweaker.Scanner;
            AutoTweakable.Manager = tweaker.Tweakables;

            //AutoTweakableTest test = new AutoTweakableTest();
            //var tweakable = tweaker.Tweakables.GetTweakable("AutoTweakableTest.int#1");
            //tweakable.SetValue(9);

            //while (true)
            //{
            //    ITweakable tweakable = null;
            //    using (AutoTweakableTest obj = new AutoTweakableTest())
            //    {
            //        tweakable = tweaker.Tweakables.GetTweakable(new SearchOptions("AutoTweakableTest.int#"));
            //        tweakable = null;
            //    }

            //    GC.Collect(100, GCCollectionMode.Forced, true);
            //}

            using(AutoTweakableTest obj = new AutoTweakableTest())
            {

            }

            while(true)
            {
                GC.Collect();
            }
        }

        //static void Main(string[] args)
        //{
        //    Tweaker tweaker = new Tweaker();
        //    tweaker.Init();
        //    IScanner scanner = tweaker.Scanner;
        //    AutoScanBase.Scanner = scanner;
        //    AutoInvokableBase.Manager = tweaker.Invokables;

        //    testClass = new AutoScan<TestClass>();
        //    IInvokable invokable = null;
        //    using (AutoIvokableTest autoInvokableTest = new AutoIvokableTest())
        //    {
        //        invokable = tweaker.Invokables.GetInvokable("Invokable");
        //        invokable.Invoke();

        //        invokable = tweaker.Invokables.GetInvokable("InvokableDel");
        //        invokable.Invoke();
        //    }

        //    invokable = tweaker.Invokables.GetInvokable("Invokable");

        //    //////////////////////////////////////////
        //    //IScanner scanner = Scanner.Global;

        //    //var name = "InstanceProperty";
        //    //uint currentId = 1;
        //    //var found = false;
        //    //scanner.AddProcessor(new TweakableProcessor());
        //    //scanner.GetResultProvider<ITweakable>().ResultProvided +=
        //    //    (s, a) =>
        //    //    {
        //    //        if (a.result.Name.StartsWith(name) &&
        //    //            a.result.Name.EndsWith("#" + currentId))
        //    //        {
        //    //            found = true;
        //    //        }
        //    //    };

        //    //TestClass instance = new TestClass();
        //    //scanner.ScanInstance(instance);
        //    //currentId++;
        //    //scanner.ScanInstance(instance);

        //    //var tweakable = tweaker.Tweakables.GetTweakable("TestPropertyStatic");
        //    //var t1 = tweakable.IsValid;
        //    //var t2 = tweakable.WeakInstance;
        //    //var t3 = tweakable.StrongInstance;

        //    //Assert.AreEqual(name, invokable.Name);
        //    //Assert.AreEqual(assembly, invokable.Assembly);
        //    //Assert.AreEqual(methodInfo, invokable.MethodInfo);
        //    //Assert.AreEqual(testClass, invokable.Instance);

        //}
    }
}
