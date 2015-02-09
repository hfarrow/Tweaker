using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// All Tweaker attributes implement this interface.
    /// </summary>
    public interface ITweakerAttribute
    {
        /// <summary>
        /// The name that will be registered when the attribute is processed.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Globaly unique identifier for this attribute instance.
        /// </summary>
        Guid Guid { get; }
    }
}
