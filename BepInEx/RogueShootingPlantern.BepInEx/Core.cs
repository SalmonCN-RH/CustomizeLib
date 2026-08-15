using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace RogueShootingPlantern.BepInEx
{
    [BepInPlugin("salmon.rogueshooting.plantern", "RogueShootingPlantern", "1.0.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            ShootingPlantern.PlanternCore.OnLoad();
            ShootingUltimatePlantern.UltimatePlanternCore.OnLoad();
            ShootingGoldThreePlantern.GoldThreePlanternCore.OnLoad();
        }
    }
}
