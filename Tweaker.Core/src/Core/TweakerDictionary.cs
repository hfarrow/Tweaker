using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// Dictionary type used to map tweaker object names to tweaker objects.
    /// </summary>
    /// <typeparam name="T">The type of the tweaker object.</typeparam>
    public class TweakerDictionary<T> : Dictionary<string, T> {}
}