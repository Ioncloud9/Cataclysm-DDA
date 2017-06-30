using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;

namespace Assets.Controllers
{
    public class PlayerController : GameBase
    {
        private GameObject _pawn;

        public void Start()
        {
            CreatePlayer(Game.Global_Scale);
        }

        private void CreatePlayer(float scale)
        {
            _pawn = VOXGameObject.CreateGameObject("Assets/tiles/player.vox", scale);
            _pawn.name = "player";
            _pawn.transform.parent = this.transform;
        }

        public void Reload()
        {

            GameObject.Destroy(_pawn);
            CreatePlayer(GameMaster.Current.Global_Scale);
        }

        public void Update()
        {
            /*
            string ddaCommand = null;
            foreach (var map in _keyMap)
            {
                if (Input.GetKeyDown(map.Key))
                {
                    ddaCommand = map.Value;
                    break;
                }
            }
            if (_playerMove.ContainsKey(ddaCommand))
            {
                var dir = _playerMove[ddaCommand];
                var newDir = DirectionUtils.ModMoveRelCamera(Game.Camera.Facing, dir);
            }
            Game.SendCommand(ddaCommand);

            var input = _moveControls.FirstOrDefault(x => x.Check());

            if (input != null)
            {
                var dir = input.Value;
                var newDir = DirectionUtils.ModMoveRelCamera(Game.Camera.Facing, dir);
                Debug.Log(string.Format("cD: {0}, mD: {1}, nD: {2}", (int)Game.Camera.Facing, (int)dir, (int)newDir));
                Game.SendCommand(_cataMoveCommand[newDir]);
            }
            var otherInput = _otherControls.FirstOrDefault(x => x.Check());
            if (otherInput != null)
            {
                Game.SendCommand(otherInput.Value);
            }
            */
        }
    }
}
