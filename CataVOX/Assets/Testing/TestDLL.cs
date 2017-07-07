using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestDLL : MonoBehaviour
{
    [DllImport("Cataclysm", EntryPoint = "init")]
    public static extern int init();

    [DllImport("Cataclysm", CharSet = CharSet.Ansi, EntryPoint = "getWorldNames")]
    // public static extern void GetWorldNames(
    //     [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStr, SizeParamIndex=1)] out string[] ar,
    //     out int size
    // );

    public static extern void GetWorldNames(
        out IntPtr ar,
        out int size
    );

    void Start()
    {
        init();
        IntPtr worlds = IntPtr.Zero;
        int size = 0;

        GetWorldNames(out worlds, out size);
        Debug.Log(size);

        string[] result = new string[size];
        MarshalUnmananagedStrArray2ManagedStrArray(worlds, size, out result);
        foreach (string world in result) {
             Debug.Log(world);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    static void MarshalUnmananagedStrArray2ManagedStrArray(
        IntPtr pUnmanagedStringArray,
        int StringCount,
        out string[] ManagedStringArray
    )
    {
        IntPtr[] pIntPtrArray = new IntPtr[StringCount];
        ManagedStringArray = new string[StringCount];

        Marshal.Copy(pUnmanagedStringArray, pIntPtrArray, 0, StringCount);

        for (int i = 0; i < StringCount; i++)
        {
            ManagedStringArray[i] = Marshal.PtrToStringAnsi(pIntPtrArray[i]);
            Marshal.FreeCoTaskMem(pIntPtrArray[i]);
        }

        Marshal.FreeCoTaskMem(pUnmanagedStringArray);
    }
}
