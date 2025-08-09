using MelonLoader;

[assembly: MelonInfo(typeof(InfiniteNut.Core), "InfiniteNut", "1.0.0", "Administrator", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace InfiniteNut
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}