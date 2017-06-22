namespace System.Threading
{
	public partial class EventWaitHandle
	{
		private static void VerifyNameForCreate (string name)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_VerifyNameForCreate (name);
			}
			else
			{
				Unix_VerifyNameForCreate (name);
			}
		}

		private void CreateEventCore (bool initialState, EventResetMode mode, string name, out bool createdNew)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_CreateEventCore (initialState, mode, name, out createdNew);
			}
			else
			{
				Unix_CreateEventCore (initialState, mode, name, out createdNew);
			}
		}

		private static OpenExistingResult OpenExistingWorker (string name, out EventWaitHandle result)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_OpenExistingWorker (name, out result);
			}
			else
			{
				return Unix_OpenExistingWorker (name, out result);
			}
		}

		private static bool ResetCore (IntPtr handle)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_ResetCore (handle);
			}
			else
			{
				return Unix_ResetCore (handle);
			}
		}

		private static bool SetCore (IntPtr handle)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_SetCore (handle);
			}
			else
			{
				return Unix_SetCore (handle);
			}
		}
	}
}