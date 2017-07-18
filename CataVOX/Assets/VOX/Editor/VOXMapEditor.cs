using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.VOX.Editor
{
    [CustomEditor(typeof(VOXMap))]
    public class VOXMapEditor : UnityEditor.Editor
    {
        private VOXMap _script;

        public void OnEnable()
        {
            _script = (VOXMap)target;
        }

    }

}
