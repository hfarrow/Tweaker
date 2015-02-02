using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;
using System.Text.RegularExpressions;

namespace Ghostbit.Tweaker.AssemblyScanner
{
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
            AddBaseProcessor(processor);
        }

        public void AddProcessor<TInput, TResult>(ITypeScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            AddBaseProcessor(processor);
        }

        public void AddProcessor<TInput, TResult>(IMemberScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            AddBaseProcessor(processor);
        }

        private void AddBaseProcessor<TInput, TResult>(IScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            if (!processors.ContainsKey(typeof(TInput)))
            {
                processors.Add(typeof(TInput), new List<BaseProcessorWrapper>());
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
                // TODO: Can Exists be skipped and just go straight to RemoveAll?
                // Don't have unit tests for the scanner so not changing atm.
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
        private class BaseProcessorWrapper
        {
            public readonly object Processor;
            private HashSet<object> processedObjects;

            protected BaseProcessorWrapper(object processor)
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

        private class ProcessorWrapper<TInput, TResult> :
            BaseProcessorWrapper
            where TInput : class
        {
            public ProcessorWrapper(IAttributeScanProcessor<TInput, TResult> processor)
                : base(processor)
            {
            }

            public ProcessorWrapper(ITypeScanProcessor<TInput, TResult> processor)
                : base(processor)
            {
            }

            public ProcessorWrapper(IMemberScanProcessor<TInput, TResult> processor)
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

            protected override void DoProcessType()
            {
                ((ITypeScanProcessor<TInput, TResult>)Processor).ProcessType();
            }

            protected override void DoProcessMember(MemberInfo memberInfo, Type type)
            {
                ((IMemberScanProcessor<TInput, TResult>)Processor).ProcessMember(memberInfo as TInput, type);
            }
        }

        private BaseProcessorWrapper MakeWrapper<TInput, TResult>(IScanProcessor<TInput, TResult> processor)
            where TInput : class
        {
            if (processor is IAttributeScanProcessor<TInput, TResult>)
            {
                return new ProcessorWrapper<TInput, TResult>((IAttributeScanProcessor<TInput, TResult>)processor);
            }
            else if (processor is ITypeScanProcessor<TInput, TResult>)
            {
                return new ProcessorWrapper<TInput, TResult>((ITypeScanProcessor<TInput, TResult>)processor);
            }
            else if (processor is IMemberScanProcessor<TInput, TResult>)
            {
                return new ProcessorWrapper<TInput, TResult>((IMemberScanProcessor<TInput, TResult>)processor);
            }
            else
            {
                return null;
            }
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

        private class ProcessorDictionary : Dictionary<Type, List<BaseProcessorWrapper>> { }
        private class ResultProviderDictionary : Dictionary<Type, IResultGroup> { }
    }
}