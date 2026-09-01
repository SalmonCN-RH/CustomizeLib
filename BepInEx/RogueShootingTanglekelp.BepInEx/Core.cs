using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace RogueShootingTanglekelp.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.tanglekelp", "RogueShootingTanglekelp", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // 类型初始化
            TangleKelp.TangleKelpCore.OnLoad();
            UltimateKelp.UltimateKelp.OnLoad();
        }
    }
}
