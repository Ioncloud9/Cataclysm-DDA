using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Inputs
{
    /// <summary>
    /// Represents a group of similar bindings, eg a group of KeyCode bindings
    /// </summary>
    /// <typeparam name="T">Type of binding</typeparam>
    public class BindingGroup<T> : Dictionary<T, BindingBase<T>>, IBindingGroup
    {
        public Type KeyType { get { return typeof(T); } }

        public IEnumerable<IBinding> GetBindings
        {
            get { return this.Values.Cast<IBinding>(); }
        }

        public void Add(BindingBase<T> binding)
        {
            base.Add(binding.InputBinding, binding);
        }
    }
}
