using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Tweaker.AssemblyScanner
{
	/// <summary>
	/// Processors operate on an object of the specified input type and produce zero,
	/// one, or more result objects via IScanResultProvider.ResultProvided.
	/// </summary>
	/// <remarks>
	/// If using the standard Scanner packaged with Tweaker, any given input object
	/// will only ever be processed once by each processor instance.
	/// </remarks>
	/// <typeparam name="TInput">The type of the input object.</typeparam>
	/// <typeparam name="TResult">The type of the result object that is created.</typeparam>
	public interface IScanProcessor<TInput, TResult> :
		IScanResultProvider<TResult>
		where TInput : class
	{
	}

	/// <summary>
	/// Processes attributes found on types or members.
	/// </summary>
	/// <typeparam name="TInput">The input attribute type.</typeparam>
	/// <typeparam name="TResult">The type of the result object that is created.</typeparam>
	public interface IAttributeScanProcessor<TInput, TResult> :
		IScanProcessor<TInput, TResult>
		where TInput : class
	{
		void ProcessAttribute(TInput input, Type type, IBoundInstance instance = null);
		void ProcessAttribute(TInput input, MemberInfo memberInfo, IBoundInstance instance = null);
	}

	/// <summary>
	/// Processes any type that inherits TInput.
	/// </summary>
	/// <typeparam name="TInput">The type that the processed type inherits from.</typeparam>
	/// <typeparam name="TResult">The type of the result object that is created.</typeparam>
	public interface ITypeScanProcessor<TInput, TResult> :
		IScanProcessor<TInput, TResult>
		where TInput : class
	{
		void ProcessType(Type type, IBoundInstance instance = null);
	}

	/// <summary>
	/// Processes any specific types of the members of any type.
	/// </summary>
	/// <typeparam name="TInput">The type to process that inherits MemberInfo.</typeparam>
	/// <typeparam name="TResult">The type of the result object that is created.</typeparam>
	public interface IMemberScanProcessor<TInput, TResult> :
		IScanProcessor<TInput, TResult>
		where TInput : class
	{
		void ProcessMember(TInput input, Type type, IBoundInstance instance = null);
	}
}
