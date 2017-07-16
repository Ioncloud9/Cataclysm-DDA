using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{

    public RectTransform GameList;

    private List<GameObject> _createdObjects;
    private string CurrentWorld;

	// Use this for initialization
	void Start () {
	    DDA.init(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnApplicationQuit()
    {
        DDA.deinit();
    }

    public void OnLoadClick()
    {
        DestroyObjs();
        CurrentWorld = null;
        _createdObjects = CreateButtonList(DDA.GetWorldNames(), OnWorldClick);
    }

    public void OnWorldClick(string world)
    {
        DestroyObjs();
        CurrentWorld = world;
        _createdObjects.Add(CreateButton("<--", x => OnLoadClick()));
        _createdObjects.AddRange(CreateButtonList(DDA.GetWorldSaves(world), DoSceneTransition));
    }

    public void DoSceneTransition(string saveName)
    {
        DDA.loadSaveGame(CurrentWorld, saveName);
        SceneManager.LoadScene("main");
    }

    private void DestroyObjs()
    {
        if (_createdObjects != null)
        {
            foreach (var obj in _createdObjects) Destroy(obj);
        }
        _createdObjects = new List<GameObject>();
    }
    private List<GameObject> CreateButtonList(IEnumerable<string> names, Action<string> callback)
    {
        var objs = new List<GameObject>();
        foreach (var name in names)
        {
            objs.Add(CreateButton(name, callback));
        }
        return objs;
    }

    private GameObject CreateButton(string name, Action<string> callback)
    {
        var pfb = (GameObject)Instantiate(Resources.Load("Prefab/UI/mainSaveButton"));
        pfb.name = name;
        pfb.transform.parent = GameList.transform;
        var btn = pfb.GetComponent<Button>();
        btn.onClick = new Button.ButtonClickedEvent();
        btn.onClick.AddListener(() => callback(name));

        var txt = pfb.GetComponentInChildren<Text>();
        txt.text = name;
        return pfb;
    }

    public void OnNewClick()
    {
        
    }
}
