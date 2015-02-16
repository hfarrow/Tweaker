using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// All tweakble tweaker objects implement this interface.
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

        bool IsSteppable { get; }
        bool IsToggable { get; }
        bool HasRange { get; }
        IStepTweakable AsStep { get; }
        IToggleTweakable AsToggle { get; }
    }

    public interface IStepTweakable
    {
        object StepSize { get; }
        object StepNext();
        object StepPrevious();
    }

    public interface IToggleTweakable : IStepTweakable
    {
        int CurrentIndex { get; }
        int GetIndexOfValue(object value);
        string GetNameByIndex(int index);
        string GetNameByValue(object value);
        object SetValueByName(string valueName);
        string GetValueName();
    }
}
