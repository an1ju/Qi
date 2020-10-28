using System;
using System.Collections.Generic;
using System.Text;

namespace OmegaIDEA.Public
{
    public class Settings
    {
        public Equipment TheMachine { get; set; } = null;

        public List<Equipment> TheBranch { get; set; } = null;

        public Settings()
        {
            if (TheMachine == null)
                TheMachine = new Equipment();
            if (TheBranch == null)
                TheBranch = new List<Equipment>();
        }
    }

    public class Equipment
    {
        public string Parent { get; set; } = "";
        public string IP { get; set; }
        public int Port { get; set; }
    }
}
