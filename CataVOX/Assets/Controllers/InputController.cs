using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Controllers
{
    public class InputPair
    {
        public InputPair(Func<bool> check, string command)
        {
            Check = check;
            Command = command;
        }
        public Func<bool> Check { get; set; }
        public string Command { get; set; }
    }
    public class InputController : GameBase
    {
        private static readonly List<InputPair> _commands = new List<InputPair>()
        {
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad7), "Move:NW"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad8), "Move:N"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad9), "Move:NE"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad6), "Move:E"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad3), "Move:SE"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad2), "Move:S"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad1), "Move:SW"),
            new InputPair(() => Input.GetKeyDown(KeyCode.Keypad4), "Move:W")
        };

        public void Update()
        {
            var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                Game.Camera.AdjustZoom(zoomDelta);
            }

            var rotationDelta = Input.GetAxis("Rotation");
            if (rotationDelta != 0f)
            {
                Game.Camera.AdjustRotation(rotationDelta);
            }

            var xDelta = Input.GetAxis("Horizontal");
            var zDelta = Input.GetAxis("Vertical");
            if (xDelta != 0f || zDelta != 0f)
            {
                Game.Camera.AdjustPosition(xDelta, zDelta);
            }

            var cmd = _commands.FirstOrDefault(x => x.Check());
            if (cmd != null)
            {
                Game.Loader.ProcessMapData(Game.SendCommand(cmd.Command));
            }
        }
    }
}
