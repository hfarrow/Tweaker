using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ghostbit.Tweaker.Core;
using System.Threading;
using NLog;

namespace Ghostbit.Tweaker.Core.Testbed
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static Tweaker tweaker;
        static void Main(string[] args)
        {
            tweaker = new Tweaker();
            tweaker.Initialized += OnInitialized;
            tweaker.Init();
        }

        private static void OnInitialized()
        {
            logger.Info("OnInitialized");
            tweaker.Shutdown();
        }
    }
}
