using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweaker.AssemblyScanner;
using NUnit.Framework;

namespace Tweaker.Core.Tests
{
	[TestFixture]
	public class AutoScanTest
	{
#pragma warning disable 0067,0649
		private class TestClass
		{
			[Tweakable("TestClass.IntField")]
			public int IntField;

			AutoInvokable autoInvokable = new AutoInvokable("TestClass.Invokable",
				new Action<TestClass, int>((TestClass obj, int i) =>
					{
						obj.IntField = i;
					}));

			AutoInvokable autoInvokable2;

			public TestClass()
			{
				Assert.NotNull(autoInvokable);

				autoInvokable2 = new AutoInvokable("TestClass.Invokable2",
					new Action<TestClass, int>((TestClass obj, int i)
						=>
						{
							obj.IntField = i;
						}), BoundInstanceFactory.Create(this));
				Assert.NotNull(autoInvokable2);
			}
		}

		private class TestClass2
		{
			public int IntField;
			AutoInvokable autoInvokable;

			public TestClass2()
			{
				autoInvokable = new AutoInvokable("TestClass2.Invokable",
					new Action<int>((int i)
						=>
					{
						IntField = i;
					}), BoundInstanceFactory.Create(this));
				Assert.NotNull(autoInvokable);
			}
		}

		private class TestClass3
		{
			public int IntField;
			AutoInvokable autoInvokable;

			public void InvokableMethod()
			{
				IntField = 13;
			}

			public TestClass3()
			{
				autoInvokable = new AutoInvokable("TestClass3.Invokable",
					"InvokableMethod", BoundInstanceFactory.Create(this));
				Assert.NotNull(autoInvokable);
			}
		}
#pragma warning restore 0067,0649

		AutoScan<TestClass> testClass;
		Tweaker tweaker;

		[SetUp]
		public void Init()
		{
			IScanner scanner = new Scanner();
			tweaker = new Tweaker();
			tweaker.Init(null, scanner);

			AutoScanBase.Scanner = scanner;
			AutoTweakable.Manager = tweaker.Tweakables;
			AutoInvokableBase.Manager = tweaker.Invokables;
		}

		[Test]
		public void CreateAutoScanObjectAndGetTweakable()
		{
			testClass = new AutoScan<TestClass>();
			ITweakable tweakable = tweaker.Tweakables.GetTweakable(new SearchOptions("TestClass.IntField#"));
			Assert.IsNotNull(tweakable);
			tweakable.SetValue(10);
			Assert.AreEqual(10, testClass.value.IntField);
		}

		[Test]
		public void CreateAutoInvokableByDelegateAndGetInvokable()
		{
			TestClass obj = new TestClass();
			IInvokable invokable = tweaker.Invokables.GetInvokable("TestClass.Invokable#0");
			Assert.IsNotNull(invokable);
			invokable.Invoke(obj, 11);
			Assert.AreEqual(11, obj.IntField);

			TestClass2 obj2 = new TestClass2();
			invokable = tweaker.Invokables.GetInvokable(new SearchOptions("TestClass2.Invokable#"));
			Assert.IsNotNull(invokable);
			invokable.Invoke(12);
			Assert.AreEqual(12, obj2.IntField);
		}

		[Test]
		public void CreateAutoInvokableByMethodNameAndGetInvokable()
		{
			TestClass3 obj = new TestClass3();
			IInvokable invokable = tweaker.Invokables.GetInvokable(new SearchOptions("TestClass3.Invokable#"));
			Assert.IsNotNull(invokable);
			invokable.Invoke();
			Assert.AreEqual(13, obj.IntField);
		}
	}
}
