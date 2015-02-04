using System.Collections.Generic;
using System.Reflection;
using System;
using Ghostbit.Tweaker.Core;
using Ghostbit.Tweaker.AssemblyScanner;

namespace Ghostbit.Tweaker.Core.Tests
{
    public class PlaceHolderAttribute : Attribute
    {
        public string Name;
    }

    public class AttributeProcessorResult
    {
        public string Name;
        public object Obj;
    }

    public class AttributeProcessor : IAttributeScanProcessor<PlaceHolderAttribute, AttributeProcessorResult>
    {

        public void ProcessAttribute(PlaceHolderAttribute input, Type type)
        {
            var result = new AttributeProcessorResult();
            result.Name = input.Name;
            result.Obj = type;
            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<AttributeProcessorResult>(result));
        }

        public void ProcessAttribute(PlaceHolderAttribute input, MemberInfo memberInfo)
        {
            var result = new AttributeProcessorResult();
            result.Name = input.Name;
            result.Obj = memberInfo;
            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<AttributeProcessorResult>(result));
        }

        public event EventHandler<ScanResultArgs<AttributeProcessorResult>> ResultProvided;
    }

    public class TypeProcessorResult
    {
        public Type ProcessedType;
        public Type InputType;
    }

    public class TypeProcessor<TInput> : ITypeScanProcessor<TInput, TypeProcessorResult>
        where TInput : class
    {
        public void ProcessType(Type type)
        {
            var result = new TypeProcessorResult();
            result.ProcessedType = type;
            result.InputType = typeof(TInput);

            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<TypeProcessorResult>(result));
        }

        public event EventHandler<ScanResultArgs<TypeProcessorResult>> ResultProvided;
    }

    public class MemberProcessorResult
    {
        public MemberInfo memberInfo;
        public Type Type;
    }

    public class MemberProcessor<TInput> : IMemberScanProcessor<TInput, MemberProcessorResult>
        where TInput : MemberInfo
    {
        public event EventHandler<ScanResultArgs<MemberProcessorResult>> ResultProvided;

        public void ProcessMember(TInput input, Type type)
        {
            var result = new MemberProcessorResult();
            result.memberInfo = input;
            result.Type = input.ReflectedType;

            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<MemberProcessorResult>(result));
        }
    }
}
