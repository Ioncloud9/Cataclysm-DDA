using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.DDAInterop.Marshalers
{
    public class IVector3_Marshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new IVector3_Marshaler();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            IVector3 vec = (IVector3)Marshal.PtrToStructure(pNativeData, typeof(IVector3));
            return vec;
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(IVector3));
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Marshal.FreeCoTaskMem(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            // don't send string arrays to c++ yet
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            // don't send string arrays to c++ yet
            return IntPtr.Zero;
        }
    }
}
