using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Inputs
{
    public class DDABindings
    {
        /// <summary>
        /// Splits bindings up into their component groups, Keyboard input would be KeyCode, Mouse input would be MouseButton, etc...
        /// </summary>
        private static readonly Dictionary<Type, IBindingGroup> _bindingGroups = new Dictionary<Type, IBindingGroup>()
        {
            { typeof(KeyCode), KeyboardBinding.GetAllKeyBinds() }
        };

        /// <summary>
        /// Pools all the binding's Check lambdas into 1 dictionary in order to make input checking faster.
        /// </summary>
        private static readonly Dictionary<Func<bool>, IBinding> _checkCache = new Dictionary<Func<bool>, IBinding>();

        public DDABindings()
        {
            foreach (var group in _bindingGroups.Values)
            {
                foreach (var binding in group.GetBindings)
                {
                    _checkCache.Add(binding.Check, binding);
                }
            }
        }

        /// <summary>
        /// Executes the _checkCache to find if any Unity Input has triggered and returns the binding
        /// </summary>
        /// <returns>The binding associated with a triggered unity input, or null if none found</returns>
        public IBinding GetInputCommand()
        {
            KeyValuePair<Func<bool>, IBinding>? input = _checkCache.FirstOrDefault(x => x.Key());
            if (!input.HasValue) return null;
            return input.Value.Value;
        }

        /// <summary>
        /// Conveniance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BindingBase<T> GetInputCommand<T>()
        {
            var input = GetInputCommand();
            if (input == null) return null;
            return (BindingBase<T>)input;
        }

        /// <summary>
        /// DDA command lookup based on the Unity Input
        /// </summary>
        /// <typeparam name="T">Type of input to look up (eg: KeyCode)</typeparam>
        /// <param name="inputValue">Input value to look up (eg: KeyCode.Keypad1)</param>
        /// <returns>The DDA command associated with that Input, or null if not found</returns>
        public string CommandFromInput<T>(T inputValue)
        {
            IBindingGroup group;
            if (!_bindingGroups.TryGetValue(typeof(T), out group)) return null;
            BindingBase<T> binding;
            if (!((BindingGroup<T>)group).TryGetValue(inputValue, out binding))
            {
                return null;
            }
            return binding.DDACommand;
        }

        /// <summary>
        /// Reverse lookup, get bound Unity Input from DDA Command
        /// </summary>
        /// <typeparam name="T">Type of input to look up (eg: KeyCode)</typeparam>
        /// <param name="command">DDACommand to find the binding for</param>
        /// <returns>The binding, if found, or null if not found</returns>
        public BindingBase<T> InputFromCommand<T>(string command)
        {
            IBindingGroup group;
            if (!_bindingGroups.TryGetValue(typeof(T), out group)) return null;
            var tGroup = (BindingGroup<T>)group;
            var binding = tGroup.Values.FirstOrDefault(x => x.DDACommand == command);
            if (binding == null) return null;
            return binding;
        }
    }
}
