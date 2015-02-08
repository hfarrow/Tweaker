using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class NameAlreadyUsedException : Exception, ISerializable
    {
        public NameAlreadyUsedException(string name)
            : base("The name '" + name + "' is already in use.")
        {
        }
    }

    public class NotFoundException : Exception, ISerializable
    {
        public NotFoundException(string name)
            : base("The name '" + name + "' is not currently in use.")
        {
        }
    }

    public class InvokeException : Exception, ISerializable
    {
        public InvokeException(string name, object[] args, Exception inner)
            : base("Invocation of '" + name + "(" + args.ToString() + ")' failed. See inner exception.", inner)
        {
        }
    }

    public class TweakableSetException : Exception, ISerializable
    {
        public TweakableSetException(string name, object value, Exception inner)
            : base("Failed to set tweakable '" + name + "' to value '" + value.ToString() + "'. See inner exception.", inner)
        {
        }

        public TweakableSetException(string name, string message)
            : base("Failed to set tweakable '" + name + "'. " + message)
        {
        }
    }

    public class TweakableGetException : Exception, ISerializable
    {
        public TweakableGetException(string name, Exception inner)
            : base("Failed to get tweakable '" + name + "'. See inner exception.", inner)
        {
        }

        public TweakableGetException(string name, string message)
            : base("Failed to get tweakable '" + name + "'. " + message)
        {
        }
    }
}
