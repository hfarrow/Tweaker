using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.AssemblyScanner
{
    public interface IScanProcessor<TInput, TResult> :
        IScanResultProvider<TResult>
        where TInput : class
    {
    }

    public interface IAttributeScanProcessor<TInput, TResult> :
        IScanProcessor<TInput, TResult>
        where TInput : class
    {
        void ProcessAttribute(TInput input, Type type);
        void ProcessAttribute(TInput input, MemberInfo memberInfo);
    }

    public interface ITypeScanProcessor<TInput, TResult> :
        IScanProcessor<TInput, TResult>
        where TInput : class
    {
        void ProcessType();
    }

    public interface IMemberScanProcessor<TInput, TResult> :
        IScanProcessor<TInput, TResult>
        where TInput : class
    {
        void ProcessMember(TInput input, Type type);
    }
}
