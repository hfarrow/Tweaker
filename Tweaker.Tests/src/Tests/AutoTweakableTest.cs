using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ghostbit.Tweaker.AssemblyScanner;
using NUnit.Framework;

namespace Ghostbit.Tweaker.Core.Tests
{
	[TestFixture]
	public class AutoTweakableTest
	{
#pragma warning disable 0067,0649,0219
		public class TestClass : IDisposable
		{
			[Tweakable("TestClass.AutoInt"),
			Range(0, 10)]
			public Tweakable<int> AutoInt = new Tweakable<int>();

			private bool disposeTweakable;

			public TestClass(bool disposeTweakable)
			{
				this.disposeTweakable = disposeTweakable;
				AutoTweakable.Bind(this);
			}

			public void Dispose()
			{
				if (disposeTweakable && AutoInt != null)
				{
					AutoInt.Dispose();
				}
				else
				{
					AutoInt = null;
				}
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
			TestClass obj = new TestClass(false);
			ITweakable tweakable = tweaker.Tweakables.GetTweakable("TestClass.AutoInt#" + obj.AutoInt.UniqueId);
			Assert.IsNotNull(tweakable);
			tweakable.SetValue(5);
			Assert.AreEqual(5, obj.AutoInt.value);
			tweakable.SetValue(100);
			Assert.AreEqual(10, obj.AutoInt.value);

			// Direct access to the value should be allowed. Will ignore range constraint.
			obj.AutoInt.value = 100;
			Assert.AreEqual(100, obj.AutoInt.value);

			// Using the value setter should contrain according to the range.
			obj.AutoInt.SetTweakableValue(200);
			Assert.AreEqual(10, obj.AutoInt.value);

			GC.KeepAlive(obj);
		}

		[Test]
		public void CreateAutoTweakableAndDispose()
		{
			ITweakable tweakable = null;
			uint uniqueId = 0;
			using (TestClass obj = new TestClass(true))
			{
				uniqueId = obj.AutoInt.UniqueId;
				tweakable = tweaker.Tweakables.GetTweakable("TestClass.AutoInt#" + uniqueId);
				Assert.IsNotNull(tweakable);
				tweakable = null;
			}

			tweakable = tweaker.Tweakables.GetTweakable("TestClass.AutoInt#" + uniqueId);
			Assert.IsNull(tweakable);
		}

		[Test]
		public void CreateAutoTweakableAndFinalize()
		{
			ITweakable tweakable = null;
			TestClass obj = new TestClass(false);
			uint uniqueId = obj.AutoInt.UniqueId;

			tweakable = tweaker.Tweakables.GetTweakable("TestClass.AutoInt#" + uniqueId);
			Assert.IsNotNull(tweakable);
			tweakable = null;
			GC.KeepAlive(obj);
			obj = null;

#if UNITY_EDITOR
			// TODO: write integration test for this and keep checking back each frame to see if finalizer was called.
			Assert.Ignore("Finalize does not work in unity editor... objects no collected on same frame?");
#else
			uint count = 0;
			while (tweaker.Tweakables.GetTweakable("TestClass.AutoInt#" + uniqueId) != null)
			{
				GC.Collect();
				count++;
				// It is difficult to know when the finalizer will run (on a different thread) so wait for a while.
				// On the computer this test was written, this test would pass with less than 50 loops so 3000
				// should be more than enough?
				if (count > 3000)
				{
					Assert.Fail("Failed to finalize AutoTweakable");
				}
			}
			Assert.Pass();
#endif
		}
	}
}
