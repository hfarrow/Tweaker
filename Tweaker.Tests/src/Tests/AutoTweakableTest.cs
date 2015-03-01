using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ghostbit.Tweaker.AssemblyScanner;
using NUnit.Framework;

namespace Ghostbit.Tweaker.Core.Tests
{
    [TestFixture]
    public class AutoTweakableTest
    {
#pragma warning disable 0067,0649,0219
        public class TestClass
        {
            [Tweakable("TestClass.AutoInt"),
            @Range(0, 10)]
            public Tweakable<int> AutoInt = new Tweakable<int>();

            public TestClass()
            {
                AutoTweakable.Bind(this);
            }
        }
#pragma warning restore 0067,0649,0219
        Tweaker tweaker;

        [SetUp]
        public void Init()
        {
            IScanner scanner = new Scanner();
            tweaker = new Tweaker();
            tweaker.Init(null, scanner);

            AutoTweakable.Manager = tweaker.Tweakables;
        }

        [Test]
        public void CreateAutoTweakableWithRangeAndValidate()
        {
            TestClass obj = new TestClass();
            ITweakable tweakable = tweaker.Tweakables.GetTweakable(new SearchOptions("TestClass.AutoInt#"));
            Assert.IsNotNull(tweakable);
            tweakable.SetValue(5);
            Assert.AreEqual(5, obj.AutoInt.value);
            tweakable.SetValue(100);
            Assert.AreEqual(10, obj.AutoInt.value);

            // Direct access to the value should be allowed. Will ignore range constraint.
            obj.AutoInt.value = 100;
            Assert.AreEqual(100, obj.AutoInt.value);

            // Using the value setter should contrain according to the range.
            obj.AutoInt.Value = 200;
            Assert.AreEqual(10, obj.AutoInt.value);
        }
    }
}
