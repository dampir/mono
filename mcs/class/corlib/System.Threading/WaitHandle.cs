//
// System.Threading.WaitHandle.cs
//
// Author:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// (C) 2002,2003 Ximian, Inc.	(http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	[StructLayout (LayoutKind.Sequential)]
	public abstract partial class WaitHandle
		: MarshalByRefObject, IDisposable
	{
		private static int WaitMultiple (WaitHandle[] safeWaitHandles, int count, int millisecondsTimeout, bool waitAll)
		{
			int release_last = -1;

			try
			{
				for (int i = 0; i < count; ++i)
				{
					try
					{
					}
					finally
					{
						/* we have to put it in a finally block, to avoid having a ThreadAbortException
						 * between the return from DangerousAddRef and the assignement to release_last */
						bool release = false;
						safeWaitHandles[i].SafeWaitHandle.DangerousAddRef (ref release);
						release_last = i;
					}
				}

				if (waitAll)
					return WaitAll_internal (safeWaitHandles, millisecondsTimeout);
				else
					return WaitAny_internal (safeWaitHandles, millisecondsTimeout);
			}
			finally
			{
				for (int i = release_last; i >= 0; --i)
				{
					safeWaitHandles[i].SafeWaitHandle.DangerousRelease ();
				}
			}
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		private static extern int WaitAll_internal (WaitHandle[] handles, int ms);

		[MethodImpl (MethodImplOptions.InternalCall)]
		private static extern int WaitAny_internal (WaitHandle[] handles, int ms);

		static int WaitOneNative(IntPtr handle, int millisecondsTimeout)
		{
			return WaitOne_internal (handle, millisecondsTimeout);
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern int WaitOne_internal (IntPtr handle, int ms);

		static int SignalAndWaitOne (SafeWaitHandle waitHandleToSignal,SafeWaitHandle waitHandleToWaitOn, int millisecondsTimeout, bool hasThreadAffinity,  bool exitContext)
		{
			bool releaseHandleToSignal = false, releaseHandleToWaitOn = false;
			try {
				waitHandleToSignal.DangerousAddRef (ref releaseHandleToSignal);
				waitHandleToWaitOn.DangerousAddRef (ref releaseHandleToWaitOn);

				return SignalAndWait_Internal (waitHandleToSignal.DangerousGetHandle (), waitHandleToWaitOn.DangerousGetHandle (), millisecondsTimeout);
			} finally {
				if (releaseHandleToSignal)
					waitHandleToSignal.DangerousRelease ();
				if (releaseHandleToWaitOn)
					waitHandleToWaitOn.DangerousRelease ();
			}
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern int SignalAndWait_Internal (IntPtr toSignal, IntPtr toWaitOn, int ms);
	}
}
