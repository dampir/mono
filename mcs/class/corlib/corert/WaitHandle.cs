using Internal.Runtime.Augments;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	public abstract partial class WaitHandle
	{
		private static bool WaitOneCore (IntPtr handle, int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_WaitOneCore (handle, millisecondsTimeout);
			}
			else
			{
				return Unix_WaitOneCore (handle, millisecondsTimeout);
			}
		}

		private static bool WaitAllCore (
			RuntimeThread currentThread,
			SafeWaitHandle[] safeWaitHandles,
			WaitHandle[] waitHandles,
			int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_WaitAllCore (currentThread, safeWaitHandles, waitHandles, millisecondsTimeout);
			}
			else
			{
				return Unix_WaitAllCore (currentThread, safeWaitHandles, waitHandles, millisecondsTimeout);
			}
		}

		private static int WaitAnyCore (
			RuntimeThread currentThread,
			SafeWaitHandle[] safeWaitHandles,
			WaitHandle[] waitHandles,
			int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_WaitAnyCore (currentThread, safeWaitHandles, waitHandles, millisecondsTimeout);
			}
			else
			{
				return Unix_WaitAnyCore (currentThread, safeWaitHandles, waitHandles, millisecondsTimeout);
			}
		}

		private static bool SignalAndWaitCore (
			IntPtr handleToSignal,
			IntPtr handleToWaitOne,
			int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_SignalAndWaitCore (handleToSignal, handleToWaitOne, millisecondsTimeout);
			}
			else
			{
				return Unix_SignalAndWaitCore (handleToSignal, handleToWaitOne, millisecondsTimeout);
			}
		}
	}
}