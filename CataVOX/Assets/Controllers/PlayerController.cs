using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Controllers
{
    public class PlayerController : GameBase
    {
        private GameObject _player;

        public void Start()
        {
            CreatePlayer(Game.Global_Scale);
        }

        private void CreatePlayer(float scale)
        {
            _player = VOXGameObject.CreateGameObject("Assets/tiles/player.vox", scale);
            _player.name = "player";
            _player.transform.parent = this.transform;
        }

        public void Reload()
        {

            GameObject.Destroy(_player);
            CreatePlayer(GameMaster.Current.Global_Scale);
        }
    }
}
