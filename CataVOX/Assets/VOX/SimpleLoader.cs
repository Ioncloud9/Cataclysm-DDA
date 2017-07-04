using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Net.Sockets;
using Assets.Scripts;
using Debug = UnityEngine.Debug;

public class SimpleLoader: MonoBehaviour //GameBase
{
    public string tilesPath = "Assets/tiles";
    GameObject map;
    VOXMapVolume volume = null;
    bool needReload = false;
    string mapJSON, paintedJSON;

    private void Start()
    {
        try
        {
            // var sw = Stopwatch.StartNew();
            // var response = Game.SendCommand("MapData");
            // sw.Stop();
            //Debug.Log(string.Format("Request sent in {0}ms", sw.ElapsedMilliseconds));
            mapJSON = File.ReadAllText("Assets/map_big.json");
            //ProcessMapData(response.Data);
            ProcessMapData(mapJSON);
        }
        catch (Exception)
        {
            Debug.Log("Map.json not created yet");
        }
    }
    
    public void ProcessMapData(string message)
    {
        if (message != mapJSON)
        {
            mapJSON = message;
            needReload = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F5))
        {
            //Game.SendCommand("MapData");
            volume = null;
            return;
        }

        if (mapJSON == null) return;
        if (paintedJSON != mapJSON)
        {
            needReload = true;
        }

        if (needReload)
        {
            GameObject.Destroy(map);
            
            // Game.Player.Reload();
            // Game.Camera.MoveTo(Game.Player.transform.position);

            Debug.Log("reloading...");

            var gameData = JsonUtility.FromJson<GameData>(mapJSON);
            if (volume == null) {
                volume = new VOXMapVolume(gameData, tilesPath);
            } else {
                volume.Reload(gameData);
            }

            // Game.UI.SetUI(gameData.weather, gameData.calendar);
            var sw = Stopwatch.StartNew();
            map = volume.CreateMapMesh();
            sw.Stop();
          
            Debug.Log(string.Format("Mesh created in {0}ms", sw.ElapsedMilliseconds));

            map.name = "map";
            map.transform.parent = gameObject.transform;
            needReload = false;
            paintedJSON = mapJSON;
        }
    }
}