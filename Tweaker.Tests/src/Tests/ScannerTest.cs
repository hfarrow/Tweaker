using System.Collections;
using System.Reflection;
using System;

using NUnit.Framework;

#if UNITY_EDITOR
using UnityEngine;
using UnityTest;
#endif

using Simtastic.Tweaker.Core;
using System.Collections.Generic;

namespace Simtastic.Tweaker.Core.Tests
{
    [TestFixture]
    public class ScannerTest
    {
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
        }

        private TestClass testClass;

        private ScanOptions MakeScanOptions()
        {
            var options = new ScanOptions();
            options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
            return options;
        }

        [SetUp]
        public void Init()
        {
            testClass = new TestClass();
        }

        [Test]
        public void ScanForStaticMethodAttribute()
        {
            var found = false;
            Scanner scanner = new Scanner();
            scanner.AddProcessor(new AttributeProcessor());
            scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
                (s, a) =>
                {
                    if (a.result.Name == "TestClass.MethodVoidVoid" && ((MemberInfo)a.result.Obj).ReflectedType == typeof(TestClass))
                        found = true;
                };
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForStaticEventAttribute()
        {
            var found = false;
            Scanner scanner = new Scanner();
            scanner.AddProcessor(new AttributeProcessor());
            scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
                (s, a) =>
                {
                    if (a.result.Name == "TestClass.ActionVoid" && ((MemberInfo)a.result.Obj).ReflectedType == typeof(TestClass))
                        found = true;
                };
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForStaticPropertyAttribute()
        {
            var found = false;
            Scanner scanner = new Scanner();
            scanner.AddProcessor(new AttributeProcessor());
            scanner.GetResultProvider<AttributeProcessorResult>().ResultProvided +=
                (s, a) =>
                {
                    if (a.result.Name == "TestClass.IntProperty" && ((MemberInfo)a.result.Obj).ReflectedType == typeof(TestClass))
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
                    if (a.result.Name == "TestClass" && ((MemberInfo)a.result.Obj) == typeof(TestClass))
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
                    if (a.result.Type == typeof(TestClass))
                        found = true;
                };
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForStaticMember()
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
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForStaticEvent()
        {
            var found = false;
            Scanner scanner = new Scanner();
            scanner.AddProcessor(new MemberProcessor<EventInfo>());
            scanner.GetResultProvider<MemberProcessorResult>().ResultProvided +=
                (s, a) =>
                {
                    if (a.result.Type == typeof(TestClass) &&
                        a.result.memberInfo.Name == "ActionVoid")
                        found = true;
                };
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForStaticProperty()
        {
            var found = false;
            Scanner scanner = new Scanner();
            scanner.AddProcessor(new MemberProcessor<PropertyInfo>());
            scanner.GetResultProvider<MemberProcessorResult>().ResultProvided +=
                (s, a) =>
                {
                    if (a.result.Type == typeof(TestClass) &&
                        a.result.memberInfo.Name == "IntProperty")
                        found = true;
                };
            scanner.Scan(MakeScanOptions());
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForMemberWithOptions()
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
                    if (a.result.Type == typeof(TestClass))
                        found = true;
                };
            ScanOptions options = MakeScanOptions();
            options.Types.NameRegex = @"TestClass";
            options.Types.ScannableRefs = new Type[] { typeof(TestClass) };
            scanner.Scan(options);
            Assert.IsTrue(found);
        }

        [Test]
        public void ScanForAttributeWithOptions()
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
                    if (a.result.Type == typeof(TestClass))
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

            Tweaker.Init(TweakerOptions.ScanEverything | TweakerOptions.Tweakables);
            var tweakables = Tweaker.Tweakables.GetTweakables(null);
        }
    }
}