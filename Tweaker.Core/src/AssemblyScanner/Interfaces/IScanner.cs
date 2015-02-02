using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.AssemblyScanner
{
    public interface IScanner
    {
        void Scan(ScanOptions options);
        void ScanAssembly(Assembly assembly, ScanOptions options);
        void ScanType(Type type, ScanOptions options);
        //void ScanGenericType(Type type, ScanOptions options);
        void ScanMember(MemberInfo member, ScanOptions options);

        IScanResultProvider<TResult> GetResultProvider<TResult>();
        void AddProcessor<TInput, TResult>(IAttributeScanProcessor<TInput, TResult> processor) where TInput : class;
        void AddProcessor<TInput, TResult>(ITypeScanProcessor<TInput, TResult> processor) where TInput : class;
        void AddProcessor<TInput, TResult>(IMemberScanProcessor<TInput, TResult> processor) where TInput : class;
        void RemoveProcessor<TInput, TResult>(IScanProcessor<TInput, TResult> processor) where TInput : class;
    }
}
