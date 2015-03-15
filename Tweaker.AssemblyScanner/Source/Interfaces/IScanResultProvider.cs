using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.AssemblyScanner
{
	/// <summary>
	/// Provides the results from a processed object.
	/// </summary>
	/// <typeparam name="TResult">The type of the result that is provided.</typeparam>
	public interface IScanResultProvider<TResult>
	{
		/// <summary>
		/// Provides results as objects are processed.
		/// </summary>
		event EventHandler<ScanResultArgs<TResult>> ResultProvided;
	}

	/// <summary>
	/// The args sent with IScanResultProvider.ResultProvided.
	/// </summary>
	/// <typeparam name="TResult">The type of the result that is provided.</typeparam>
	public class ScanResultArgs<TResult> : EventArgs
	{
		public ScanResultArgs(TResult result)
		{
			this.result = result;
		}

		public TResult result;
	}
}
