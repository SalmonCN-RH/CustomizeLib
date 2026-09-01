using CustomizeLib.BepInEx.Hook;
using CustomizeLib.BepInEx.ToolInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.LoadEvent
{
    internal static class CoreOnLoad
    {
        public static void OnLoad()
        {
            EventListenr.AddListener(ListenerType.OnGameLaunch, ApplyNativeHookTools.RunAll);
        }
    }
}
