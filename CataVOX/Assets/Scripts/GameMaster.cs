using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Controllers;
using NetMQ;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameMaster : MonoBehaviour
    {
        private static readonly object _locker = new object();
        private static GameMaster _instance;

        private zMQThread _zmqClient = new zMQThread();
        
        public float Global_Scale = 0.4f;


        public void Awake()
        {
            Camera = FindObjectOfType<CameraController>();
            Input = FindObjectOfType<InputController>();
            Player = FindObjectOfType<PlayerController>();
            UI = FindObjectOfType<UIController>();
            Loader = FindObjectOfType<Loader>();
            _instance = this;
            _zmqClient.OnMessageReceived += _zmqClient_OnMessageReceived;
            _zmqClient.Start();
        }

        public zMQResponse SendCommand(string command)
        {
            try
            {
                var response = _zmqClient.SendCommand(new zMQCommand(command));
                if (response == null || response.Data == "BUSY")
                {
                    Debug.Log("DDA refused command, command already processing");
                }
                return response;
            }
            catch (SendCommandException sce)
            {
                Debug.Log(string.Format("SendCommand failed with {0}", sce.Message));
            }
            return null;
        }

        private void _zmqClient_OnMessageReceived(zMQResponse response)
        {
            Debug.Log(string.Format("Received {0}", response));
            Loader.ProcessMapData(response.Data);
        }


        public static GameMaster Current
        {
            get { return _instance; }
        }

        public void OnApplicationQuit()
        {
            _zmqClient.Stop();
            NetMQConfig.Cleanup();
        }

        public CameraController Camera { get; private set; }
        public InputController Input { get; private set; }
        public PlayerController Player { get; private set; }
        public Loader Loader { get; private set; }
        public UIController UI { get; private set; }
    }
}
