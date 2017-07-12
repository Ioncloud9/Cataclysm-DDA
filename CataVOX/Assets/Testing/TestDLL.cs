using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.Text;

public class TestDLL : MonoBehaviour
{
    void Start()
    {
        DDA.init(true);
		Debug.Log(DDA.playerPos());
		// GameData data = GetGameData();
		// Tile tile = data.map.tileAt (5, 5);
		// Debug.Log (string.Format("terrain at ({0}, {1}, {2}) is {3}", tile.loc.x, tile.loc.y, tile.loc.z, tile.ter));
		// doAction ("move_e");
		// data = GetGameData ();
		// tile = data.map.tileAt (5, 5);
		// Debug.Log (string.Format("terrain at ({0}, {1}, {2}) is {3}", tile.loc.x, tile.loc.y, tile.loc.z, tile.ter));
    }

    void OnApplicationQuit()
    {
        DDA.deinit();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
