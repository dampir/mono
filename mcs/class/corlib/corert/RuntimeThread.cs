using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Internal.Runtime.Augments
{
	public sealed partial class RuntimeThread
	{
		private void PlatformSpecificInitialize ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_PlatformSpecificInitialize ();
			}
			else
			{
				if (!Environment.IsMacOS)
				{
					Unix_PlatformSpecificInitialize ();
				}
			}		
		}

		private void PlatformSpecificInitializeExistingThread ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_PlatformSpecificInitializeExistingThread ();
			}
			else
			{
				Unix_PlatformSpecificInitializeExistingThread ();
			}
		}

		internal SafeWaitHandle[] RentWaitedSafeWaitHandleArray (int requiredCapacity)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_RentWaitedSafeWaitHandleArray (requiredCapacity);
			}
			else
			{
				return Unix_RentWaitedSafeWaitHandleArray (requiredCapacity);
			}
		}

		internal void ReturnWaitedSafeWaitHandleArray (SafeWaitHandle[] waitedSafeWaitHandles)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_ReturnWaitedSafeWaitHandleArray (waitedSafeWaitHandles);
			}
			else
			{
				Unix_ReturnWaitedSafeWaitHandleArray (waitedSafeWaitHandles);
			}
		}

		private ThreadPriority GetPriorityLive ()
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_GetPriorityLive ();
			}
			else
			{
				return Unix_GetPriorityLive ();
			}
		}

		private bool SetPriorityLive (ThreadPriority priority)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_SetPriorityLive (priority);
			}
			else
			{
				return Unix_SetPriorityLive (priority);
			}
		}

		private ThreadState GetThreadState ()
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_GetThreadState ();
			}
			else
			{
				return Unix_GetThreadState ();
			}
		}

		private bool JoinInternal(int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_JoinInternal (millisecondsTimeout);
			}
			else
			{
				return Unix_JoinInternal (millisecondsTimeout);
			}
		}

		private bool CreateThread(GCHandle thisThreadHandle)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_CreateThread (thisThreadHandle);
			}
			else
			{
				return Unix_CreateThread (thisThreadHandle);
			}
		}

#if WIN_PLATFORM
		private static uint ThreadEntryPoint (IntPtr parameter)
		{
			return Windows_ThreadEntryPoint (parameter);
		}
#else
		private static IntPtr ThreadEntryPoint (IntPtr parameter)
		{ 
			return Unix_ThreadEntryPoint (parameter);
		}
#endif

		public void Interrupt ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_Interrupt ();
			}
			else
			{
				Unix_Interrupt ();
			}
		}

		internal static void UninterruptibleSleep0 ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_UninterruptibleSleep0 ();
			}
			else
			{
				Unix_UninterruptibleSleep0 ();
			}
		}

		private static void SleepInternal (int millisecondsTimeout)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_SleepInternal (millisecondsTimeout);
			}
			else
			{
				Unix_SleepInternal (millisecondsTimeout);
			}
		}

		internal static bool ReentrantWaitsEnabled => Environment.IsRunningOnWindows ? Windows_ReentrantWaitsEnabled : Unix_ReentrantWaitsEnabled;

		internal static void SuppressReentrantWaits ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_SuppressReentrantWaits ();
			}
			else
			{
				Unix_SuppressReentrantWaits ();
			}
		}

		internal static void RestoreReentrantWaits ()
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_RestoreReentrantWaits ();
			}
			else
			{
				Unix_RestoreReentrantWaits ();
			}
		}
	}
}
