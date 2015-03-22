using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
	public interface ITweakerLogger
	{
		void Trace(string format, params object[] args);
		void Debug(string format, params object[] args);
		void Info(string format, params object[] args);
		void Warn(string format, params object[] args);
		void Error(string format, params object[] args);
		void Fatal(string format, params object[] args);
	}

	public interface ITweakerLogManager
	{
		ITweakerLogger GetLogger(string name);
		ITweakerLogger GetCurrentClassLogger();
	}

	internal class DummyLogger : ITweakerLogger
	{

		public void Trace(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "TRACE", string.Format(format, args)));
		}

		public void Debug(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "DEBUG", string.Format(format, args)));
		}

		public void Info(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "INFO", string.Format(format, args)));
		}

		public void Warn(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "WARN", string.Format(format, args)));
			System.Diagnostics.Debug.WriteLine(new StackTrace(true).ToString());
		}

		public void Error(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "ERROR", string.Format(format, args)));
			System.Diagnostics.Debug.WriteLine(new StackTrace(true).ToString());
		}

		public void Fatal(string format, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("{0} | {1}", "FATAL", string.Format(format, args)));
			System.Diagnostics.Debug.WriteLine(new StackTrace(true).ToString());
		}
	}

	internal class DummyLogManager : ITweakerLogManager
	{
		public ITweakerLogger GetLogger(string name)
		{
			return new DummyLogger();
		}

		public ITweakerLogger GetCurrentClassLogger()
		{
			return new DummyLogger();
		}
	}

	public static class LogManager
	{
		public static ITweakerLogManager Instance { get; private set; }
		public static void Set(ITweakerLogManager instance)
		{
			Instance = instance;
		}

		static LogManager()
		{
			Set(new DummyLogManager());
		}
	}
}
