using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Inputs
{
    /// <summary>
    /// Mouse bindings
    /// </summary>
    public class MouseButtonBinding : BindingBase<MouseButton>
    {
        public MouseButtonBinding(MouseButton button, string ddaCommand) : base(() => Input.GetMouseButtonDown((int)button), ddaCommand, button) { }
    }

    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }
}
