using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            _data = DDA.GetGameData();
            needReload = true;
        }
        catch (Exception ex)
        {
            Debug.Log("Map.json not created yet");
        }
    }

    private void AddOrInstantiate(Vector3 loc, string id, string def)
    {
        if (id == null) return;
        float d = 0.0f;
        if (objects.ContainsKey(id))
        {
            //Debug.Log(String.Format("found object {0}, cloning", id));
            GameObject obj = Instantiate(objects[id], loc, Quaternion.identity, frame.transform);
            var bData = obj.GetComponent<BlockData>();
            bData.Location = loc;
            bData.id = id;
            bData.def = def;
            obj.SetActive(true);
        }
        else
        {
            //Debug.Log(String.Format("object {0} not found, loading", id));
            GameObject newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + id + ".vox", Game.Global_Scale);
            var bData = newObj.AddComponent<BlockData>();
            bData.Location = loc;
            bData.id = id;
            bData.def = def;
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

            GameObject obj = Instantiate(newObj, loc, Quaternion.identity, frame.transform);
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
            try
            {
                Debug.Log(_data.playerPosition);
                Game.Player.Reload(_data.playerPosition);
                Game.Camera.MoveTo(new Vector3(_data.playerPosition.x, 0, _data.playerPosition.z));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

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

            foreach (var tile in _data.map.tiles)
            {
                AddOrInstantiate(tile.loc, tile.ter == null ? "t_unseen" : tile.ter, "t_unknown");
                if (tile.furn != "f_null")
                {
                    AddOrInstantiate(tile.loc, tile.furn, "f_unknown");
                }
            }
            needReload = false;
        }
    }

    public class BlockData : MonoBehaviour
    {
        public Vector3 Location;
        public string id;
        public string def;
    }
}