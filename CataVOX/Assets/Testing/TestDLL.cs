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
		IVector3 ppos = DDA.playerPos();
		int size = 5;
		IVector2 from = new IVector2 (ppos.x - size, ppos.y - size);
		IVector2 to = new IVector2 (ppos.x + size, ppos.y + size);
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
