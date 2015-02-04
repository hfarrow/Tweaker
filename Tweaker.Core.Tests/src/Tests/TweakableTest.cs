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
    public class TweakableTest
    {
        private class TestClass
        {
            [Tweakable("IntProperty")]
            public static int IntProperty { get; set; }

            [Tweakable("IntPropertyRange"), Core.Range(0, 100)]
            public static int IntPropertyRange { get; set; }

            [Tweakable("IntPropertyToggle"),
             NamedToggleValue("zero", 0, 0),
             NamedToggleValue("eleven", 11, 1),
             NamedToggleValue("hundred", 100, 2)]
            public static int IntPropertyToggle { get; set; }

            [Tweakable("intField")]
            public static int intField;

            [Tweakable("intFieldRange"), Core.Range(0, 100)]
            public static int intFieldRange;

            [Tweakable("intFieldToggle"),
             NamedToggleValue("zero", 0, 0),
             NamedToggleValue("eleven", 11, 1),
             NamedToggleValue("hundred", 100, 2)]
            public static int intFieldToggle;
        }

        private TestClass testClass;

        [SetUp]
        public void Init()
        {
            testClass = new TestClass();
        }

        public const int MIN_VALUE = 0;
        public const int MAX_VALUE = 100;

        public void ValidateBaseTweakable(ITweakable tweakable, Func<int> getter)
        {
            Assert.IsNotNull(tweakable);
            tweakable.SetValue(11);
            Assert.AreEqual(11, getter());
            Assert.AreEqual(11, tweakable.GetValue());
        }

        public void ValidateTweakableRange(ITweakable tweakable, Func<int> getter)
        {
            ValidateBaseTweakable(tweakable, getter);

            // max
            tweakable.SetValue(MAX_VALUE);
            Assert.AreEqual(MAX_VALUE, getter());
            Assert.AreEqual(MAX_VALUE, tweakable.GetValue());

            // max + 1
            tweakable.SetValue(MAX_VALUE + 1);
            Assert.AreEqual(MAX_VALUE, getter());
            Assert.AreEqual(MAX_VALUE, tweakable.GetValue());

            // min
            tweakable.SetValue(MIN_VALUE);
            Assert.AreEqual(MIN_VALUE, getter());
            Assert.AreEqual(MIN_VALUE, tweakable.GetValue());

            // min - 1
            tweakable.SetValue(MIN_VALUE - 1);
            Assert.AreEqual(MIN_VALUE, getter());
            Assert.AreEqual(MIN_VALUE, tweakable.GetValue());
        }

        public void ValidateTweakableToggle(TweakableToggle<int> tweakable, Func<int> getter)
        {
            ValidateBaseTweakable(tweakable, getter);

            Assert.AreEqual(-1, tweakable.CurrentIndex);
            Assert.AreEqual(0, tweakable.NextValue());
            Assert.AreEqual(0, tweakable.CurrentIndex);
            Assert.AreEqual(0, getter());
            Assert.AreEqual("zero", tweakable.GetValueName());

            // toggle forward all values
            Assert.AreEqual(11, tweakable.NextValue());
            Assert.AreEqual(1, tweakable.CurrentIndex);
            Assert.AreEqual(11, getter());
            Assert.AreEqual("eleven", tweakable.GetValueName());

            Assert.AreEqual(100, tweakable.NextValue());
            Assert.AreEqual(2, tweakable.CurrentIndex);
            Assert.AreEqual(100, getter());
            Assert.AreEqual("hundred", tweakable.GetValueName());

            // wrap
            Assert.AreEqual(0, tweakable.NextValue());
            Assert.AreEqual(0, tweakable.CurrentIndex);

            // toggle backwards all values
            // wrap first this time;
            Assert.AreEqual(100, tweakable.PreviousValue());
            Assert.AreEqual(2, tweakable.CurrentIndex);

            Assert.AreEqual(11, tweakable.PreviousValue());
            Assert.AreEqual(1, tweakable.CurrentIndex);
            Assert.AreEqual(0, tweakable.PreviousValue());
            Assert.AreEqual(0, tweakable.CurrentIndex);

            // GetIndexOfValue
            Assert.AreEqual(0, tweakable.GetIndexOfValue(0));
            Assert.AreEqual(1, tweakable.GetIndexOfValue(11));
            Assert.AreEqual(2, tweakable.GetIndexOfValue(100));

            // GetNameByIndex
            Assert.AreEqual("zero", tweakable.GetNameByIndex(0));
            Assert.AreEqual("eleven", tweakable.GetNameByIndex(1));
            Assert.AreEqual("hundred", tweakable.GetNameByIndex(2));

            // GetNameByValue
            Assert.AreEqual("zero", tweakable.GetNameByValue(0));
            Assert.AreEqual("eleven", tweakable.GetNameByValue(11));
            Assert.AreEqual("hundred", tweakable.GetNameByValue(100));

            // SetValueByName
            tweakable.SetValueByName("eleven");
            Assert.AreEqual(11, getter());
            Assert.AreEqual(11, tweakable.GetValue());
            Assert.AreEqual("eleven", tweakable.GetValueName());

        }

        [Test]
        public void MakeBaseTweakableFromFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntProperty", BindingFlags.Static | BindingFlags.Public);
            FieldInfo field = typeof(TestClass).GetField("intField", BindingFlags.Static | BindingFlags.Public);

            TweakableInfo<int> info = new TweakableInfo<int>("IntProperty", null, null, null);
            ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, property, null);
            ValidateBaseTweakable(tweakable, () => { return TestClass.IntProperty; });

            info = new TweakableInfo<int>("intField", null, null, null);
            tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, field, null);
            ValidateBaseTweakable(tweakable, () => { return TestClass.intField; });
        }

        [Test]
        public void MakeTweakableRangeFromFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntProperty", BindingFlags.Static | BindingFlags.Public);
            FieldInfo field = typeof(TestClass).GetField("intField", BindingFlags.Static | BindingFlags.Public);

            var range = new TweakableInfo<int>.TweakableRange(0, 100);
            TweakableInfo<int> info = new TweakableInfo<int>("IntProperty", range, null, null);
            ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, property, null);
            ValidateTweakableRange(tweakable, () => { return TestClass.IntProperty; });


            info = new TweakableInfo<int>("intField", range, null, null);
            tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, field, null);
            ValidateTweakableRange(tweakable, () => { return TestClass.intField; });
        }

        [Test]
        public void MakeTweakableToggleFromFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntProperty", BindingFlags.Static | BindingFlags.Public);
            FieldInfo field = typeof(TestClass).GetField("intField", BindingFlags.Static | BindingFlags.Public);

            var toggleValues = new TweakableInfo<int>.TweakableNamedToggleValue[]
                {
                    new TweakableInfo<int>.TweakableNamedToggleValue("zero", 0),
                    new TweakableInfo<int>.TweakableNamedToggleValue("eleven", 11),
                    new TweakableInfo<int>.TweakableNamedToggleValue("hundred", 100),
                };
            TweakableInfo<int> info = new TweakableInfo<int>("IntProperty", null, null, toggleValues);
            ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, property, null);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.IntProperty; });


            info = new TweakableInfo<int>("intField", null, null, toggleValues);
            tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, field, null);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.intField; });
        }

        [Test]
        public void MakeBaseTweakableFromReflectionFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntProperty", BindingFlags.Static | BindingFlags.Public);
            Tweakable propertyAttribute = property.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;
            FieldInfo field = typeof(TestClass).GetField("intField", BindingFlags.Static | BindingFlags.Public);
            Tweakable fieldAttribute = field.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;

            ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
            ValidateBaseTweakable(tweakable, () => { return TestClass.IntProperty; });

            tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
            ValidateBaseTweakable(tweakable, () => { return TestClass.intField; });
        }

        [Test]
        public void MakeTweakableRangeFromReflectionFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntPropertyRange", BindingFlags.Static | BindingFlags.Public);
            Tweakable propertyAttribute = property.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;
            FieldInfo field = typeof(TestClass).GetField("intFieldRange", BindingFlags.Static | BindingFlags.Public);
            Tweakable fieldAttribute = field.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;

            ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
            ValidateTweakableRange(tweakable, () => { return TestClass.IntPropertyRange; });

            tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
            ValidateTweakableRange(tweakable, () => { return TestClass.intFieldRange; });
        }

        [Test]
        public void MakeTweakableToggleFromReflectionFactoryAndValidate()
        {
            PropertyInfo property = typeof(TestClass).GetProperty("IntPropertyToggle", BindingFlags.Static | BindingFlags.Public);
            Tweakable propertyAttribute = property.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;
            FieldInfo field = typeof(TestClass).GetField("intFieldToggle", BindingFlags.Static | BindingFlags.Public);
            Tweakable fieldAttribute = field.GetCustomAttributes(typeof(Tweakable), false)[0] as Tweakable;

            ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.IntPropertyToggle; });

            tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.intFieldToggle; });
        }

        [Test]
        public void ScanAndAddToManager()
        {
            Scanner scanner = new Scanner();
            ScanOptions options = new ScanOptions();
            options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
            options.Types.ScannableRefs = new Type[] { typeof(TestClass) };

            TweakableManager manager = new TweakableManager(scanner);
            scanner.Scan(options);

            var tweakables = manager.GetTweakables(null);
            Assert.AreEqual(6, tweakables.Count);

            var tweakable = manager.GetTweakable(new SearchOptions("IntPropertyToggle"));
            Assert.IsNotNull(tweakable);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.IntPropertyToggle; });

            tweakable = manager.GetTweakable(new SearchOptions("intFieldToggle"));
            Assert.IsNotNull(tweakable);
            ValidateTweakableToggle(tweakable as TweakableToggle<int>, () => { return TestClass.intFieldToggle; });
        }
    }
}