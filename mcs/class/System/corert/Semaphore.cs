namespace System.Threading
{
    public sealed partial class Semaphore
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

		private void CreateSemaphoreCore(int initialCount, int maximumCount, string name, out bool createdNew)
		{
			if (Environment.IsRunningOnWindows)
			{
				Windows_CreateSemaphoreCore (initialCount, maximumCount, name, out createdNew);
			}
			else
			{
				Unix_CreateSemaphoreCore (initialCount, maximumCount, name, out createdNew);
			}
		}

		private static OpenExistingResult OpenExistingWorker (string name, out Semaphore result)
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

		private static int ReleaseCore (IntPtr handle, int releaseCount)
		{
			if (Environment.IsRunningOnWindows)
			{
				return Windows_ReleaseCore (handle, releaseCount);
			}
			else
			{
				return Unix_ReleaseCore (handle, releaseCount);
			}
		}
    }
}