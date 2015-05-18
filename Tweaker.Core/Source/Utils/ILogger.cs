using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Tweaker.Core
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

		public static ITweakerLogger GetLogger(string name)
		{
			return Instance.GetLogger(name);
		}

		// Attributes copied form NLog to ensure callstack is not optimized away resulting
		// in incorrect logger names
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static ITweakerLogger GetCurrentClassLogger()
		{
			return Instance.GetLogger(GetClassFullName());
		}

		public static ITweakerLogger GetClassLogger<T>()
		{
			return Instance.GetLogger(typeof(T).FullName);
		}

		/// <summary>
		/// Gets the fully qualified name of the class invoking the LogManager, including the 
		/// namespace but not the assembly.
		/// <remarks>Method copied from NLog: "https://github.com/NLog/NLog/blob/master/src/NLog/LogManager.cs"</remarks>
		/// </summary>
		private static string GetClassFullName()
		{
			string className;
			Type declaringType;
			int framesToSkip = 2;

			do
			{
				StackFrame frame = new StackFrame(framesToSkip, false);
				MethodBase method = frame.GetMethod();
				declaringType = method.DeclaringType;
				if (declaringType == null)
				{
					className = method.Name;
					break;
				}

				framesToSkip++;
				className = declaringType.FullName;
			} while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

			return className;
		}
	}
}
