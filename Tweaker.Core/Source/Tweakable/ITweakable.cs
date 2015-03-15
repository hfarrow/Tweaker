using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
	/// <summary>
	/// All tweakable tweaker objects implement this interface.
	/// </summary>
	public interface ITweakable : ITweakerObject
	{
		/// <summary>
		/// Assign a value to the tweakable object.
		/// </summary>
		/// <param name="value">The value that the tweakable object will be assigned.</param>
		/// <remarks>
		/// This interface does not promise type checking or exception handling.
		/// However, is is recommended that implementers provice sufficient safety.
		/// </remarks>
		void SetValue(object value);

		/// <summary>
		/// Retreive the value currently assigned to the tweakable object.
		/// </summary>
		/// <returns>The value currently assigned to the tweakable object.</returns>
		object GetValue();

		/// <summary>
		/// The type represented by the tweakable object
		/// </summary>
		/// <remarks>
		/// It is the responsibility of implementers to ensure that GetValue always
		/// returns an object of this type.
		/// </remarks>
		Type TweakableType { get; }

		/// <summary>
		/// The manager that this tweakable is registered to.
		/// A tweakable can only be registered to one manager at any given time.
		/// </summary>
		ITweakableManager Manager { get; set; }

		/// <summary>
		/// True if Step is not null and the value of this tweakable can be stepped
		/// forwards and backwards.
		/// </summary>
		bool HasStep { get; }

		/// <summary>
		/// True if Toggle is not null and the value of this tweakable can be toggled
		/// Toggles can have multiple values and not just on/off.
		/// </summary>
		bool HasToggle { get; }

		/// <summary>
		/// True if a range constraint (min, max) has been defined for this tweakable.
		/// </summary>
		bool HasRange { get; }

		/// <summary>
		/// The object used to step the value of this tweakable forward or backwards.
		/// null if this tweakable is not steppable.
		/// </summary>
		IStepTweakable Step { get; }

		/// <summary>
		/// The object used to toggle the value of this tweakable.
		/// null if this tweakable is not toggable.
		/// </summary>
		IToggleTweakable Toggle { get; }
	}

	/// <summary>
	/// Interface for interacting with a tweakable that can be stepped forwards and backwards through
	/// values.
	/// </summary>
	public interface IStepTweakable
	{
		/// <summary>
		/// The size of each step.
		/// </summary>
		object StepSize { get; }

		/// <summary>
		/// Advance the value of the tweakable forward by StepSize.
		/// </summary>
		/// <returns>The new value of the tweakable.</returns>
		object StepNext();

		/// <summary>
		/// Advance the value of the tweakable backwards by StepSize.
		/// </summary>
		/// <returns>The new value of the tweakable.</returns>
		object StepPrevious();

		/// <summary>
		/// The type of this tweakable. The objects returned by StepSize, StepNext, and StepPrevious
		/// can be casted to this type.
		/// </summary>
		Type TweakableType { get; }
	}

	/// <summary>
	/// Interface for interacting with a tweakable that can be toggled through 2 or more values.
	/// </summary>
	public interface IToggleTweakable : IStepTweakable
	{
		/// <summary>
		/// The current value index of the toggle.
		/// </summary>
		int CurrentIndex { get; }

		/// <summary>
		/// Get the index for the specified value.
		/// </summary>
		/// <param name="value">The value to get the index for.</param>
		/// <returns>The index for the specified value or -1 if the toggle does not include the value.</returns>
		int GetIndexOfValue(object value);

		/// <summary>
		/// Get the name assigned to the value at the specified index.
		/// </summary>
		/// <param name="index">The value index for the name that will be returned.</param>
		/// <returns>The name of the value or null if the index is out of bounds.</returns>
		string GetNameByIndex(int index);

		/// <summary>
		/// Get the name assigned to the specified value.
		/// </summary>
		/// <param name="value">The value to get the name for.</param>
		/// <returns>The name of the value or null if the toggle does not include the value.</returns>
		string GetNameByValue(object value);

		/// <summary>
		/// Set the tweakable to the value defined by the provided name.
		/// </summary>
		/// <param name="valueName">The name of the value to assign to the tweakable.</param>
		/// <returns>Return the new value of the tweakable. If the name was not valid, the
		/// tweakable will not change and the previous (current) value is returned.</returns>
		object SetValueByName(string valueName);

		/// <summary>
		/// Get the name of the current tweakable value.
		/// </summary>
		/// <returns>The name of the current tweakable value or null if it does have a name defined.</returns>
		string GetValueName();
	}
}
