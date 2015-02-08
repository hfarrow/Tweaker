using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    /// <summary>
    /// Contains information about a tweaker object
    /// </summary>
    public class TweakerObjectInfo
    {
        /// <summary>
        /// All tweaker objects must have a unique name. This name is
        /// used to register with managers.
        /// </summary>
        public string Name { get; private set; }

        public TweakerObjectInfo(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Base tweaker object class.
    /// </summary>
    public abstract class TweakerObject : ITweakerObject
    {
        private TweakerObjectInfo Info { get; set; }

        private readonly bool isPublic;
        private readonly object instance;
        private readonly Assembly assembly;

        /// <summary>
        /// The name of the tweaker object. This name is used to register
        /// with managers.
        /// </summary>
        public string Name
        {
            get { return Info.Name; }
        }

        /// <summary>
        /// Does this tweaker object bind to a public type of member?
        /// </summary>
        public bool IsPublic
        {
            get { return isPublic; }
        }

        /// <summary>
        ///  The assembly of the type or member that this tweaker objects binds to.
        /// </summary>
        public Assembly Assembly
        {
            get { return assembly; }
        }

        /// <summary>
        /// The instance that this tweaker object binds to. null if static type or member.
        /// </summary>
        public object Instance
        {
            get { return instance; }
        }

        public TweakerObject(TweakerObjectInfo info, Assembly assembly, object instance, bool isPublic)
        {
            Info = info;
            this.assembly = assembly;
            this.instance = instance;
            this.isPublic = isPublic;
        }
    }
}
