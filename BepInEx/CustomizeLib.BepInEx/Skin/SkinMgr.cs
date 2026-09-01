using CustomizeLib.BepInEx.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class SkinMgr
    {
        public static Dictionary<(PlantType pt, int index), Assembly> SkinScripts = new();

        public static bool IsPlantSkinEnable(PlantType plantType)
        {
            if (CustomCore.EnableSkin.ContainsKey(plantType))
                return CustomCore.EnableSkin[plantType];
            else
            {
                CustomCore.EnableSkin.Add(plantType, false);
                return false;
            }
        }

        public static void AddScript(PlantType pt, int index, string script)
        {
            var asm = SkinScript.GetCSharpScript(script);
            if (asm != null)
                SkinScripts[(pt, index)] = asm;
        }

        public static void RunScript(PlantType pt, int index, string name)
        {
            if (SkinScripts.TryGetValue((pt, index), out var asm))
            {
                SkinScript.CallMethod(asm, name);
            }
        }
    }
}
