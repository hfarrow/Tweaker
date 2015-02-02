using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// All invokable tweaker objects implement this interface.
    /// </summary>
    public interface IInvokable : ITweakerObject
    {
        /// <summary>
        /// Invoke the invokable object with the provided arguments.
        /// </summary>
        /// <param name="args">Arguments to invoke with. Pass null for no arguments.</param>
        /// <returns>The return value of the invokable.</returns>
        object Invoke(object[] args = null);
    }
}
