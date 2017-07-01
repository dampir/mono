namespace System.Runtime
{
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Principal;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;


    public static class RuntimeImports
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void FreeHandle (int handle);

        [SuppressUnmanagedCodeSecurity]
        [HostProtection(Synchronization = true, ExternalThreading = true),
         ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool YeildInternal ();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void SpinWait_nop ();

        internal static IntPtr RhHandleAlloc (object value, GCHandleType type)
        {
            return (IntPtr) GCHandle.Alloc (value, type);
        }
        
        internal static Object RhHandleGet (IntPtr handle)
        {
            return (((GCHandle)handle).Target);
        }

        internal static void RhHandleFree (IntPtr handle)
        {
            FreeHandle ((int) handle);
        }

        internal static bool RhYield () { return YeildInternal (); }

        internal static void RhSpinWait (int iterations)
        {
            if (iterations < 0) return;
            while (iterations-- > 0)
            {
                SpinWait_nop ();
            }
        }

        internal static bool RhSetThreadExitCallback (IntPtr exitCallback)
        {
            return true;
        }

        internal static unsafe int RhCompatibleReentrantWaitAny (bool alterable, int timeout, int count, IntPtr* handles)
        {
            // TODO: Here must be used not implemeted function from corert PalCompatibleWaitAny
            // it uses CoWaitForMultipleHandles COM function under the hood.
            return 0x0;
        }
    }
}