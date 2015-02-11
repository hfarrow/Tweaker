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
        protected TweakerObjectInfo Info { get; set; }

        protected readonly bool isPublic;
        protected readonly WeakReference<object> instance;
        protected readonly Assembly assembly;

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
        public virtual WeakReference<object> WeakInstance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// The strong reference to the instance this tweaker object is bound to.
        /// Null if bound to a static tweaker object.
        /// </summary>
        public virtual object StrongInstance
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

        /// <summary>
        /// Indicates that the weak reference is still bound to a non-destroyed object and
        /// is in a valid state. All invalid tweaker object references should be nulled
        /// by objects holding a reference.
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                return WeakInstance == null || StrongInstance != null;
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
            if (!IsValid)
            {
                // The instance that this invokable was bound to has been destroyed.
                // Somewhere else in the code should catch this exception and
                // unregister the invokable.
                throw new TweakerObjectInvalidException(this);
            }
        }
    }
}
