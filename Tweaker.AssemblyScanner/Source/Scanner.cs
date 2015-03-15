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
			ScanType(type, null, options);
		}

		public void ScanGenericType(Type type, ScanOptions options = null)
		{
			ScanGenericType(type, null, options);
		}

		public void ScanMember(MemberInfo member, ScanOptions options = null)
		{
			ScanMember(member, null, options);
		}

		public void ScanAttribute(Attribute attribute, object reflectedObject, ScanOptions options = null)
		{
			ScanAttribute(attribute, reflectedObject, null, options);
		}

		public IBoundInstance ScanInstance(object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance", "Cannot scan null instance.");
			}

			IBoundInstance boundInstance = BoundInstanceFactory.Create(instance);
			ScanType(instance.GetType(), boundInstance);
			return boundInstance;
		}

		public void ScanType(Type type, IBoundInstance instance, ScanOptions options = null)
		{
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
			if (instance == null)
			{
				flags |= BindingFlags.Static;
			}
			else
			{
				flags |= BindingFlags.Instance;
			}

			MemberInfo[] members = type.GetMembers(flags);
			foreach (var member in members)
			{
				if (options == null || options.CheckMatch(member))
				{
					ScanMember(member, instance, options);
				}
			}

			if (options == null || options.CheckMatch(type))
			{
				foreach (Type typeKey in processors.Keys)
				{
					if (type.IsSubclassOf(typeKey))
					{
						var list = processors[typeKey];
						foreach (var processor in list)
						{
							processor.ProcessType(type, instance);
						}
					}
				}
			}
		}

		public void ScanGenericType(Type type, IBoundInstance instance, ScanOptions options = null)
		{
			//throw new NotImplementedException("Not currently supported.");
		}

		public void ScanMember(MemberInfo member, IBoundInstance instance, ScanOptions options = null)
		{
			foreach (var attribute in Attribute.GetCustomAttributes(member, false))
			{
				if (options == null || options.CheckMatch(attribute))
				{
					// The nested member type will be scanned by Assembly.GetTypes() so there
					// is no need to scan it twice.
					if (member.MemberType != MemberTypes.NestedType)
					{
						ScanAttribute(attribute, member, instance, options);
					}
				}
			}

			var type = member.GetType().BaseType;
			if (processors.ContainsKey(type))
			{
				var list = processors[type];
				foreach (var processor in list)
				{
					processor.ProcessMember(member, member.ReflectedType, instance);
				}
			}
		}

		public void ScanAttribute(Attribute attribute, object reflectedObject, IBoundInstance instance, ScanOptions options = null)
		{
			Type type = attribute.GetType();
			if (processors.ContainsKey(type))
			{
				var list = processors[type];
				foreach (var processor in list)
				{
					if (reflectedObject is MemberInfo)
						processor.ProcessAttribute(attribute, (MemberInfo)reflectedObject, instance);
					else if (reflectedObject is Type)
						processor.ProcessAttribute(attribute, (Type)reflectedObject, instance);
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
				list.RemoveAll(wrapper => wrapper.Processor == processor);
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

			// Only types and static members are tracked.
			// We can not safely track the instances of the user.
			// Last thing we want to do is add instances to a HashSet
			// because we would have no way of knowing when those objects
			// are garbage collected.
			private HashSet<object> processedObjects;

			protected BaseProcessorWrapper(object processor)
			{
				Processor = processor;
				processedObjects = new HashSet<object>();
			}

			private bool CheckAlreadyProcessed(object obj, IBoundInstance instance)
			{
				if (instance != null)
				{
					// We will process an instance every time ScanInstance is called by
					// the user.
					return false;
				}
				else
				{
					if (!processedObjects.Contains(obj))
					{
						processedObjects.Add(obj);
						return false;
					}
				}
				return true;
			}

			public void ProcessAttribute(Attribute attribute, Type type, IBoundInstance instance = null)
			{
				if (!CheckAlreadyProcessed(attribute.GetType().FullName + type.FullName, instance))
					DoProcessAttribute(attribute, type, instance);
			}

			public void ProcessAttribute(Attribute attribute, MemberInfo memberInfo, IBoundInstance instance = null)
			{
				if (!CheckAlreadyProcessed(attribute.GetType().FullName + memberInfo.ReflectedType.FullName + memberInfo.Name, instance))
					DoProcessAttribute(attribute, memberInfo, instance);
			}

			public void ProcessType(Type type, IBoundInstance instance = null)
			{
				if (!CheckAlreadyProcessed(type.FullName, instance))
					DoProcessType(type, instance);
			}

			public void ProcessMember(MemberInfo memberInfo, Type type, IBoundInstance instance = null)
			{
				if (!CheckAlreadyProcessed(memberInfo.ReflectedType.FullName + memberInfo.Name, instance))
					DoProcessMember(memberInfo, type, instance);
			}

			protected virtual void DoProcessAttribute(Attribute attribute, Type type, IBoundInstance instance = null) { }
			protected virtual void DoProcessAttribute(Attribute attribute, MemberInfo memberInfo, IBoundInstance instance = null) { }
			protected virtual void DoProcessType(Type type, IBoundInstance instance = null) { }
			protected virtual void DoProcessMember(MemberInfo memberInfo, Type type, IBoundInstance instance = null) { }
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

			protected override void DoProcessAttribute(Attribute attribute, MemberInfo memberInfo, IBoundInstance instance = null)
			{
				var attributeProcessor = Processor as IAttributeScanProcessor<TInput, TResult>;
				if (attributeProcessor != null)
				{
					attributeProcessor.ProcessAttribute(attribute as TInput, memberInfo, instance);
				}
			}

			protected override void DoProcessAttribute(Attribute attribute, Type type, IBoundInstance instance = null)
			{
				var attributeProcessor = Processor as IAttributeScanProcessor<TInput, TResult>;
				if (attributeProcessor != null)
				{
					attributeProcessor.ProcessAttribute(attribute as TInput, type, instance);
				}
			}

			protected override void DoProcessType(Type type, IBoundInstance instance = null)
			{
				var typeProcessor = Processor as ITypeScanProcessor<TInput, TResult>;
				if (typeProcessor != null)
				{
					typeProcessor.ProcessType(type, instance);
				}
			}

			protected override void DoProcessMember(MemberInfo memberInfo, Type type, IBoundInstance instance = null)
			{
				var memberProcessor = Processor as IMemberScanProcessor<TInput, TResult>;
				if (memberProcessor != null)
				{
					memberProcessor.ProcessMember(memberInfo as TInput, type, instance);
				}
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
				if (ResultProvided != null)
					ResultProvided(sender, args);
			}
		}

		private class ProcessorDictionary : Dictionary<Type, List<BaseProcessorWrapper>> { }
		private class ResultProviderDictionary : Dictionary<Type, IResultGroup> { }
	}
}