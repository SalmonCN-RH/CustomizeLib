using BepInEx.Unity.IL2CPP;
using BepInEx;
using System.Reflection;
using HarmonyLib;

namespace RogueShootingSeaShroom.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.seashroom", "RogueShootingSeaShroom", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            ShootingSeaShroom.SeaShroomCore.OnLoad();
            ShootingUltimateSeaShroom.UltimateSeaShroomCore.OnLoad();
        }
    }
}
