using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Net.Sockets;
using Assets;

[Serializable]
class Map
{
    public int width;
    public int height;
    public Tile[] tiles;
    public Map(int width, int height, Tile[] tiles)
    {
        this.width = width;
        this.height = height;
        this.tiles = tiles;
    }
}

[Serializable]
class Tile
{
    public string ter;
    public string furn;
    public Tile(string terrain, string furniture)
    {
        this.ter = terrain;
        this.furn = furniture;
    }
}

struct Label
{
    public string text;
    public Vector3 pos;
}

class UnknownTile : MonoBehaviour
{

}

class Loader : GameBase
{
    Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
    GameObject frame, cached;
    List<Label> labels = new List<Label>();
    Vector2 size = new Vector2(1, 1);
    bool needReload = false;
    bool quitting = false;
    string mapJSON, paintedJSON;
    Thread client;

    void Start()
    {
        cached = new GameObject("cache");
        client = new Thread(new ThreadStart(this.RequestMapJSON));
        client.Start();
        try
        {
            mapJSON = File.ReadAllText("Assets/map.json");
        }
        catch (Exception ex)
        {
            Debug.Log("Map.json not created yet");
        }
    }

    void OnApplicationQuit()
    {
        //Debug.Log("exit");
        quitting = true;
        NetMQConfig.Cleanup();
    }

    private void RequestMapJSON()
    {
		try 
		{
	        using (var subscriber = new SubscriberSocket("tcp://localhost:3332"))
	        {
	            string prevMap = null;
	            subscriber.Subscribe("");
	            while (!quitting)
	            {
	                mapJSON = subscriber.ReceiveFrameString();
	                if (prevMap != mapJSON) {
	                    needReload = true;
	                }
	                prevMap = mapJSON;
	            }
	        }
		} catch(SocketException)
		{
			Debug.Log ("Socket exception");
		}
        catch (Exception)
        {
            Debug.Log("Some exception");
        }
        NetMQConfig.Cleanup();
    }

    void AddOrInstantiate(float x, float y, string id, string def)
    {
        if (id == null) return;
        float d = 0.0f;
        if (objects.ContainsKey(id))
        {
            //Debug.Log(String.Format("found object {0}, cloning", id));
            GameObject obj = Instantiate(objects[id], new Vector3(x * size.x, 0, y * size.y), Quaternion.identity, frame.transform);
            obj.SetActive(true);

            if (objects[id].GetComponent<UnknownTile>() != null)
            {
                Label label;
                label.pos = new Vector3(x * size.x, 0, y * size.y);
                label.text = id;
                labels.Add(label);
            }

        }
        else
        {
            //Debug.Log(String.Format("object {0} not found, loading", id));
            GameObject newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + id + ".vox", Game.Global_Scale);
            if (VOXGameObject.model.size.x == 0 &&
                VOXGameObject.model.size.y == 0 &&
                VOXGameObject.model.size.z == 0)
            {
                //Debug.Log(String.Format("object {0} has not been found, creating unknow instead", id));
                GameObject.Destroy(newObj);
                newObj = VOXGameObject.CreateGameObject("Assets/tiles/" + def + ".vox", Game.Global_Scale);
                newObj.AddComponent<UnknownTile>();
                if (VOXGameObject.model.size.x == 0 &&
                VOXGameObject.model.size.y == 0 &&
                VOXGameObject.model.size.z == 0)
                {
                    //Debug.Log(String.Format("unknow has not been found", id));
                    return;
                }
            }
            newObj.transform.parent = cached.transform;
            newObj.SetActive(false);
            newObj.name = id;

            objects[id] = newObj;
            size.x = VOXGameObject.model.size.x * (VOXGameObject.scale + d);
            size.y = VOXGameObject.model.size.z * (VOXGameObject.scale + d);


            if (newObj.GetComponent<UnknownTile>() != null)
            {
                Label label;
                label.pos = new Vector3(x * size.x , 0, y * size.y);
                label.text = id;
                labels.Add(label);
                //Debug.Log(String.Format("added label {0}", id));
            }
            GameObject obj = Instantiate(newObj, new Vector3(x * size.x, 0, y * size.y), Quaternion.identity, frame.transform);
            obj.SetActive(true);
        }
    }

    void OnGUI()
    {
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        foreach (Label label in labels)
        {
            Vector3 pos = Game.Camera.MainCamera.WorldToScreenPoint(label.pos);
            GUI.Label(new Rect(pos.x - 150 / 2, Screen.height - pos.y - 130 / 2, 150, 130), label.text, centeredStyle);
        }
    }

    void Update()
    {
        if (mapJSON == null) return;
        if (paintedJSON != mapJSON)
        {
            needReload = true;
        }

        if (Input.GetKeyUp(KeyCode.F5))
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
            needReload = true;
            return;
        }
        if (needReload)
        {
            Map map = JsonUtility.FromJson<Map>(mapJSON);
            Debug.Log("reloading...");
            labels = new List<Label>();
            GameObject.Destroy(frame);
            frame = new GameObject("frame");
            frame.transform.parent = this.gameObject.transform;

            int i = 0;
            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    AddOrInstantiate(x, y, map.tiles[i].ter == null ? "t_unseen" : map.tiles[i].ter, "t_unknown");
                    AddOrInstantiate(x, y, map.tiles[i].furn, "f_unknown");
                    i++;
                }
            }
            frame.transform.SetPositionAndRotation(new Vector3(-map.width / 2 * size.x, 0, -map.height / 2 * size.y), Quaternion.identity);
            needReload = false;
            paintedJSON = mapJSON;
        }
    }
}