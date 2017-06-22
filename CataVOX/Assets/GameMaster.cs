using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Controllers;
using NetMQ;
using UnityEngine;

namespace Assets
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
            Loader = FindObjectOfType<Loader>();
            _instance = this;
            _zmqClient.OnMessageReceived += _zmqClient_OnMessageReceived;
            _zmqClient.Start();
        }

        public void SendCommandAsync(string command, Action<string> callback)
        {
            _zmqClient.SendCommandAsync(command, callback);
        }
        public string SendCommand(string command)
        {
            return _zmqClient.SendCommand(command);
        }

        private void _zmqClient_OnMessageReceived(string messageType, string message)
        {
            switch (messageType.ToLower())
            {
                case "mapdata":
                    Loader.ProcessMapData(message);
                    break;
            }
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
    }
}
