using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Controllers
{
    public class InputController : GameBase
    {
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
        }
    }
}
