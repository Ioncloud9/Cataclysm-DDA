using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Controllers;
using UnityEngine;

namespace Assets
{
    public class GameMaster : MonoBehaviour
    {
        private static readonly object _locker = new object();
        private static GameMaster _instance;

        public float Global_Scale = 0.4f;

        public void Awake()
        {
            Camera = FindObjectOfType<CameraController>();
            Input = FindObjectOfType<InputController>();
            Player = FindObjectOfType<PlayerController>();
            _instance = this;
        }

        public static GameMaster Current
        {
            get { return _instance; }
        }

        public CameraController Camera { get; private set; }
        public InputController Input { get; private set; }
        public PlayerController Player { get; private set; }
    }
}
