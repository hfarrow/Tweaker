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
        private readonly WeakReference<object> instance;
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
        /// The weak reference to the instance this tweaker object is bound to.
        /// Null if bound to a static tweaker object.
        /// </summary>
        public WeakReference<object> WeakInstance
        {
            get { return instance; }
        }

        /// <summary>
        /// The strong reference to the instance this tweaker object is bound to.
        /// Null if bound to a static tweaker object.
        /// </summary>
        public object StrongInstance
        {
            get
            {
                if(WeakInstance == null)
                {
                    return null;
                }

                object strongRef = null;
                instance.TryGetTarget(out strongRef);
                return strongRef;
            }
        }

        public TweakerObject(TweakerObjectInfo info, Assembly assembly, WeakReference<object> instance, bool isPublic)
        {
            Info = info;
            this.assembly = assembly;
            this.instance = instance;
            this.isPublic = isPublic;
        }

        protected void CheckInstanceIsValid()
        {
            if (WeakInstance != null && StrongInstance == null)
            {
                // The instance that this invokable was bound to has been destroyed.
                // Somewhere else in the code should catch this exception and
                // unregister the invokable.
                throw new TweakerObjectInvalidException(this);
            }
        }
    }
}
