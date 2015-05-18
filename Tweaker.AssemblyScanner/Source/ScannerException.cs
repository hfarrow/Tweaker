using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Tweaker.AssemblyScanner
{
	public class ScannerException : Exception, ISerializable
	{
		public ScannerException(string msg)
			: base(msg)
		{
		}
	}
}
