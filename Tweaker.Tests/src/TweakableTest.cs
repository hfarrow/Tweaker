using System.Collections;
using System.Reflection;
using System;

using NUnit.Framework;

using Tweaker.Core;
using Tweaker.AssemblyScanner;

#if UNITY_EDITOR
using UnityEngine;
using UnityTest;
#endif

namespace Tweaker.Core.Tests
{
	[TestFixture]
	public class TweakableTest
	{
#pragma warning disable 0067,0649,0659,0661
		private class TestClass
		{
			[Tweakable("IntProperty")]
			public static int IntProperty { get; set; }

			[Tweakable("IntPropertyRange"), TweakerRange(0, 100)]
			public static int IntPropertyRange { get; set; }

			[Tweakable("IntPropertyToggle"),
			 NamedToggleValue("zero", 0, 0),
			 NamedToggleValue("eleven", 11, 1),
			 NamedToggleValue("hundred", 100, 2)]
			[CustomTweakerAttribute]
			public static int IntPropertyToggle { get; set; }

			[Tweakable("intPropertyStep"), StepSize(10)]
			public static int intPropertyStep { get; set; }

			[Tweakable("intWrapperProperty"), StepSize(10)]
			public static IntWrapper intWrapperProperty { get; set; }

			[Tweakable("intField")]
			public static int intField;

			[Tweakable("intFieldRange"), TweakerRange(0, 100)]
			public static int intFieldRange;

			[Tweakable("intFieldToggle"),
			 NamedToggleValue("zero", 0, 0),
			 NamedToggleValue("eleven", 11, 1),
			 NamedToggleValue("hundred", 100, 2)]
			public static int intFieldToggle;

			[Tweakable("intFieldStep"), StepSize(10)]
			public static int intFieldStep = 0;

			[Tweakable("intWrapperField"), StepSize(10)]
			public static IntWrapper intWrapperField = new IntWrapper();
		}

		private class IntWrapper
		{
			public int value;

			public IntWrapper()
			{
				this.value = default(int);
			}

			public IntWrapper(int value)
			{
				this.value = value;
			}

			public static IntWrapper operator +(IntWrapper a, IntWrapper b)
			{
				return new IntWrapper(a.value + b.value);
			}

			public static IntWrapper operator -(IntWrapper a, IntWrapper b)
			{
				return new IntWrapper(a.value - b.value);
			}

			public static bool operator ==(IntWrapper a, IntWrapper b)
			{
				return a.value == b.value;
			}

			public static bool operator !=(IntWrapper a, IntWrapper b)
			{
				return !(a == b);
			}

			public override bool Equals(object obj)
			{
				if (obj is IntWrapper)
				{
					return (IntWrapper)obj == this;
				}
				else
				{
					return false;
				}
			}
		}
#pragma warning restore 0067,0649,0659,0661

		[SetUp]
		public void Init()
		{

		}

		public const int MIN_VALUE = 0;
		public const int MAX_VALUE = 100;

		public void ValidateBaseTweakable<T>(ITweakable tweakable, Func<T> getter)
		{
			if (typeof(T) == typeof(int))
			{
				Assert.IsNotNull(tweakable);
				tweakable.SetValue(11);
				Assert.AreEqual(11, getter());
				Assert.AreEqual(11, tweakable.GetValue());
			}
			else if (typeof(T) == typeof(IntWrapper))
			{
				Assert.IsNotNull(tweakable);
				tweakable.SetValue(new IntWrapper(11));
				Assert.AreEqual(new IntWrapper(11), getter());
				Assert.AreEqual(new IntWrapper(11), tweakable.GetValue());
			}
			else
			{
				Assert.Fail("Can only validate int and IntWrapper");
			}
		}

		public void ValidateTweakableRange<T>(ITweakable tweakable, Func<T> getter)
		{
			Assert.IsTrue(tweakable.HasRange);
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

		public void ValidateTweakableStep<T>(ITweakable tweakable, Func<T> getter)
		{
			Assert.IsTrue(tweakable.HasStep);
			IStepTweakable stepInterface = tweakable.Step;
			Assert.IsNotNull(stepInterface);
			StepTweakable<T> step = stepInterface as StepTweakable<T>;
			Assert.IsNotNull(step);
			ValidateBaseTweakable(tweakable, getter);

			T oldValue = getter();
			T expectedValue = default(T);
			if (oldValue is int)
			{
				object obj = oldValue;
				int value = (int)obj + (int)step.StepSize;
				obj = value;
				expectedValue = (T)obj;
			}
			else if (oldValue is IntWrapper)
			{
				object obj = oldValue;
				IntWrapper value = (IntWrapper)obj + (IntWrapper)step.StepSize;
				obj = value;
				expectedValue = (T)obj;
			}
			else
			{
				Assert.Fail("Can only validate int and IntWrapper");
			}

			step.StepNext();
			Assert.AreEqual(expectedValue, getter());
			Assert.AreEqual(expectedValue, (T)tweakable.GetValue());

			step.StepPrevious();
			Assert.AreEqual(oldValue, getter());
			Assert.AreEqual(oldValue, (T)tweakable.GetValue());
		}

		public void ValidateTweakableToggle(BaseTweakable<int> tweakable, Func<int> getter)
		{
			Assert.IsTrue(tweakable.HasToggle);
			IToggleTweakable toggleInterface = tweakable.Toggle;
			Assert.IsNotNull(toggleInterface);
			Assert.AreEqual(typeof(int), tweakable.TweakableType);
			ToggleTweakable<int> toggle = toggleInterface as ToggleTweakable<int>;
			Assert.IsNotNull(toggle);
			ValidateBaseTweakable(tweakable, getter);
			toggle.SetValueByName(toggle.GetNameByIndex(0));

			Assert.AreEqual(toggle.GetIndexOfValue(getter()), toggle.CurrentIndex);
			Assert.AreEqual(0, toggle.CurrentIndex);
			Assert.AreEqual(0, getter());
			Assert.AreEqual("zero", toggle.GetValueName());

			// toggle forward all values
			Assert.AreEqual(11, toggle.StepNext());
			Assert.AreEqual(1, toggle.CurrentIndex);
			Assert.AreEqual(11, getter());
			Assert.AreEqual("eleven", toggle.GetValueName());

			Assert.AreEqual(100, toggle.StepNext());
			Assert.AreEqual(2, toggle.CurrentIndex);
			Assert.AreEqual(100, getter());
			Assert.AreEqual("hundred", toggle.GetValueName());

			// wrap
			Assert.AreEqual(0, toggle.StepNext());
			Assert.AreEqual(0, toggle.CurrentIndex);

			// toggle backwards all values
			// wrap first this time;
			Assert.AreEqual(100, toggle.StepPrevious());
			Assert.AreEqual(2, toggle.CurrentIndex);

			Assert.AreEqual(11, toggle.StepPrevious());
			Assert.AreEqual(1, toggle.CurrentIndex);
			Assert.AreEqual(0, toggle.StepPrevious());
			Assert.AreEqual(0, toggle.CurrentIndex);

			// GetIndexOfValue
			Assert.AreEqual(0, toggle.GetIndexOfValue(0));
			Assert.AreEqual(1, toggle.GetIndexOfValue(11));
			Assert.AreEqual(2, toggle.GetIndexOfValue(100));

			// GetNameByIndex
			Assert.AreEqual("zero", toggle.GetNameByIndex(0));
			Assert.AreEqual("eleven", toggle.GetNameByIndex(1));
			Assert.AreEqual("hundred", toggle.GetNameByIndex(2));

			// GetNameByValue
			Assert.AreEqual("zero", toggle.GetNameByValue(0));
			Assert.AreEqual("eleven", toggle.GetNameByValue(11));
			Assert.AreEqual("hundred", toggle.GetNameByValue(100));

			// SetValueByName
			toggle.SetValueByName("eleven");
			Assert.AreEqual(11, getter());
			Assert.AreEqual(11, tweakable.GetValue());
			Assert.AreEqual("eleven", toggle.GetValueName());

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
		public void MakeTweakbleStepFromFactoryAndValidate()
		{
			PropertyInfo property = typeof(TestClass).GetProperty("intPropertyStep", BindingFlags.Static | BindingFlags.Public);
			FieldInfo field = typeof(TestClass).GetField("intFieldStep", BindingFlags.Static | BindingFlags.Public);

			var stepSize = new TweakableInfo<int>.TweakableStepSize(10);

			TweakableInfo<int> info = new TweakableInfo<int>("intPropertyStep", null, stepSize, null);
			ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, property, null);
			ValidateTweakableStep(tweakable, () => { return TestClass.intPropertyStep; });

			info = new TweakableInfo<int>("intFieldStep", null, stepSize, null);
			tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, field, null);
			ValidateTweakableStep(tweakable, () => { return TestClass.intFieldStep; });
		}

		[Test]
		public void MakeTweakbleStepCustomClassFromFactoryAndValidate()
		{
			PropertyInfo property = typeof(TestClass).GetProperty("intWrapperProperty", BindingFlags.Static | BindingFlags.Public);
			FieldInfo field = typeof(TestClass).GetField("intWrapperField", BindingFlags.Static | BindingFlags.Public);

			var stepSize = new TweakableInfo<IntWrapper>.TweakableStepSize(new IntWrapper(10));

			TweakableInfo<IntWrapper> info = new TweakableInfo<IntWrapper>("intWrapperProperty", null, stepSize, null);
			ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo<IntWrapper>(info, property, null);
			ValidateTweakableStep(tweakable, () => { return TestClass.intWrapperProperty; });

			info = new TweakableInfo<IntWrapper>("intWrapperField", null, stepSize, null);
			tweakable = TweakableFactory.MakeTweakableFromInfo<IntWrapper>(info, field, null);
			ValidateTweakableStep(tweakable, () => { return TestClass.intWrapperField; });
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
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.IntProperty; });


			info = new TweakableInfo<int>("intField", null, null, toggleValues);
			tweakable = TweakableFactory.MakeTweakableFromInfo<int>(info, field, null);
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.intField; });
		}

		[Test]
		public void MakeBaseTweakableFromReflectionFactoryAndValidate()
		{
			PropertyInfo property = typeof(TestClass).GetProperty("IntProperty", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute propertyAttribute = property.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;
			FieldInfo field = typeof(TestClass).GetField("intField", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute fieldAttribute = field.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;

			ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
			ValidateBaseTweakable(tweakable, () => { return TestClass.IntProperty; });

			tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
			ValidateBaseTweakable(tweakable, () => { return TestClass.intField; });
		}

		[Test]
		public void MakeTweakableRangeFromReflectionFactoryAndValidate()
		{
			PropertyInfo property = typeof(TestClass).GetProperty("IntPropertyRange", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute propertyAttribute = property.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;
			FieldInfo field = typeof(TestClass).GetField("intFieldRange", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute fieldAttribute = field.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;

			ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
			ValidateTweakableRange(tweakable, () => { return TestClass.IntPropertyRange; });

			tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
			ValidateTweakableRange(tweakable, () => { return TestClass.intFieldRange; });
		}

		[Test]
		public void MakeTweakableToggleFromReflectionFactoryAndValidate()
		{
			PropertyInfo property = typeof(TestClass).GetProperty("IntPropertyToggle", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute propertyAttribute = property.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;
			FieldInfo field = typeof(TestClass).GetField("intFieldToggle", BindingFlags.Static | BindingFlags.Public);
			TweakableAttribute fieldAttribute = field.GetCustomAttributes(typeof(TweakableAttribute), false)[0] as TweakableAttribute;

			ITweakable tweakable = TweakableFactory.MakeTweakable(propertyAttribute, property, null);
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.IntPropertyToggle; });

			tweakable = TweakableFactory.MakeTweakable(fieldAttribute, field, null);
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.intFieldToggle; });
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
			Assert.AreEqual(10, tweakables.Count);

			var tweakable = manager.GetTweakable(new SearchOptions("IntPropertyToggle"));
			Assert.IsNotNull(tweakable);
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.IntPropertyToggle; });

			tweakable = manager.GetTweakable(new SearchOptions("intFieldToggle"));
			Assert.IsNotNull(tweakable);
			ValidateTweakableToggle(tweakable as BaseTweakable<int>, () => { return TestClass.intFieldToggle; });
		}

		[Test]
		public void ScanAndRetrieveCustomTweakerAttribute()
		{
			Scanner scanner = new Scanner();
			ScanOptions options = new ScanOptions();
			options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			options.Types.ScannableRefs = new Type[] { typeof(TestClass) };

			TweakableManager manager = new TweakableManager(scanner);
			scanner.Scan(options);

			var tweakables = manager.GetTweakables(null);
			Assert.AreEqual(10, tweakables.Count);

			var tweakable = manager.GetTweakable(new SearchOptions("IntPropertyToggle"));
			Assert.IsNotNull(tweakable);
			Assert.AreEqual(1, tweakable.CustomAttributes.Length);
			Assert.AreEqual(typeof(CustomTweakerAttribute), tweakable.CustomAttributes[0].GetType());
		}

		[Test]
		public void StaticTweakableAlwaysValid()
		{
			Scanner scanner = new Scanner();
			ScanOptions options = new ScanOptions();
			options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			options.Types.ScannableRefs = new Type[] { typeof(TestClass) };

			TweakableManager manager = new TweakableManager(scanner);
			scanner.Scan(options);

			var tweakable = manager.GetTweakable("intFieldToggle");
			Assert.IsTrue(tweakable.IsValid);
			Assert.IsNull(tweakable.WeakInstance);
			Assert.IsNull(tweakable.StrongInstance);
		}

		[Test]
		public void TweakableValueChangedEvent()
		{
			Scanner scanner = new Scanner();
			ScanOptions options = new ScanOptions();
			options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			options.Types.ScannableRefs = new Type[] { typeof(TestClass) };

			TweakableManager manager = new TweakableManager(scanner);
			scanner.Scan(options);

			var tweakable = manager.GetTweakable("IntProperty");
			TestClass.IntProperty = 0;

			int expectedValue = 1;
			bool wasDispatched = false;
			tweakable.ValueChanged += (oldValue, newValue) =>
			{
				Assert.AreEqual(0, oldValue);
				Assert.AreEqual(expectedValue, newValue);
				wasDispatched = true;
			};
			tweakable.SetValue(expectedValue);
			Assert.IsTrue(wasDispatched);
		}

		[Test]
		public void MakeTweakableFromVirtualField()
		{
			const string fieldName = "TestVirtualField";
			object virtualFieldRef;

			TweakableInfo<int> info = new TweakableInfo<int>(fieldName, null, null, null);
			ITweakable tweakable = TweakableFactory.MakeTweakableFromInfo(info, out virtualFieldRef);
			Assert.IsNotNull(tweakable);
			Assert.AreEqual(fieldName, tweakable.Name);
			Assert.AreEqual(0, tweakable.GetValue());
			tweakable.SetValue(1);
			Assert.AreEqual(1, tweakable.GetValue());

			tweakable = TweakableFactory.MakeTweakable(typeof(int), fieldName, "waka waka", out virtualFieldRef);
			Assert.IsNotNull(tweakable);
			Assert.AreEqual(fieldName, tweakable.Name);
			Assert.AreEqual(0, tweakable.GetValue());
			tweakable.SetValue(1);
			Assert.AreEqual(1, tweakable.GetValue());
		}
	}
}