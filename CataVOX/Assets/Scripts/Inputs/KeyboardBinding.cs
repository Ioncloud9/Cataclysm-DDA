using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Inputs
{
    /// <summary>
    /// Keyboard bindings
    /// </summary>
    public class KeyboardBinding : BindingBase<KeyCode>
    {
        public KeyboardBinding(KeyCode code, string ddaCommand) : base(() => Input.GetKeyDown(code), ddaCommand, code) { }

        //Move keys are relative to the camera. In DDA move_n is always north; however, in unity if the camera is facing SE Keypad8 is now Keypad1 move_se and not move_n
        public override string DDACommand
        {
            get
            {
                var newKey = MoveTransform(InputBinding);
                if (newKey == InputBinding) return base.DDACommand;
                var newCmd = _keybinds.Where(x => x.Value.InputBinding == newKey).Select(x => x.Value).FirstOrDefault();
                if (newCmd == null) return base.DDACommand;
                return newCmd.DDACommand;
            }
        }

        public static BindingGroup<KeyCode> GetAllKeyBinds()
        {
            return _keybinds;
        }

        /// <summary>
        /// Maps Keypad input to a Directional
        /// </summary>
        public static List<Tuple<KeyCode, Direction>> KeyDirection = new List<Tuple<KeyCode, Direction>>()
        {
            new Tuple<KeyCode, Direction>(KeyCode.Keypad1, Direction.SW),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad2, Direction.S),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad3, Direction.SE),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad4, Direction.W),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad6, Direction.E),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad7, Direction.NW),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad8, Direction.N),
            new Tuple<KeyCode, Direction>(KeyCode.Keypad9, Direction.NE)

        };

        /// <summary>
        /// Maps keypad input to a command
        /// </summary>
        private static BindingGroup<KeyCode> _keybinds = new BindingGroup<KeyCode>()
        {
            {new KeyboardBinding(KeyCode.Keypad1, "move_sw")},
            {new KeyboardBinding(KeyCode.Keypad2, "move_s")},
            {new KeyboardBinding(KeyCode.Keypad3, "move_se")},
            {new KeyboardBinding(KeyCode.Keypad4, "move_w")},
            {new KeyboardBinding(KeyCode.Keypad5, "wait")},
            {new KeyboardBinding(KeyCode.Keypad6, "move_e")},
            {new KeyboardBinding(KeyCode.Keypad7, "move_nw")},
            {new KeyboardBinding(KeyCode.Keypad8, "move_n")},
            {new KeyboardBinding(KeyCode.Keypad9, "move_ne")}
        };

        /// <summary>
        /// Maps keypad input to a transform
        /// </summary>
//        private static Dictionary<KeyCode, Func<KeyCode, KeyCode>> _keyTransforms = new Dictionary<KeyCode, Func<KeyCode, KeyCode>>()
//        {
//            {KeyCode.Keypad1, MoveTransform },
//            {KeyCode.Keypad2, MoveTransform },
//            {KeyCode.Keypad3, MoveTransform },
//            {KeyCode.Keypad4, MoveTransform },
//            {KeyCode.Keypad6, MoveTransform },
//            {KeyCode.Keypad7, MoveTransform },
//            {KeyCode.Keypad8, MoveTransform },
//            {KeyCode.Keypad9, MoveTransform }
//        };

        /// <summary>
        /// Transforms the keypress into a different keypress based on the Camera's facing
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static KeyCode MoveTransform(KeyCode key)
        {
            var dir = DirectionFromInput(key);
            if (!dir.HasValue) return key;
            var newDir = DirectionUtils.ModMoveRelCamera(GameMaster.Current.Camera.Facing, dir.Value);
            var ret = InputFromDirection(newDir);
            if (ret.HasValue) return ret.Value;
            return key;
        }

        /// <summary>
        /// Get a Direction from a keypress
        /// </summary>
        /// <param name="key">The keypress</param>
        /// <returns>The direction associated with the keypress</returns>
        public static Direction? DirectionFromInput(KeyCode key)
        {
            var item = KeyDirection.FirstOrDefault(x => x.Item1 == key);
            if (item == null) return null;
            return item.Item2;
        }

        /// <summary>
        /// Get a keypress from a direction
        /// </summary>
        /// <param name="dir">The direction</param>
        /// <returns>The keypress associated with the direction</returns>
        public static KeyCode? InputFromDirection(Direction dir)
        {
            var item = KeyDirection.FirstOrDefault(x => x.Item2 == dir);
            if (item == null) return null;
            return item.Item1;
        }
    }
}
