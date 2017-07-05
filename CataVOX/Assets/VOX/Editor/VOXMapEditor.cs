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
            zMQResponse response;
            zMQResponse mqResponse = null;
            using (var receiver = new ResponseSocket(voxConst.LISTENER_URI))
            {
                using (var requestor = new RequestSocket(voxConst.SENDER_URI))
                {
                    requestor.SendFrame(string.Format("{0}:MapData", Guid.NewGuid()));
                    string res = requestor.ReceiveFrameString();
                    response = new zMQResponse(res);
                }
                var data = receiver.ReceiveFrameString();

                if (!zMQResponse.TryParse(data, out mqResponse))
                {
                    receiver.SendFrame("OK");
                }
                else
                {
                    receiver.SendFrame(string.Format("{0}:OK", response.ID));
                }
            }
            if (mqResponse == null) return null;
            return JsonUtility.FromJson<GameData>(mqResponse.Data);
        }

    }

}
