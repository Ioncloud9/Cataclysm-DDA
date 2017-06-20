using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class GameBase : MonoBehaviour
    {
        public GameMaster Game
        {
            get { return GameMaster.Current; }
        }
    }
}
