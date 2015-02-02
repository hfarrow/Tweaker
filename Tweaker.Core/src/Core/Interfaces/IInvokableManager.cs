using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// Manager that tracks invokable tweaker objects.
    /// Invokables can be registered automatically if an IScanner is provided.
    /// Invokables can also be manually registered through this interface.
    /// </summary>
    public interface IInvokableManager
    {
        /// <summary>
        /// Register and bind a Delegate to an IInvokable.
        /// </summary>
        /// <param name="info">Info about the invokable.</param>
        /// <param name="del">The Delegate to register.</param>
        /// <returns>An bound IInvokable instance.</returns>
        IInvokable RegisterInvokable(InvokableInfo info, Delegate del);

        /// <summary>
        /// Register and bind a MethodInfo to an IInvokable.
        /// </summary>
        /// <param name="info">Info about the invokable.</param>
        /// <param name="invokable">The MethodInfo to bind the invokable to.</param>
        /// <param name="instance">The instance to bind the MethodInfo to. Pass null if static method.</param>
        /// <returns>An bound IInvokable instance.</returns>
        IInvokable RegisterInvokable(InvokableInfo info, MethodInfo invokable, object instance = null);

        /// <summary>
        /// Register an externally created invokable.
        /// </summary>
        /// <param name="invokable">The invokable to register.</param>
        void RegisterInvokable(IInvokable invokable);

        /// <summary>
        /// Unregister an invokable by reference.
        /// </summary>
        /// <param name="invokable">The invokable to unregister.</param>
        void UnregisterInvokable(IInvokable invokable);

        /// <summary>
        /// Unregister an invokable by name.
        /// </summary>
        /// <param name="name">The name of the invokable to unregister.</param>
        void UnregisterInvokable(string name);

        /// <summary>
        /// Retreive a set of invokables using the provided search options.
        /// </summary>
        /// <param name="options">Options to search with. null options will retreive all invokables.</param>
        /// <returns>A dictionary of matching invokables.</returns>
        TweakerDictionary<IInvokable> GetInvokables(SearchOptions options);

        /// <summary>
        /// Retreive the first invokable matched using the provided search options.
        /// </summary>
        /// <param name="options">Options to search with.</param>
        /// <returns>The matching invokable or null if none found.</returns>
        IInvokable GetInvokable(SearchOptions options);

        /// <summary>
        /// Retreive an invokable by the name it was registered with.
        /// </summary>
        /// <param name="name">The name of the desired invokable</param>
        /// <returns>The matching invokable or null if none found.</returns>
        IInvokable GetInvokable(string name);

        /// <summary>
        /// Invoke the provided invokable with the provided args.
        /// </summary>
        /// <param name="invokable">The invokable to invoke.</param>
        /// <param name="args">The args passed to the invocation. Pass null if no arguments.</param>
        /// <returns>The return value of the invokable.</returns>
        object Invoke(IInvokable invokable, object[] args);

        /// <summary>
        /// Invoke the invokable registered with the provided name and provided args if it exists.
        /// </summary>
        /// <param name="name">The name of the registered invokable.</param>
        /// <param name="args">The args passed to the invocation. Pass null if no arguments.</param>
        /// <returns>The return value of the invokable.</returns>
        object Invoke(string name, object[] args);
    }
}
