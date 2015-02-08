using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Ghostbit.Tweaker.Core;
using Ghostbit.Tweaker.AssemblyScanner;


namespace Ghostbit.Tweaker.Core.Testbed
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

    class Program
    {
        static Tweaker tweaker;
        static void Main(string[] args)
        {
            tweaker = new Tweaker();
            tweaker.Init();

            //var instance = new TestClass();
            //Scanner.Global.ScanInstance(instance);
            //tweaker.Invokables.Invoke("TestMethod", null);


        }
    }
}
