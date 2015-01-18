using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;
using System.Text.RegularExpressions;

namespace Ghostbit.Tweaker.Core
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

    public interface IScanResultProvider<TResult>
    {
        event EventHandler<ScanResultArgs<TResult>> ResultProvided;
    }

    public class ScanResultArgs<TResult> : EventArgs
    {
        public ScanResultArgs(TResult result)
        {
            this.result = result;
        }
        public TResult result;
    }

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

    public class Scanner : IScanner
    {
        private static Scanner instance;
        public static Scanner Global
        {
            get
            {
                if (instance == null)
                    instance = new Scanner();

                return instance;
            }
        }

        private ProcessorDictionary processors;
        private ResultProviderDictionary resultProviders;

        public Scanner()
        {
            processors = new ProcessorDictionary();
            resultProviders = new ResultProviderDictionary();
        }

        public void Scan(ScanOptions options = null)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (options == null || options.CheckMatch(assembly))
                {
                    ScanAssembly(assembly, options);
                }
            }
        }

        public void ScanAssembly(Assembly assembly, ScanOptions options = null)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                foreach (var attribute in Attribute.GetCustomAttributes(type, false))
                {
                    if (options == null || options.CheckMatch(attribute))
                    {
                        ScanAttribute(attribute, type, options);
                    }
                }

                if (options == null || options.CheckMatch(type))
                {
                    if (type.ContainsGenericParameters)
                    {
                        ScanGenericType(type, options);
                    }
                    else
                    {
                        ScanType(type, options);
                    }
                }
            }
        }

        public void ScanType(Type type, ScanOptions options = null)
        {
            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var member in members)
            {
                if (options == null || options.CheckMatch(member))
                {
                    ScanMember(member, options);
                }
            }

            if (processors.ContainsKey(type))
            {
                var list = processors[type];
                foreach (var processor in list)
                {
                    if (options == null || options.CheckMatch(type))
                    {
                        processor.ProcessType(type);
                    }
                }
            }
        }

        public void ScanGenericType(Type type, ScanOptions options = null)
        {
            //throw new NotImplementedException("Not currently supported.");
        }

        public void ScanMember(MemberInfo member, ScanOptions options = null)
        {
            foreach (var attribute in Attribute.GetCustomAttributes(member, false))
            {
                if (options == null || options.CheckMatch(attribute))
                {
                    // The nested member type will be scanned by Assembly.GetTypes() so there
                    // is no need to scan it twice.
                    if (member.MemberType != MemberTypes.NestedType)
                        ScanAttribute(attribute, member, options);
                }
            }

            var type = member.GetType().BaseType;
            if (processors.ContainsKey(type))
            {
                var list = processors[type];
                foreach (var processor in list)
                {
                    processor.ProcessMember(member, member.ReflectedType);
                }
            }
        }

        public void ScanAttribute(Attribute attribute, object reflectedObject, ScanOptions options = null)
        {
            Type type = attribute.GetType();
            if (processors.ContainsKey(type))
            {
                var list = processors[type];
                foreach (var processor in list)
                {
                        if (reflectedObject is MemberInfo)
                            processor.ProcessAttribute(attribute, (MemberInfo)reflectedObject);
                        else if (reflectedObject is Type)
                            processor.ProcessAttribute(attribute, (Type)reflectedObject);
                }
            }
        }

        public IScanResultProvider<TResult> GetResultProvider<TResult>()
        {
            if (!resultProviders.ContainsKey(typeof(TResult)))
            {
                resultProviders.Add(typeof(TResult), new ResultGroup<TResult>());
            }
            return (IScanResultProvider<TResult>)resultProviders[typeof(TResult)];
        }

        public void AddProcessor<TInput, TResult>(IAttributeScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            AddProcessorBase(processor);
        }

        public void AddProcessor<TInput, TResult>(ITypeScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            AddProcessorBase(processor);
        }

        public void AddProcessor<TInput, TResult>(IMemberScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            AddProcessorBase(processor);
        }

        private void AddProcessorBase<TInput, TResult>(IScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            if (!processors.ContainsKey(typeof(TInput)))
            {
                processors.Add(typeof(TInput), new List<ScanProcessorWrapper>());
            }

            var processorList = processors[typeof(TInput)];
            if (!processorList.Exists(wrapper => wrapper.Processor == processor))
            {
                processorList.Add(MakeWrapper<TInput, TResult>(processor));
            }

            if (!resultProviders.ContainsKey(typeof(TResult)))
            {
                resultProviders.Add(typeof(TResult), new ResultGroup<TResult>());
            }

            var group = (ResultGroup<TResult>)resultProviders[typeof(TResult)];
            group.AddProcessor(processor);
        }

        public void RemoveProcessor<TInput, TResult>(IScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            if (processors.ContainsKey(typeof(TInput)))
            {
                var list = processors[typeof(TInput)];
                if (list.Exists(wrapper => wrapper.Processor == processor))
                {
                    list.RemoveAll(wrapper => wrapper.Processor == processor);
                }
            }

            if (resultProviders.ContainsKey(typeof(TResult)))
            {
                var group = (ResultGroup<TResult>)resultProviders[typeof(TResult)];
                group.RemoveProcessor(processor);
            }
        }

        /// <summary>
        /// Wraps an IScanPrecessor<TInput, TResult> in order to remove generics so that all processors
        /// can be stored in a collection. To use the processor that is wrapped, this class is inherited
        /// and has some of it's methods overriden. Those overriden methods then cast the processor back
        /// to it's original type and run the process method of that type.
        /// </summary>
        /// <remarks>
        /// If this library supports .NET 4.0 as a min spec in the future, this class could be replaced by
        /// use of the DLR. 'List<ScanProcessorWrapper>' could be replaced by 'List<dynamic>'.
        /// </remarks>
        private class ScanProcessorWrapper
        {
            public readonly object Processor;
            private HashSet<object> processedObjects;

            protected ScanProcessorWrapper(object processor)
            {
                Processor = processor;
                processedObjects = new HashSet<object>();
            }

            private bool CheckAlreadyProcessed(object obj)
            {
                if (!processedObjects.Contains(obj))
                {
                    processedObjects.Add(obj);
                    return false;
                }
                return true;
            }

            public void ProcessAttribute(Attribute attribute, Type type)
            {
                if (!CheckAlreadyProcessed(attribute.GetType().FullName + type.FullName))
                    DoProcessAttribute(attribute, type);
            }

            public void ProcessAttribute(Attribute attribute, MemberInfo memberInfo)
            {
                if (!CheckAlreadyProcessed(attribute.GetType().FullName + memberInfo.ReflectedType.FullName + memberInfo.Name))
                    DoProcessAttribute(attribute, memberInfo);
            }

            public void ProcessType(Type type)
            {
                if (!CheckAlreadyProcessed(type))
                    DoProcessType();
            }

            public void ProcessMember(MemberInfo memberInfo, Type type)
            {
                if (!CheckAlreadyProcessed(memberInfo))
                    DoProcessMember(memberInfo, type);
            }

            protected virtual void DoProcessAttribute(Attribute attribute, Type type) { }
            protected virtual void DoProcessAttribute(Attribute attribute, MemberInfo memberInfo) { }
            protected virtual void DoProcessType() { }
            protected virtual void DoProcessMember(MemberInfo memberInfo, Type type) { }
        }

        private class AttributeScanProcessorWrapper<TInput, TResult> :
            ScanProcessorWrapper
            where TInput : class
        {
            public AttributeScanProcessorWrapper(IAttributeScanProcessor<TInput, TResult> processor)
                : base(processor)
            {
            }

            protected override void DoProcessAttribute(Attribute attribute, MemberInfo memberInfo)
            {
                ((IAttributeScanProcessor<TInput, TResult>)Processor).ProcessAttribute(attribute as TInput, memberInfo);
            }

            protected override void DoProcessAttribute(Attribute attribute, Type type)
            {
                ((IAttributeScanProcessor<TInput, TResult>)Processor).ProcessAttribute(attribute as TInput, type);
            }
        }

        private class TypeScanProcessorWrapper<TInput, TResult> :
            ScanProcessorWrapper
            where TInput : class
        {
            public TypeScanProcessorWrapper(ITypeScanProcessor<TInput, TResult> processor)
                : base(processor)
            {
            }

            protected override void DoProcessType()
            {
                ((ITypeScanProcessor<TInput, TResult>)Processor).ProcessType();
            }
        }

        private class MemberScanProcessorWrapper<TInput, TResult> :
            ScanProcessorWrapper
            where TInput : class
        {
            public MemberScanProcessorWrapper(IMemberScanProcessor<TInput, TResult> processor)
                : base(processor)
            {
            }

            protected override void DoProcessMember(MemberInfo memberInfo, Type type)
            {
                ((IMemberScanProcessor<TInput, TResult>)Processor).ProcessMember(memberInfo as TInput, type);
            }
        }

        private ScanProcessorWrapper MakeWrapper<TInput, TResult>(IScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            if (processor is IAttributeScanProcessor<TInput, TResult>)
                return new AttributeScanProcessorWrapper<TInput, TResult>((IAttributeScanProcessor<TInput, TResult>)processor);
            else if (processor is ITypeScanProcessor<TInput, TResult>)
                return new TypeScanProcessorWrapper<TInput, TResult>((ITypeScanProcessor<TInput, TResult>)processor);
            else if (processor is IMemberScanProcessor<TInput, TResult>)
                return new MemberScanProcessorWrapper<TInput, TResult>((IMemberScanProcessor<TInput, TResult>)processor);
            else
                return null;
        }

        private interface IResultGroup
        {
            Type TResult { get; }
            void AddProcessor(object processor);
            void RemoveProcessor(object processor);
        }

        private class ResultGroup<T> : IScanResultProvider<T>, IResultGroup
        {
            public event EventHandler<ScanResultArgs<T>> ResultProvided;
            public Type TResult { get { return typeof(T); } }

            public void AddProcessor(object processor)
            {
                if (processor is IScanResultProvider<T>)
                {
                    ((IScanResultProvider<T>)processor).ResultProvided += OnResultProvided;
                }
            }

            public void RemoveProcessor(object processor)
            {
                if (processor is IScanResultProvider<T>)
                {
                    ((IScanResultProvider<T>)processor).ResultProvided -= OnResultProvided;
                }
            }

            private void OnResultProvided(object sender, ScanResultArgs<T> args)
            {
                if (ResultProvided != null) ResultProvided(sender, args);
            }
        }

        private class ProcessorDictionary : Dictionary<Type, List<ScanProcessorWrapper>> { }
        private class ResultProviderDictionary : Dictionary<Type, IResultGroup> { }
    }

    public class ScannerException : Exception, ISerializable
    {
        public ScannerException(string msg)
            : base(msg)
        {
        }
    }
}