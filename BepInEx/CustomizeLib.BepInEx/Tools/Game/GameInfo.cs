using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class GameInfo
    {
        public static int zombieLayer
        {
            get => LayerMask.GetMask("Zombie");
        }
    }
}
