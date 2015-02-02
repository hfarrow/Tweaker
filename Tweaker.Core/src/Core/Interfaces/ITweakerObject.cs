using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// All Tweaker objects implement this interface.
    /// </summary>
    public interface ITweakerObject
    {
        /// <summary>
        /// The name that this tweaker object registers with.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Does this tweaker object bind a public member or type?
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// The assembly of the type or member that this tweaker objects binds to.
        /// </summary>
        Assembly Assembly { get; }
    }
}
