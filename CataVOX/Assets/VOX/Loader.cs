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

public struct Label
{
    public string text;
    public Vector3 pos;
}

public class UnknownTile : MonoBehaviour
{

}

public class Loader : GameBase
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    GameObject frame, cached;
     Vector2 size = new Vector2(1, 1);
    bool needReload = false;
    private GameData _data;

    private void Start()
    {
        cached = new GameObject("cache");
        try
        {
            var sw = Stopwatch.StartNew();
            sw.Stop();
            Debug.Log(string.Format("Request sent in {0}ms", sw.ElapsedMilliseconds));
            _data = DDA.GetGameData();
            needReload = true;
        }
        catch (Exception ex)
        {
            Debug.Log("Map.json not created yet");
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
            _data = DDA.GetGameData();
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
            try
            {
                Game.UI.SetUI(_data.weather, _data.calendar);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            Debug.Log("reloading...");
            GameObject.Destroy(frame);
            frame = new GameObject("frame");
            frame.transform.parent = this.gameObject.transform;

            int i = 0;
            for (int y = 0; y < _data.map.height; y++)
            {
                for (int x = 0; x < _data.map.width; x++)
                {
                    
                    AddOrInstantiate(x, y, _data.map.tiles[i].ter == null ? "t_unseen" : _data.map.tiles[i].ter, "t_unknown");
                    if (_data.map.tiles[i].furn != "f_null")
                    {
                        AddOrInstantiate(x, y, _data.map.tiles[i].furn, "f_unknown");
                    }
                    i++;
                }
            }
            frame.transform.SetPositionAndRotation(new Vector3(-_data.map.width / 2 * size.x, 0, -_data.map.height / 2 * size.y), Quaternion.identity);
            needReload = false;
        }
    }
}