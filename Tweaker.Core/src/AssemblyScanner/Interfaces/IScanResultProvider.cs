using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ghostbit.Tweaker.AssemblyScanner
{
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
}
