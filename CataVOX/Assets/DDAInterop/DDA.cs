using System;
using System.Runtime.InteropServices;

public class DDA
{
	[DllImport("Cataclysm", EntryPoint = "init")]
	public static extern void init(bool openMainMenu = false);

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

	[DllImport("Cataclysm", EntryPoint = "playerPos")]
	public static extern Vector3Int playerPos();

	[DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "loadGame")] // loads first save from the world
	public static extern void loadGame(
		[MarshalAs(UnmanagedType.LPStr)] string worldName
	);

    [DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "loadSaveGame")] // loads specific save
    public static extern void loadSaveGame(
        [MarshalAs(UnmanagedType.LPStr)] string worldName,
        [MarshalAs(UnmanagedType.LPStr)] string saveName
    );

    [DllImport("Cataclysm", EntryPoint = "getWorldNames")]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType= "Assets.DDAInterop.Marshalers.CStringArray_Marshaler")] 
	public static extern string[] GetWorldNames();

	[DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getWorldSaves")]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType= "Assets.DDAInterop.Marshalers.CStringArray_Marshaler")] 
	public static extern string[] GetWorldSaves(
		[MarshalAs(UnmanagedType.LPStr)] string worldName
	);

	[DllImport("Cataclysm", CharSet = CharSet.Auto, EntryPoint = "getGameData")]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType= "Assets.DDAInterop.Marshalers.CGameData_Marshaler")] 
	public static extern GameData GetGameData();

    [DllImport("Cataclysm", EntryPoint = "getTilesBetween", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Assets.DDAInterop.Marshalers.CMap_Marshaler")]
    public static extern Map GetTilesBetween(Vector2Int from, Vector2Int to);
}
