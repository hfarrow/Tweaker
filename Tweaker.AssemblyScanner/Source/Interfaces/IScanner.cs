using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Tweaker.AssemblyScanner
{
	/// <summary>
	/// A scanner enumarates all assemblies, types, and members. Processors are added to capture
	/// and return results to listeners.
	/// </summary>
	/// <remarks>
	/// Which assemblies, types, and members that get scanned can be controlled
	/// or filtered by the ScanOptions param that the scanning methods define.
	/// Scanning a large number of objects is expensive so it is recommended that
	/// ScanOptions are used to filter only to Assemblies or types that will contain
	/// the input types of the scanner processors. For example, in almost all cases,
	/// you should not need to scan .Net or other system assemblies. 
	/// 
	/// Depending on the implementation of IScanner (such as the provided Scanner class)
	/// Only assemblies that are loaded will be scanned. It is possible that your 
	/// app references assemblies that have not yet been loaded when Scan is called.
	/// If that is the case, it is your responsiblity to scan those lazy loaded assemblies
	/// or other assemblies that are dynamically loaded.
	/// 
	/// In order for IScanner to be useful, you must add processors to the IScanner
	/// instance. Processors operate on the enumerated types or members and provide
	/// resulting objects defined by the processor. There are three categories of
	/// processors:
	///     1. Attribute Processor
	///         Process all types or members that are annotated with provided attribute.
	///     2. Type Processor
	///         Process all types that inherit from the provided base type.
	///     3. Member Processor
	///         Process all members of the provided type. The provided type must inherit
	///         MemberInfo. This lets you process all Methods or all events.
	///         
	/// The Scan methods only process types and static members except for ScanInstance.
	/// To scan for instance members, use ScanInstance. There is no way for a scanner
	/// to discover all your object instance in order to scan them. Such functionality
	/// does not exist in the CLR. This means you must manually call ScanInstance on
	/// objects as needed. If your objects are created via a factory, the factory can
	/// call ScanInstance on the object before returning it.
	/// </remarks>
	public interface IScanner
	{
		/// <summary>
		/// Scan all assemblies using the provided options.
		/// Only static members will be processed.
		/// </summary>
		/// <param name="options">The options for performing the scan.</param>
		void Scan(ScanOptions options = null);

		/// <summary>
		/// Scan the provided assembly with the provided options.
		/// Only static members will be processed.
		/// </summary>
		/// <param name="assembly">The assembly to be scanned.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanAssembly(Assembly assembly, ScanOptions options = null);

		/// <summary>
		/// Scan the provided type with the provided options.
		/// Only static members will be processed.
		/// </summary>
		/// <param name="type">The type to be scanned.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanType(Type type, ScanOptions options = null);

		// Not currently implemented and therefore not exposed.
		//void ScanGenericType(Type type, ScanOptions options = null);

		/// <summary>
		/// Scan the provided member with the provided options
		/// Only static members will be processed.
		/// </summary>
		/// <param name="member">The member to be scanned.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanMember(MemberInfo member, ScanOptions options = null);

		/// <summary>
		/// Scan the provided object.
		/// Only instance members will be processed. Static members will be ignored.
		/// </summary>
		/// <param name="instance">The object to scan.</param>
		/// <rereturns>The bound instance that was created as a result of the scan.</rereturns>
		IBoundInstance ScanInstance(object instance);

		/// <summary>
		/// Scan the instance members of the provided type with the provided options.
		/// Only instance members will be processed if the provides instance is not null.
		/// Otherwise, only static members will be processed.
		/// </summary>
		/// <param name="type">The type to be scanned.</param>
		/// <param name="instance">The instance of the provided type.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanType(Type type, IBoundInstance instance, ScanOptions options = null);

		/// <summary>
		/// Scan the instance members of the provided generic type with the provided options.
		/// Only instance members will be processed if the provides instance is not null.
		/// Otherwise, only static members will be processed.
		/// </summary>
		/// <param name="type">The type to be scanned.</param>
		/// <param name="instance">The instance of the provided type.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanGenericType(Type type, IBoundInstance instance, ScanOptions options = null);

		/// <summary>
		/// Scan the member of belonging to the provided instance with the provided options.
		/// Only instance members will be processed if the provides instance is not null.
		/// Otherwise, only static members will be processed.
		/// </summary>
		/// <param name="member">The member to be scanned.</param>
		/// <param name="instance">The instance of the provided object containing the provided member.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanMember(MemberInfo member, IBoundInstance instance, ScanOptions options = null);

		/// <summary>
		/// Scan the attribute annotating to the provided object and instance with the provided options.
		/// Only instance members will be processed if the provides instance is not null.
		/// Otherwise, only static members will be processed.
		/// </summary>
		/// <param name="attribute">The attribute to scan.</param>
		/// <param name="reflectedObject">The object the attribute annotates. Must be an instance of MemberInfo or Type.</param>
		/// <param name="instance">The instance of the provided object containing the provided member.</param>
		/// <param name="options">The options for performing the scan.</param>
		void ScanAttribute(Attribute attribute, object reflectedObject, IBoundInstance instance, ScanOptions options = null);

		/// <summary>
		/// Get the result provider for the provided result type.
		/// All processors that provide results of TResult will be grouped
		/// together into the provider returns by this.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <returns>A result provider for the provided result type.</returns>
		IScanResultProvider<TResult> GetResultProvider<TResult>();

		/// <summary>
		/// Add an attribute processor of the provided input and result types.
		/// The results of the added processor should be retrieved via the
		/// GetResultProvider\<TResult\>.ResultProvided event.
		/// </summary>
		/// <typeparam name="TInput">The processor input type.</typeparam>
		/// <typeparam name="TResult">The processor result type.</typeparam>
		/// <param name="processor">The processor to add.</param>
		void AddProcessor<TInput, TResult>(IAttributeScanProcessor<TInput, TResult> processor) where TInput : class;

		/// <summary>
		/// Add a type processor of the provided input and result types.
		/// The results of the added processor should be retrieved via the
		/// GetResultProvider\<TResult\>.ResultProvided event.
		/// </summary>
		/// <typeparam name="TInput">The processor input type.</typeparam>
		/// <typeparam name="TResult">The processor result type.</typeparam>
		/// <param name="processor">The processor to add.</param>
		void AddProcessor<TInput, TResult>(ITypeScanProcessor<TInput, TResult> processor) where TInput : class;

		/// <summary>
		/// Add a member processor of the provided input and result types.
		/// The results of the added processor should be retrieved via the
		/// GetResultProvider\<TResult\>.ResultProvided event.
		/// </summary>
		/// <typeparam name="TInput">The processor input type.</typeparam>
		/// <typeparam name="TResult">The processor result type.</typeparam>
		/// <param name="processor">The processor to add.</param>
		void AddProcessor<TInput, TResult>(IMemberScanProcessor<TInput, TResult> processor) where TInput : class;

		/// <summary>
		/// Remove a processor of the provided input and result types.
		/// The processor will no longer be called upon by the Scanner and
		/// the results of the processor will no longer be provided by GetResultProvider.
		/// </summary>
		/// <typeparam name="TInput">The processor input type.</typeparam>
		/// <typeparam name="TResult">The processor result type.</typeparam>
		/// <param name="processor">The processor to add.</param>
		void RemoveProcessor<TInput, TResult>(IScanProcessor<TInput, TResult> processor) where TInput : class;
	}
}
