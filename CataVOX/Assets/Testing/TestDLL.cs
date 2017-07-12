using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;

public class TestDLL : MonoBehaviour
{
    [DllImport("Cataclysm", EntryPoint = "init")]
	public static extern void init(bool openMainMenu = false);
    [DllImport("Cataclysm", EntryPoint = "mainMenu")]
	public static extern void mainMenu();    
	[DllImport("Cataclysm", EntryPoint = "noMainMenu")]
	public static extern void noMainMenu();
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

    [DllImport("Cataclysm", EntryPoint = "getWorldNames")]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="Marshalers.CStringArray_Marshaler")] 
    public static extern string[] GetWorldNames();

    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getWorldSaves")]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="Marshalers.CStringArray_Marshaler")] 
    public static extern string[] GetWorldSaves(
        [MarshalAs(UnmanagedType.LPStr)] string worldName
    );

	[DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getGameData")]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="Marshalers.CGameData_Marshaler")] 
	public static extern GameData GetGameData();

    void Start()
    {
        init(true);
		GameData data = GetGameData();
		Debug.Log (data.map.tileAt (5, 5).ter);
		doAction ("move_e");
		data = GetGameData ();
		Debug.Log (data.map.tileAt (5, 5).ter);
    }

    void OnApplicationQuit()
    {
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
