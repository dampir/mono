namespace System.Threading
{
    public sealed partial class Mutex
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

		private void CreateMutexCore (bool initiallyOwned, string name, out bool createdNew)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_CreateMutexCore (initiallyOwned, name, out createdNew);
			}
			else
			{
				Unix_CreateMutexCore (initiallyOwned, name, out createdNew);
			}
		}

		private static OpenExistingResult OpenExistingWorker (string name, out Mutex result)
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

		private static void ReleaseMutexCore (IntPtr handle)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_ReleaseMutexCore (handle);
			}
			else
			{
				Unix_ReleaseMutexCore (handle);
			}
		}
	}
}

