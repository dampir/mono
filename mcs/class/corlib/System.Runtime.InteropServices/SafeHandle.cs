//
// System.Runtime.InteropServices.SafeHandle
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
// Notes:
//     This code is only API complete, but it lacks the runtime support
//     for CriticalFinalizerObject and any P/Invoke wrapping that might
//     happen.
//
//     For details, see:
//     http://blogs.msdn.com/cbrumme/archive/2004/02/20/77460.aspx
//
//     CER-like behavior is implemented for Close and DangerousAddRef
//     via the try/finally uninterruptible pattern in case of async
//     exceptions like ThreadAbortException.
//
// On implementing SafeHandles:
//     http://blogs.msdn.com/bclteam/archive/2005/03/15/396335.aspx
//
// Issues:
//
//     TODO: Although DangerousAddRef has been implemented, I need to
//     find out whether the runtime performs the P/Invoke if the
//     handle has been disposed already.
//

//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// Files:
//  - mscorlib/system/runtime/interopservices/safehandle.cs
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Runtime.InteropServices
{
	[StructLayout (LayoutKind.Sequential)]
	public abstract partial class SafeHandle
	{
		const int RefCount_Mask = 0x7ffffffc;
		const int RefCount_One = 0x4;

		enum State {
			Closed = 0x00000001,
			Disposed = 0x00000002,
		}

		/*
		 * This should only be called for cases when you know for a fact that
		 * your handle is invalid and you want to record that information.
		 * An example is calling a syscall and getting back ERROR_INVALID_HANDLE.
		 * This method will normally leak handles!
		 */
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void SetHandleAsInvalid ()
		{
			try {}
			finally {
				int old_state, new_state;

				do {
					old_state = _state;
					new_state = old_state | (int) State.Closed;
				} while (Interlocked.CompareExchange (ref _state, new_state, old_state) != old_state);

				GC.SuppressFinalize (this);
			}
		}

		/*
		 * Add a reason why this handle should not be relinquished (i.e. have
		 * ReleaseHandle called on it). This method has dangerous in the name since
		 * it must always be used carefully (e.g. called within a CER) to avoid
		 * leakage of the handle. It returns a boolean indicating whether the
		 * increment was actually performed to make it easy for program logic to
		 * back out in failure cases (i.e. is a call to DangerousRelease needed).
		 * It is passed back via a ref parameter rather than as a direct return so
		 * that callers need not worry about the atomicity of calling the routine
		 * and assigning the return value to a variable (the variable should be
		 * explicitly set to false prior to the call). The only failure cases are
		 * when the method is interrupted prior to processing by a thread abort or
		 * when the handle has already been (or is in the process of being)
		 * released.
		 */
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public void DangerousAddRef (ref bool success)
		{
			try {}
			finally {
				if (!_fullyInitialized)
					throw new InvalidOperationException ();

				int old_state, new_state;

				do {
					old_state = _state;

					if ((old_state & (int) State.Closed) != 0)
						throw new ObjectDisposedException (null, "Safe handle has been closed");

					new_state = old_state + RefCount_One;
				} while (Interlocked.CompareExchange (ref _state, new_state, old_state) != old_state);

				success = true;
			}
		}

		/*
		 * Partner to DangerousAddRef. This should always be successful when used in
		 * a correct manner (i.e. matching a successful DangerousAddRef and called
		 * from a region such as a CER where a thread abort cannot interrupt
		 * processing). In the same way that unbalanced DangerousAddRef calls can
		 * cause resource leakage, unbalanced DangerousRelease calls may cause
		 * invalid handle states to become visible to other threads. This
		 * constitutes a potential security hole (via handle recycling) as well as a
		 * correctness problem -- so don't ever expose Dangerous* calls out to
		 * untrusted code.
		 */
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void DangerousRelease ()
		{
			DangerousReleaseInternal (false);
		}

		void InternalDispose ()
		{
			if (!_fullyInitialized)
				throw new InvalidOperationException ();

			DangerousReleaseInternal (true);
			GC.SuppressFinalize (this);
		}

		void InternalFinalize ()
		{
			if (_fullyInitialized)
				DangerousReleaseInternal (true);
		}

		// Do not call this directly - only call through the extension method SafeHandleExtensions.DangerousAddRef.
		internal void DangerousAddRef_WithNoNullCheck()
		{
			// To prevent handle recycling security attacks we must enforce the
			// following invariant: we cannot successfully AddRef a handle on which
			// we've committed to the process of releasing.

			// We ensure this by never AddRef'ing a handle that is marked closed and
			// never marking a handle as closed while the ref count is non-zero. For
			// this to be thread safe we must perform inspection/updates of the two
			// values as a single atomic operation. We achieve this by storing them both
			// in a single aligned DWORD and modifying the entire state via interlocked
			// compare exchange operations.

			// Additionally we have to deal with the problem of the Dispose operation.
			// We must assume that this operation is directly exposed to untrusted
			// callers and that malicious callers will try and use what is basically a
			// Release call to decrement the ref count to zero and free the handle while
			// it's still in use (the other way a handle recycling attack can be
			// mounted). We combat this by allowing only one Dispose to operate against
			// a given safe handle (which balances the creation operation given that
			// Dispose suppresses finalization). We record the fact that a Dispose has
			// been requested in the same state field as the ref count and closed state.

			// So the state field ends up looking like this:
			//
			//  31                                                        2  1   0
			// +-----------------------------------------------------------+---+---+
			// |                           Ref count                       | D | C |
			// +-----------------------------------------------------------+---+---+
			// 
			// Where D = 1 means a Dispose has been performed and C = 1 means the
			// underlying handle has (or will be shortly) released.


			// Might have to perform the following steps multiple times due to
			// interference from other AddRef's and Release's.
			int oldState, newState;
			do
			{
				// First step is to read the current handle state. We use this as a
				// basis to decide whether an AddRef is legal and, if so, to propose an
				// update predicated on the initial state (a conditional write).
				oldState = _state;

				// Check for closed state.
				if ((oldState & StateBits.Closed) != 0)
				{
					throw new ObjectDisposedException("SafeHandle");
				}

				// Not closed, let's propose an update (to the ref count, just add
				// StateBits.RefCountOne to the state to effectively add 1 to the ref count).
				// Continue doing this until the update succeeds (because nobody
				// modifies the state field between the read and write operations) or
				// the state moves to closed.
				newState = oldState + StateBits.RefCountOne;
			} while (Interlocked.CompareExchange(ref _state, newState, oldState) != oldState);
			// If we got here we managed to update the ref count while the state
			// remained non closed. So we're done.
		}

		// Bitmasks for the _state field above.
		private static class StateBits
		{
			public const int Closed = 0x00000001;
			public const int Disposed = 0x00000002;
			public const int RefCount = unchecked((int)0xfffffffc);
			public const int RefCountOne = 4;       // Amount to increment state field to yield a ref count increment of 1
		};

		void DangerousReleaseInternal (bool dispose)
		{
			try {}
			finally {
				if (!_fullyInitialized)
					throw new InvalidOperationException ();

				int old_state, new_state;

				/* See AddRef above for the design of the synchronization here. Basically we
				 * will try to decrement the current ref count and, if that would take us to
				 * zero refs, set the closed state on the handle as well. */
				bool perform_release = false;

				do {
					old_state = _state;

					/* If this is a Dispose operation we have additional requirements (to
					 * ensure that Dispose happens at most once as the comments in AddRef
					 * detail). We must check that the dispose bit is not set in the old
					 * state and, in the case of successful state update, leave the disposed
					 * bit set. Silently do nothing if Dispose has already been called
					 * (because we advertise that as a semantic of Dispose). */
					if (dispose && (old_state & (int) State.Disposed) != 0) {
						/* we cannot use `return` in a finally block, so we have to ensure
						 * that we are not releasing the handle */
						perform_release = false;
						break;
					}

					/* We should never see a ref count of zero (that would imply we have
					 * unbalanced AddRef and Releases). (We might see a closed state before
					 * hitting zero though -- that can happen if SetHandleAsInvalid is
					 * used). */
					if ((old_state & RefCount_Mask) == 0)
						throw new ObjectDisposedException (null, "Safe handle has been closed");

					if ((old_state & RefCount_Mask) != RefCount_One)
						perform_release = false;
					else if ((old_state & (int) State.Closed) != 0)
						perform_release = false;
					else if (!_ownsHandle)
						perform_release = false;
					else if (IsInvalid)
						perform_release = false;
					else
						perform_release = true;

					/* Attempt the update to the new state, fail and retry if the initial
					 * state has been modified in the meantime. Decrement the ref count by
					 * substracting SH_RefCountOne from the state then OR in the bits for
					 * Dispose (if that's the reason for the Release) and closed (if the
					 * initial ref count was 1). */
					new_state = (old_state & RefCount_Mask) - RefCount_One;
					if ((old_state & RefCount_Mask) == RefCount_One)
						new_state |= (int) State.Closed;
					if (dispose)
						new_state |= (int) State.Disposed;
				} while (Interlocked.CompareExchange (ref _state, new_state, old_state) != old_state);

				if (perform_release)
					ReleaseHandle ();
			}
		}
	}
}
