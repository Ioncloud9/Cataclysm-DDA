using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestDLL : MonoBehaviour {
    [DllImport("Cataclysm", EntryPoint="testSum")]
    public static extern int testSum(int a, int b);
    [DllImport("Cataclysm", EntryPoint="loadGame")]
    public static extern int loadGame();
	// Use this for initialization
	void Start () {
		Debug.Log(testSum(39,3));
        Debug.Log(loadGame());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
