using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// Manager that tracks tweakable tweaker objects.
    /// Tweakables can be registered automatically if an IScanner is provided.
    /// Tweakables can also be manually registered through this interface.
    /// </summary>
    public interface ITweakableManager
    {
        /// <summary>
        /// Register and bind a PropertyInfo to an ITweakable.
        /// </summary>
        /// <typeparam name="T">The expected type of the tweakable.</typeparam>
        /// <param name="info">Info about the tweakable.</param>
        /// <param name="tweakable">The PropertyInfo to bind the tweakable to.</param>
        /// <param name="instance">The instance to bind the PropertyInfo to. Pass null if static property.</param>
        /// <returns>A bound ITweakable instance.</returns>
        ITweakable RegisterTweakable<T>(TweakableInfo<T> info, PropertyInfo tweakable, object instance = null);

        /// <summary>
        /// Register and bind a FieldInfo to an ITweakable.
        /// </summary>
        /// <typeparam name="T">The type of the tweakable.</typeparam>
        /// <param name="info">Info about the tweakable.</param>
        /// <param name="tweakable">The FieldInfo to bind the tweakable to.</param>
        /// <param name="instance">The instance to bind the FieldInfo to. Pass null if static field.</param>
        /// <returns>A bound ITweakable instance.</returns>
        ITweakable RegisterTweakable<T>(TweakableInfo<T> info, FieldInfo tweakable, object instance = null);

        /// <summary>
        /// Register an externally created tweakable.
        /// </summary>
        /// <param name="tweakable">The tweakable to register.</param>
        void RegisterTweakable(ITweakable tweakable);

        /// <summary>
        /// Unregister a tweakable by reference.
        /// </summary>
        /// <param name="tweakable">The tweakable to unregister.</param>
        void UnregisterTweakable(ITweakable tweakable);

        /// <summary>
        /// Unregister a tweakable by name.
        /// </summary>
        /// <param name="name">The name of the tweakable to unregister.</param>
        void UnregisterTweakable(string name);

        /// <summary>
        /// Retreive a set of tweakables using the provided search options.
        /// </summary>
        /// <param name="options">Options to search with. null options will retreive all tweakables.</param>
        /// <returns>A dictionary of matching tweakables.</returns>
        TweakerDictionary<ITweakable> GetTweakables(SearchOptions options);

        /// <summary>
        /// Retreive the first tweakable matched using the provided search options.
        /// </summary>
        /// <param name="options">Options to search with.</param>
        /// <returns>The matching tweakable or null if none found.</returns>
        ITweakable GetTweakable(SearchOptions options);

        /// <summary>
        /// Retreive an tweakable by the name it was registered with.
        /// </summary>
        /// <param name="name">The name of the desired tweakable</param>
        /// <returns>The matching tweakable or null if none found.</returns>
        ITweakable GetTweakable(string name);

        /// <summary>
        /// Assign the provided value to a tweakable reference.
        /// </summary>
        /// <typeparam name="T">The expected type of the tweakable.</typeparam>
        /// <param name="tweakable">The tweakable reference.</param>
        /// <param name="value">The value to assign to the tweakable.</param>
        void SetTweakableValue<T>(ITweakable tweakable, T value);

        /// <summary>
        /// Assign the provided value to the tweakable registered with the provided name.
        /// </summary>
        /// <typeparam name="T">The expected type of the tweakable.</typeparam>
        /// <param name="name">The name of the registered tweakable.</param>
        /// <param name="value">The value to assign to the tweakable.</param>
        void SetTweakableValue<T>(string name, T value);

        /// <summary>
        /// Retreive the value assigned to the provided tweakable.
        /// </summary>
        /// <typeparam name="T">The expected type of the tweakable.</typeparam>
        /// <param name="tweakable">The tweakable to retreive the currently assigned value from.</param>
        /// <returns>The value currently assigned to the tweakable.</returns>
        T GetTweakableValue<T>(ITweakable tweakable);

        /// <summary>
        /// Retreive the value assign to the tweakable registered with the provided name.
        /// </summary>
        /// <typeparam name="T">The expected type of the tweakable.</typeparam>
        /// <param name="name">The name of the registered tweakable.</param>
        /// <returns>The value currently assigned to the tweakable.</returns>
        T GetTweakableValue<T>(string name);
    }
}
