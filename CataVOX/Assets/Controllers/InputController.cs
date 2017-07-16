using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Inputs;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Controllers
{
    public class InputController : GameBase
    {
        private static DDABindings _inputBindings = new DDABindings();

        public DDABindings Bindings { get { return _inputBindings; } }
        public void Update()
        {
            var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                Game.Camera.AdjustZoom(zoomDelta);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Game.Camera.AdjustRotation(1);
                //Game.Camera.AdjustRotation(rotationDelta);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Game.Camera.AdjustRotation(-1);
            }

            var xDelta = Input.GetAxis("Horizontal");
            var zDelta = Input.GetAxis("Vertical");
            if (xDelta != 0f || zDelta != 0f)
            {
                Game.Camera.AdjustPosition(xDelta, zDelta);
            }

            var sw = Stopwatch.StartNew();
            var binding = Bindings.GetInputCommand();
            if (binding != null)
            {
                UnityEngine.Debug.Log(string.Format("Command for key is {0}", binding.DDACommand));
                DDA.doAction(binding.DDACommand);
            }
            sw.Stop();
            //UnityEngine.Debug.Log(string.Format("Found binding in {0}ms", sw.ElapsedMilliseconds));
        }
    }
}
