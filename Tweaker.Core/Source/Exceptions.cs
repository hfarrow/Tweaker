﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class NameAlreadyRegisteredException : Exception, ISerializable
    {
        public NameAlreadyRegisteredException(string name)
            : base("The name '" + name + "' is already registered.")
        {
        }
    }

    public class InstanceAlreadyRegisteredException : Exception, ISerializable
    {
        public InstanceAlreadyRegisteredException(ITweakerObject obj)
            : base("The instance of type '" + obj.GetType().Name + "' with name '" + obj.Name + "' is already registered.")
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
            : base("Invocation of '" + name + "(" + args + ")' failed. See inner exception.", inner)
        {
        }
    }

    public class TweakerObjectInvalidException : Exception, ISerializable
    {
        public TweakerObjectInvalidException(ITweakerObject obj)
            : base("The invokable named '" + obj.Name + "' is no longer valid and should be unregistered or uncached.")
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

    public class StepTweakableInvalidException : Exception, ISerializable
    {
        public StepTweakableInvalidException(string name, string message)
            : base("The step tweakable named '" + name + "' is invalid: " + message)
        {
            
        }
    }

    public class ProcessorException : Exception, ISerializable
    {
        public ProcessorException(string msg)
            : base(msg)
        {
        }
    }
}
