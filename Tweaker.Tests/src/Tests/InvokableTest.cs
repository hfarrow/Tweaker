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
    public class InvokableTest
    {
        private class TestClass
        {
            public bool didRunMethod = false;
            public static bool didRunStaticMethod = false;
            public void Reset() { didRunMethod = false; }
            public static void ResetStatic() { didRunStaticMethod = false; }

            public void TestMethodVoidVoid()
            {
                didRunMethod = true;
            }

            public int TestMethodIntObject(object arg)
            {
                didRunMethod = true;
                return 99;
            }

            [Invokable("TestMethodStaticVoidVoid")]
            public static void TestMethodStaticVoidVoid()
            {
                didRunStaticMethod = true;
            }

            public event Action TestEventVoidVoid;

            [Invokable("TestEventStaticVoidVoid")]
            public static event Action TestEventStaticVoidVoid;
        }

        private TestClass testClass;

        private IInvokable CreateInvokableMethod(string methodName, object instance)
        {
            var name = "TestClass." + methodName;
            var methodInfo = instance.GetType().GetMethod(methodName);
            var assembly = methodInfo.GetType().Assembly;
            return new InvokableMethod(new InvokableInfo(name), methodInfo, instance);
        }

        private IInvokable CreateInvokableDelegate(string methodName, Delegate del)
        {
            var name = "TestClass." + methodName;
            var assembly = del.Method.GetType().Assembly;
            return new InvokableMethod(new InvokableInfo(name), del);
        }

        private void VerifyInvoke(Func<string, object, IInvokable> factory)
        {
            VerifyInvoke((s, o, d) => factory(s, o));
        }

        private void VerifyInvoke(Func<string, Delegate, IInvokable> factory)
        {
            VerifyInvoke((s, o, d) => factory(s, d));
        }

        private void VerifyInvoke(Func<string, object, Delegate, IInvokable> factory)
        {
            var testClass = new TestClass();

            var invokableVoidVoid = factory("TestMethodVoidVoid", testClass, new Action(testClass.TestMethodVoidVoid));
            Assert.IsFalse(testClass.didRunMethod);
            Assert.IsNull(invokableVoidVoid.Invoke(null));
            Assert.IsTrue(testClass.didRunMethod);
            testClass.Reset();

            var invokableStaticVoidVoid = factory("TestMethodStaticVoidVoid", testClass, new Action(TestClass.TestMethodStaticVoidVoid));
            Assert.IsFalse(TestClass.didRunStaticMethod);
            Assert.IsNull(invokableStaticVoidVoid.Invoke(null));
            Assert.IsTrue(TestClass.didRunStaticMethod);
            TestClass.ResetStatic();

            var invokableIntObject = factory("TestMethodIntObject", testClass, new Func<object, int>(testClass.TestMethodIntObject));
            Assert.IsFalse(testClass.didRunMethod);
            Assert.AreEqual(99, invokableIntObject.Invoke(new object[] { "Test Object" }));
            Assert.IsTrue(testClass.didRunMethod);
            testClass.Reset();
        }

        [SetUp]
        public void Init()
        {
            testClass = new TestClass();
        }

        [Test]
        public void BindInvokableMethodAndVerifyProperties()
        {
            var testClass = new TestClass();
            var name = "TestClass.VoidVoid";
            var assembly = typeof(TestClass).Assembly;
            var methodInfo = typeof(TestClass).GetMethod("TestMethodVoidVoid");
            var invokable = new InvokableMethod(new InvokableInfo(name), methodInfo, testClass);

            Assert.AreEqual(name, invokable.Name);
            Assert.AreEqual(assembly, invokable.Assembly);
            Assert.AreEqual(methodInfo, invokable.MethodInfo);
            Assert.AreEqual(testClass, invokable.Instance);
        }

        [Test]
        public void BindInvokableDelegateAndVerifyProperties()
        {
            var testClass = new TestClass();
            var name = "TestClass.VoidVoid";
            var assembly = typeof(TestClass).Assembly;
            Delegate del = new Action(testClass.TestMethodVoidVoid);
            var methodInfo = del.Method;
            var invokable = new InvokableMethod(new InvokableInfo(name), del);

            Assert.AreEqual(name, invokable.Name);
            Assert.AreEqual(assembly, invokable.Assembly);
            Assert.AreEqual(methodInfo, invokable.MethodInfo);
            Assert.AreEqual(testClass, invokable.Instance);
        }

        [Test]
        public void BindInvokableMethodAndInvoke()
        {
            VerifyInvoke(new Func<string, object, IInvokable>(CreateInvokableMethod));
        }

        [Test]
        public void BindInvokableDelegateAndInvoke()
        {
            VerifyInvoke(new Func<string, Delegate, IInvokable>(CreateInvokableDelegate));
        }

        [Test]
        public void BindInvokableEventAndVerifyProperties()
        {
            var testClass = new TestClass();
            
            var name = "TestEventVoidVoid";
            var assembly = testClass.GetType().Assembly;
            var fieldInfo = testClass.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fieldInfo);
            var invokable = new InvokableEvent(new InvokableInfo(name), fieldInfo, testClass);

            Assert.AreEqual(name, invokable.Name);
            Assert.AreEqual(assembly, invokable.Assembly);
            Assert.AreEqual(fieldInfo, invokable.FieldInfo);
            Assert.AreEqual(testClass, invokable.Instance);
        }

        [Test]
        public void BindInvokableEventAndInvoke()
        {
            var testClass = new TestClass();
            bool lambdaDidRun = false;
            testClass.TestEventVoidVoid += () => { lambdaDidRun = true; };

            var name = "TestEventVoidVoid";
            var assembly = testClass.GetType().Assembly;
            var fieldInfo = testClass.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var invokable = new InvokableEvent(new InvokableInfo(name), fieldInfo, testClass);

            invokable.Invoke(null);
            Assert.IsTrue(lambdaDidRun);
        }

        [Test]
        public void ScanAndAddToManager()
        {
            Scanner scanner = new Scanner();
            ScanOptions options = new ScanOptions();
            options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
            options.Types.ScannableRefs = new Type[] { typeof(TestClass) };

            InvokableManager manager = new InvokableManager(scanner);
            scanner.Scan(options);

            var invokables = manager.GetInvokables(null);
            Assert.AreEqual(2, invokables.Count);

            var invokable = manager.GetInvokable(new SearchOptions("TestMethodStaticVoidVoid"));
            Assert.IsNotNull(invokable);

            invokable = manager.GetInvokable(new SearchOptions("TestEventStaticVoidVoid"));
            Assert.IsNotNull(invokable);

            invokable = manager.GetInvokable("TestMethodStaticVoidVoid");
            Assert.IsNotNull(invokable);
        }
    }
}