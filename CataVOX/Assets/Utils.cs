using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;

namespace Assets
{
    public static class DirectionUtils
    {


        public static Direction ModMoveRelCamera(Direction cameraDirection, Direction moveDirection)
        {
            //if (cameraDirection == Direction.N) return moveDirection;
            //if (moveDirection == Direction.N) return cameraDirection;
            //if (cameraDirection == moveDirection) return Direction.N;


            /*
             * 7  0  1
             * 6     2
             * 5  4  3
             * 
             * cameraDirection = 7
             * moveDirection = 2
             * 7 + 2 = 9
             * 9 - 8 = 1
             * return NE
             * 
             * cameraDirection = 7
             * moveDirection = 6
             * 7 + 6 = 13
             * 13 - 8 = 5
             * return SW
             */
            var offsetDir = (int)cameraDirection + (int)moveDirection;
            if (offsetDir > 7) offsetDir -= 8;
            return (Direction)offsetDir;
        }
    }
}
