using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tweaker.AssemblyScanner;

namespace Tweaker.Core
{
	public class AutoScanBase
	{
		public static IScanner Scanner { get; set; }
	}

	/// <summary>
	/// Objects wrapped with this type will automatically be scanned and added to managers.
	/// When the wrapper is finalized, the tweaker objects will be automatically unregistered.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AutoScan<T> : AutoScanBase, IDisposable
		where T : new()
	{
		public T value;
		private List<ITweakerObject> scannedObjects;

		public AutoScan() :
			this(new T())
		{
		}

		public AutoScan(T value)
		{
			this.value = value;
			scannedObjects = new List<ITweakerObject>();
			if (Scanner == null)
			{
				throw new AutoScanException("No scanner has been set. Set a scanner through AutoScan.Scanner before creating auto scannable instances.");
			}
			else
			{
				Scanner.GetResultProvider<IInvokable>().ResultProvided += OnInvokableScanned;
				Scanner.GetResultProvider<ITweakable>().ResultProvided += OnTweakableScanned;
				Scanner.GetResultProvider<IWatchable>().ResultProvided += OnWatchableScanned;
				Scanner.ScanInstance(value);
				Scanner.GetResultProvider<IInvokable>().ResultProvided -= OnInvokableScanned;
				Scanner.GetResultProvider<ITweakable>().ResultProvided -= OnTweakableScanned;
				Scanner.GetResultProvider<IWatchable>().ResultProvided -= OnWatchableScanned;
			}
		}

		~AutoScan()
		{
			Dispose();
		}

		private void OnInvokableScanned(object sender, ScanResultArgs<IInvokable> e)
		{
			scannedObjects.Add(e.result);
		}

		private void OnTweakableScanned(object sender, ScanResultArgs<ITweakable> e)
		{
			scannedObjects.Add(e.result);
		}

		private void OnWatchableScanned(object sender, ScanResultArgs<IWatchable> e)
		{
			scannedObjects.Add(e.result);
		}

		public void Dispose()
		{
			if (scannedObjects != null)
			{
				foreach (ITweakerObject obj in scannedObjects)
				{
					if (obj is IInvokable)
					{
						if (AutoInvokable.Manager != null)
						{
							AutoInvokableBase.Manager.UnregisterInvokable(obj as IInvokable);
						}
					}
					else if (obj is ITweakable)
					{
						if (AutoTweakable.Manager != null)
						{
							AutoTweakable.Manager.UnregisterTweakable(obj as ITweakable);
						}
					}
					else if (obj is IWatchable)
					{
						// TODO: implement
						throw new NotImplementedException();
						//if (AutoWatchable.Manager != null)
						//{
						//	AutoWatchable.Manager.UnregisterWatchable(obj as IWatchable);
						//}
					}
					else
					{
						throw new AutoScanException("Could not unregister invalid ITweaketObject");
					}
				}
			}
		}
	}

	public class AutoScanException : Exception, ISerializable
	{
		public AutoScanException(string message)
			: base(message)
		{
		}
	}
}