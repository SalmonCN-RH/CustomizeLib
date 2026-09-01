using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.Utility
{
    public struct ResetSig(bool start = false)
    {
        private bool sig = start;

        public bool Reset()
        {
            var tmp = sig;
            sig = false;
            return tmp;
        }

        public void Set(bool val = true) => sig = val;

        public static implicit operator ResetSig(bool val) => new(val);
    }
}
