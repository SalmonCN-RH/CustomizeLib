using MelonLoader;

[assembly: MelonInfo(typeof(UltimateWinterMelonGargantuar.Core), "UltimateWinterMelonGargantuar", "1.0.0", "Administrator", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateWinterMelonGargantuar
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}