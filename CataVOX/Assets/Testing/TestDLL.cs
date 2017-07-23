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
		Vector3Int ppos = DDA.playerPos();
		int size = 5;
		Vector2Int from = new Vector2Int (ppos.x - size, ppos.y - size);
		Vector2Int to = new Vector2Int (ppos.x + size, ppos.y + size);
		Map map = DDA.GetTilesBetween (from, to);
		Debug.Log (map);
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
