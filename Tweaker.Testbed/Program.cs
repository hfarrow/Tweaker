using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ghostbit.Tweaker.Core;
using System.Threading;
using Ghostbit.Tweaker.AssemblyScanner;
using System.Diagnostics;

namespace Ghostbit.Tweaker.Core.Testbed
{
    public class TestClass
    {
        [Invokable("TestClass")]
        public void TestMethod()
        {
            Debug.WriteLine("TestMethod");
        }
    }

    class Program
    {
        static Tweaker tweaker;
        static void Main(string[] args)
        {
            tweaker = new Tweaker();
            tweaker.Init();

            var instance = new TestClass();
            Scanner.Global.ScanInstance(instance);
        }
    }
}
