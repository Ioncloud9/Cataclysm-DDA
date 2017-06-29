using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{

    public class zMQCommand
    {
        public zMQCommand(string command) : this(Guid.NewGuid(), command)
        {
        }

        public zMQCommand(Guid id, string command)
        {
            ID = id;
            Command = command;
        }

        public Guid ID { get; set; }
        public string Command { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", ID, Command);
        }
    }
}
