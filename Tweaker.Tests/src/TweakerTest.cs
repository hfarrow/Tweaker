using System.Collections;
using System.Reflection;
using System;

using NUnit.Framework;

using Ghostbit.Tweaker.Core;
using Ghostbit.Tweaker.AssemblyScanner;

#if UNITY_EDITOR
using UnityEngine;
using UnityTest;
#endif

namespace Ghostbit.Tweaker.Core.Tests
{
	[TestFixture]
	public class TweakerTest
	{
#pragma warning disable 0067,0649
		private class TweakerTestClass
		{
			public TweakerTestClass()
			{

			}

			public TweakerTestClass(int intParam, string stringParam)
			{

			}

			[Invokable("TestMethod")]
			public void TestMethod()
			{

			}
		}
#pragma warning restore 0067,0649

		private Scanner scanner;
		private Tweaker tweaker;
		private TweakerFactory factory;
		private uint nextExpectedId;

		[SetUp]
		public void Init()
		{
			scanner = new Scanner();
			tweaker = new Tweaker();
			var options = new TweakerOptions();
			options.Flags =
				TweakerOptionFlags.DoNotAutoScan |
				TweakerOptionFlags.ScanForInvokables |
				TweakerOptionFlags.ScanForTweakables |
				TweakerOptionFlags.ScanForWatchables;
			tweaker.Init(options, scanner);
			factory = new TweakerFactory(tweaker.Scanner);

			// The static BoundInstance<T>.nextId does not ever get reset so to find
			// out what the next id will be, create a dummy IBoundInstance
			// to get the current id
			TweakerTestClass dummy = new TweakerTestClass();
			IBoundInstance instance = BoundInstanceFactory.Create(dummy);
			nextExpectedId = instance.UniqueId + 1;
		}

		[Test]
		public void TweakerFactoryScanInstance()
		{
			var instance = factory.Create<TweakerTestClass>();
			Assert.IsNotNull(instance);
			Assert.IsNotNull(tweaker.Invokables.GetInvokable("TestMethod#" + nextExpectedId));
		}

		[Test]
		public void TweakerFactoryScanInstanceWithArgs()
		{
			var instance = factory.Create<TweakerTestClass>(10, "test");
			Assert.IsNotNull(tweaker.Invokables.GetInvokable("TestMethod#" + nextExpectedId));
			Assert.IsNotNull(instance);
		}

		[Test]
		public void ScanMultipleInstancesOfSameClass()
		{
			var instance_a = factory.Create<TweakerTestClass>();
			var instance_b = factory.Create<TweakerTestClass>();
			Assert.NotNull(instance_a);
			Assert.NotNull(instance_b);

			var invokables = tweaker.Invokables.GetInvokables(null);
			Assert.AreEqual(2, invokables.Count);
		}


		[Test]
		public void ScanSameInstanceTwice()
		{
			var instance_a = factory.Create<TweakerTestClass>();
			Assert.Throws<InstanceAlreadyRegisteredException>(delegate { scanner.ScanInstance(instance_a); });
		}
	}
}