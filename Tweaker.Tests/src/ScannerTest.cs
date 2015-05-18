using System.Collections;
using System.Reflection;
using System;

using NUnit.Framework;

#if UNITY_EDITOR
using UnityEngine;
using UnityTest;
#endif

using Tweaker.Core;
using System.Collections.Generic;
using Tweaker.AssemblyScanner;

namespace Tweaker.Core.Tests
{
	[TestFixture]
	public class ScannerTest
	{
#pragma warning disable 0067,0649
		[PlaceHolderAttribute(Name = "TestClass")]
		private class TestClass
		{
			[PlaceHolderAttribute(Name = "TestClass.MethodVoidVoid")]
			public static void MethodVoidVoid()
			{

			}

			[PlaceHolderAttribute(Name = "TestClass.ActionVoid")]
			public static event Action ActionVoid;

			[PlaceHolderAttribute(Name = "TestClass.IntProperty")]
			public static int IntProperty { get; set; }

			[PlaceHolderAttribute(Name = "TestClass.IntField")]
			public static int IntField;

			[PlaceHolderAttribute(Name = "TestClass.MethodVoidVoidInstance")]
			public void MethodVoidVoidInstance()
			{

			}

			[PlaceHolderAttribute(Name = "TestClass.ActionVoidInstance")]
			public event Action ActionVoidInstance;

			[PlaceHolderAttribute(Name = "TestClass.IntPropertyInstance")]
			public int IntPropertyInstance { get; set; }

			[PlaceHolderAttribute(Name = "TestClass.IntFieldInstance")]
			public int IntFieldInstance;

			[Tweakable("Tweakable")]
			public int Tweakable;

		}

		private class SubTestClass : TestClass
		{

		}
#pragma warning restore 0067,0649

		private ScanOptions MakeScanOptions()
		{
			var options = new ScanOptions();
			options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			return options;
		}

		[SetUp]
		public void Init()
		{

		}

		[Test]
		public void ScanForStaticMethodAttribute()
		{
			ScanForStaticMemberAttribute("TestClass.MethodVoidVoid");
		}

		[Test]
		public void ScanForStaticEventAttribute()
		{
			ScanForStaticMemberAttribute("TestClass.ActionVoid");
		}

		[Test]
		public void ScanForStaticPropertyAttribute()
		{
			ScanForStaticMemberAttribute("TestClass.IntProperty");
		}

		[Test]
		public void ScanForStaticFieldAttribute()
		{
			ScanForStaticMemberAttribute("TestClass.IntField");
		}

		private void ScanForStaticMemberAttribute(string name)
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new AttributeProcessor());
			scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Name == name && ((MemberInfo)a.result.Obj).ReflectedType == typeof(TestClass))
						found = true;
				};
			scanner.Scan(MakeScanOptions());
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForTypeAttribute()
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new AttributeProcessor());
			scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Name == "TestClass" &&
						((MemberInfo)a.result.Obj) == typeof(TestClass))
						found = true;
				};
			scanner.Scan(MakeScanOptions());
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForType()
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new TypeProcessor<TestClass>());
			scanner.GetResultProvider<TypeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.InputType == typeof(TestClass)
						&& a.result.ProcessedType == typeof(SubTestClass))
						found = true;
				};
			scanner.Scan(MakeScanOptions());
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForStaticMethod()
		{
			ScanForStaticMember<MethodInfo>("MethodVoidVoid");
		}

		[Test]
		public void ScanForStaticEvent()
		{
			ScanForStaticMember<EventInfo>("ActionVoid");
		}

		[Test]
		public void ScanForStaticProperty()
		{
			ScanForStaticMember<PropertyInfo>("IntProperty");
		}

		private void ScanForStaticMember<TInput>(string name)
			where TInput : MemberInfo
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new MemberProcessor<TInput>());
			scanner.GetResultProvider<MemberProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Type == typeof(TestClass) &&
						a.result.memberInfo.Name == name)
						found = true;
				};
			scanner.Scan(MakeScanOptions());
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForStaticMemberWithOptions()
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new MemberProcessor<MethodInfo>());
			scanner.GetResultProvider<MemberProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Type == typeof(TestClass) &&
						a.result.memberInfo.Name == "MethodVoidVoid")
						found = true;
				};
			ScanOptions options = MakeScanOptions();
			options.Members.NameRegex = @"MethodVoidVoid";
			scanner.Scan(options);
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForTypeWithOptions()
		{
			var found = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new TypeProcessor<TestClass>());
			scanner.GetResultProvider<TypeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.InputType == typeof(TestClass)
						 && a.result.ProcessedType == typeof(SubTestClass))
						found = true;
				};
			ScanOptions options = MakeScanOptions();
			options.Types.NameRegex = @"TestClass";
			options.Types.ScannableRefs = new Type[] { typeof(TestClass), typeof(SubTestClass) };
			scanner.Scan(options);
			Assert.IsTrue(found);
		}

		[Test]
		public void ScanForStaticAttributeWithOptions()
		{
			var foundType = false;
			var foundMember = false;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new AttributeProcessor());
			scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Obj is Type &&
						((Type)a.result.Obj).FullName == typeof(TestClass).FullName &&
						a.result.Name == "TestClass")
						foundType = true;

					else if (a.result.Obj is MemberInfo &&
							 a.result.Name == "TestClass.MethodVoidVoid")
						foundMember = true;
				};
			ScanOptions options = MakeScanOptions();
			options.Types.NameRegex = @"TestClass";
			options.Types.ScannableRefs = new Type[] { typeof(TestClass) };
			scanner.Scan(options);
			Assert.IsTrue(foundType);
			Assert.IsTrue(foundMember);
		}

		[Test]
		public void ProcessTypeOnlyOnce()
		{
			var count = 0;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new TypeProcessor<TestClass>());
			scanner.GetResultProvider<TypeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.InputType == typeof(TestClass)
						&& a.result.ProcessedType == typeof(SubTestClass))
						count++;
				};
			var options = MakeScanOptions();
			scanner.Scan(options);
			Assert.AreEqual(1, count);
			scanner.Scan(options);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void ProcessAttributeOnlyOnce()
		{
			var count = 0;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new AttributeProcessor());
			scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Name == "TestClass.MethodVoidVoid")
						count++;
				};
			var options = MakeScanOptions();
			scanner.Scan(options);
			Assert.AreEqual(1, count);
			scanner.Scan(options);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void ProcessMemberOnlyOnce()
		{
			var count = 0;
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new MemberProcessor<MethodInfo>());
			scanner.GetResultProvider<MemberProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Type == typeof(TestClass) &&
						a.result.memberInfo.Name == "MethodVoidVoid")
						count++;
				};
			var options = MakeScanOptions();
			scanner.Scan(options);
			Assert.AreEqual(1, count);
			scanner.Scan(options);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void ScanForInstanceMethodAttribute()
		{
			ScanForInstanceMember("TestClass.MethodVoidVoidInstance");
		}

		[Test]
		public void ScanForInstanceEventAttribute()
		{
			ScanForInstanceMember("TestClass.ActionVoidInstance");
		}

		[Test]
		public void ScanForInstancePropertyAttribute()
		{
			ScanForInstanceMember("TestClass.IntPropertyInstance");
		}

		[Test]
		public void ScanForInstanceFieldAttribute()
		{
			ScanForInstanceMember("TestClass.IntFieldInstance");
		}

		[Test]
		public void ScanMultipleInstancesAndValidateIdIncremented()
		{
			var name = "Tweakable";
			var found = false;
			var instance = new TestClass();
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new TweakableProcessor());
			uint currentId = 0;
			uint previousId = 0;
			scanner.GetResultProvider<ITweakable>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Name.StartsWith(name) &&
						a.result.Name.Contains("#"))
					{
						currentId = uint.Parse(a.result.Name.Split('#')[1]);
						found = true;
					}
				};

			scanner.ScanInstance(instance);
			Assert.IsTrue(found);
			previousId = currentId;
			found = false;

			scanner.ScanInstance(instance);
			Assert.IsTrue(found);
			Assert.AreEqual(currentId, previousId + 1);
		}

		private void ScanForInstanceMember(string name)
		{
			var found = false;
			var instance = new TestClass();
			Scanner scanner = new Scanner();
			scanner.AddProcessor(new AttributeProcessor());
			scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
				(s, a) =>
				{
					if (a.result.Name == name &&
						((MemberInfo)a.result.Obj).ReflectedType == typeof(TestClass) &&
						a.result.Instance == instance)
					{
						found = true;
					}
				};

			scanner.ScanInstance(instance);
			Assert.IsTrue(found);
		}
	}
}