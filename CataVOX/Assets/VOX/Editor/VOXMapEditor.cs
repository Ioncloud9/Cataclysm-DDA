using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using NetMQ;
using NetMQ.Sockets;
using UnityEditor;
using UnityEngine;

namespace Assets.VOX.Editor
{
    [CustomEditor(typeof(VOXMap))]
    public class VOXMapEditor : UnityEditor.Editor
    {
        private VOXMap _script;

        public void OnEnable()
        {
            _script = (VOXMap)target;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Gen"))
            {
                var data = GetData();
                if (data != null)
                {
                    _script.CreateMap(data);
                    _script.Render();
                }
            }
        }
        
        private GameData GetData()
        {
            return DDA.GetGameData();
        }

    }

}
