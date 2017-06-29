using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Inputs
{
    /// <summary>
    /// Binds a unity input to a DDA command
    /// </summary>
    /// <typeparam name="T">Binding type</typeparam>
    public abstract class BindingBase<T> : IBinding
    {
        protected BindingBase(Func<bool> check, string ddaCommand, T inputBinding = default(T))
        {
            Check = check;
            DDACommand = ddaCommand;
            InputBinding = inputBinding;
        }
        public virtual Func<bool> Check { get; private set; }
        public virtual string DDACommand { get; private set; }
        public virtual T InputBinding { get; set; }
    }
}
