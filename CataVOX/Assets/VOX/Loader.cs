using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Net.Sockets;
using Assets;
using Assets.Scripts;
using Debug = UnityEngine.Debug;

public class UnknownTile : MonoBehaviour
{

}

public class Loader : GameBase
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    GameObject frame, cached;
     Vector2 size = new Vector2(1, 1);
    bool needReload = false;
    string mapJSON, paintedJSON;

    private void Start()
    {
        cached = new GameObject("cache");
        try
        {
            var sw = Stopwatch.StartNew();
            var response = Game.SendCommand("MapData");
            sw.Stop();
            Debug.Log(string.Format("Request sent in {0}ms", sw.ElapsedMilliseconds));
            ProcessMapData(response.Data);
            mapJSON = File.ReadAllText("Assets/map.json");
        }
        catch (Exception ex)
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

    private void AddOrInstantiate(float x, float y, string id, string def)
    {
        if (id == null) return;
        float d = 0.0f;
        if (objects.ContainsKey(id))
        {
            //Debug.Log(String.Format("found object {0}, cloning", id));
            GameObject obj = Instantiate(objects[id], new Vector3(x * size.x, 0, y * size.y), Quaternion.identity, frame.transform);
            obj.SetActive(true);
        }
        else
        {
            //Debug.Log(String.Format("object {0} not found, loading", id));
            GameObject newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + id + ".vox", Game.Global_Scale);
            if (VOXGameObject.model.sizeX == 0 &&
                VOXGameObject.model.sizeY == 0 &&
                VOXGameObject.model.sizeZ == 0)
            {
                //Debug.Log(String.Format("object {0} has not been found, creating unknow instead", id));
                GameObject.Destroy(newObj);
                newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + def + ".vox", Game.Global_Scale);
                newObj.AddComponent<UnknownTile>();
                if (VOXGameObject.model.sizeX == 0 &&
                VOXGameObject.model.sizeY == 0 &&
                VOXGameObject.model.sizeZ == 0)
                {
                    //Debug.Log(String.Format("unknow has not been found", id));
                    return;
                }
            }
            newObj.transform.parent = cached.transform;
            newObj.SetActive(false);
            newObj.name = id;

            objects[id] = newObj;
            size.x = VOXGameObject.model.sizeX * (VOXGameObject.scale + d);
            size.y = VOXGameObject.model.sizeZ * (VOXGameObject.scale + d);

            GameObject obj = Instantiate(newObj, new Vector3(x * size.x, 0, y * size.y), Quaternion.identity, frame.transform);
            obj.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F5))
        {
            Game.SendCommand("MapData");
            return;
        }

        if (mapJSON == null) return;
        if (paintedJSON != mapJSON)
        {
            needReload = true;
        }

        if (needReload)
        {
            GameObject.Destroy(cached);
            cached = new GameObject("cache");
            foreach (var obj in objects)
            {
                GameObject.Destroy(obj.Value);
            }
            objects = new Dictionary<string, GameObject>();
            Game.Player.Reload();
            Game.Camera.MoveTo(Game.Player.transform.position);
            Debug.Log(mapJSON);
            var gameData = JsonUtility.FromJson<GameData>(mapJSON);
            Game.UI.SetUI(gameData.weather, gameData.calendar);
            Debug.Log("reloading...");
            GameObject.Destroy(frame);
            frame = new GameObject("frame");
            frame.transform.parent = this.gameObject.transform;

            int i = 0;
            for (int y = 0; y < gameData.map.height; y++)
            {
                for (int x = 0; x < gameData.map.width; x++)
                {
                    AddOrInstantiate(x, y, gameData.map.tiles[i].ter == null ? "t_unseen" : gameData.map.tiles[i].ter, "t_unknown");
                    AddOrInstantiate(x, y, gameData.map.tiles[i].furn, "f_unknown");
                    i++;
                }
            }
            frame.transform.SetPositionAndRotation(new Vector3(-gameData.map.width / 2 * size.x, 0, -gameData.map.height / 2 * size.y), Quaternion.identity);
            needReload = false;
            paintedJSON = mapJSON;
        }
    }
}