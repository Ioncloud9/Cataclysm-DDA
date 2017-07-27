using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.VOX.Editor
{
    [CustomEditor(typeof(Map))]
    public class MapEditor : UnityEditor.Editor
    {
        //private Map _script;

        public void OnEnable()
        {
          //  _script = (Map)target;
        }

    }

}
