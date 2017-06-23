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

        public void Update()
        {
            var input = _moveControls.FirstOrDefault(x => x.Check());
            if (input != null)
            {
                var dir = input.Value;
                var newDir = DirectionUtils.ModMoveRelCamera(Game.Camera.Facing, dir);
                Debug.Log(string.Format("cD: {0}, mD: {1}, nD: {2}", (int)Game.Camera.Facing, (int)dir, (int)newDir));
                var response = Game.SendCommand(string.Format("Move:{0}", Enum.GetName(typeof(Direction), newDir)));
                if (response == null) return;
                Game.Loader.ProcessMapData(response);
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                
            }
        }


        private static readonly List<InputPair<Direction>> _moveControls = new List<InputPair<Direction>>()
        {
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad7), Direction.NW),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad8), Direction.N),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad9), Direction.NE),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad6), Direction.E),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad3), Direction.SE),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad2), Direction.S),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad1), Direction.SW),
            new InputPair<Direction>(() => Input.GetKeyDown(KeyCode.Keypad4), Direction.W)
        };
    }
}
