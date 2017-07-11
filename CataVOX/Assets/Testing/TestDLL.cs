﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;

public class TestDLL : MonoBehaviour
{
    [DllImport("Cataclysm", EntryPoint = "init")]
    public static extern void init();
    [DllImport("Cataclysm", EntryPoint = "deinit")]
    public static extern void deinit();
    [DllImport("Cataclysm", EntryPoint = "moveRight")]
    public static extern void moveRight();

    [DllImport("Cataclysm", EntryPoint = "doTurn")]
    public static extern void doTurn();

    [DllImport("Cataclysm", EntryPoint = "doAction")]
    public static extern void doAction(
        [MarshalAs(UnmanagedType.LPStr)] string action 
    );

    [DllImport("Cataclysm", EntryPoint = "getTurn")]
    public static extern int getTurn();

    [DllImport("Cataclysm", EntryPoint = "playerX")]
    public static extern int playerX();
    [DllImport("Cataclysm", EntryPoint = "playerY")]
    public static extern int playerY();

    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "loadGame")]
    public static extern void loadGame(
        [MarshalAs(UnmanagedType.LPStr)] string worldName
    );


    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getTer")]
    public static extern void getTer(
        out IntPtr ter
    );

    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getWorldNames")]
    public static extern void GetWorldNames(
        out IntPtr ar,
        out int size
    );

    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getWorldSaves")]
    public static extern void GetWorldSaves(
        [MarshalAs(UnmanagedType.LPStr)] string worldName,
        out IntPtr ar,
        out int size
    );

    void Start()
    {
        init();
        IntPtr ter = IntPtr.Zero;
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
        doAction("move_e");
        ter = IntPtr.Zero;
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
        doAction("move_e");
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
        doAction("move_e");
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
        doAction("move_e");
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
        doAction("move_e");
        getTer(out ter);
        Debug.Log(string.Format("ter: {0}, turn: {1}, pos: ({2}, {3})", Cpp2net_str(ter), getTurn(), playerX(), playerY()));
    }

    void OnApplicationQuit()
    {
        Debug.Log("onquit");
        deinit();
    }

    // Update is called once per frame
    void Update()
    {

    }


    static void Cpp2net_strArray(
        IntPtr pUnmanagedStringArray,
        int StringCount,
        out string[] ManagedStringArray
    )
    {
        if (StringCount == 0)
        {
            ManagedStringArray = new string[0];
            return;
        }
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

    static string Cpp2net_str(
        IntPtr pUnmanagedString
    ) {
        string result;
        result = Marshal.PtrToStringAnsi(pUnmanagedString);
        Marshal.FreeCoTaskMem(pUnmanagedString);
        return result;
    }
}
