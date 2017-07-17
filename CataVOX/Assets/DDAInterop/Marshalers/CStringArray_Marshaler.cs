using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.DDAInterop.Marshalers
{
    public class CStringArray_Marshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new CStringArray_Marshaler();
        }
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            CStringArray arr = (CStringArray)Marshal.PtrToStructure(pNativeData, typeof(CStringArray));
            string[] managedStringArray = new string[arr.len];
            IntPtr[] pIntPtrArray = new IntPtr[arr.len];
            Marshal.Copy(arr.stringArray, pIntPtrArray, 0, arr.len);
            for (int i = 0; i < arr.len; i++)
            {
                managedStringArray[i] = Marshal.PtrToStringAnsi(pIntPtrArray[i]);
            }
            return managedStringArray;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            CStringArray arr = (CStringArray)Marshal.PtrToStructure(pNativeData, typeof(CStringArray));
            IntPtr[] pIntPtrArray = new IntPtr[arr.len];
            Marshal.Copy(arr.stringArray, pIntPtrArray, 0, arr.len);
            for (int i = 0; i < arr.len; i++)
            {
                Marshal.FreeCoTaskMem(pIntPtrArray[i]);
            }
            if (arr.len > 0)
            {
                Marshal.FreeCoTaskMem(arr.stringArray);
            }
            Marshal.FreeCoTaskMem(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            // don't send string arrays to c++ yet
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(CStringArray));
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            // don't send string arrays to c++ yet
            return IntPtr.Zero;
        }
    }
}
