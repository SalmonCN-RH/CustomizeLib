using BepInEx;
using Core;
using CustomizeLib.BepInEx;
using HarmonyLib;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace UltimateWinterLevel.BepInEx
{
    [BepInPlugin("salmon.ultimatewinterlevel", "UltimateWinterLevel", "1.0")]
    public class Core : CorePlugin
    {
        public static int levelID = -1;
        public override void OnGameInit()
        {
            var customLevelData = new CustomLevelData();
            var boardTag = new Board.BoardTag();
            customLevelData.BoardTag = boardTag;
            customLevelData.Name = () => "我是僵尸·陨冬雪影";
            customLevelData.SceneType = SceneType.Snow;
            customLevelData.RowCount = 5;
            customLevelData.WaveCount = () => 40;
            customLevelData.BgmType = MusicType.Snow_boss;
            customLevelData.Logo = GameAPP.resourcesManager.zombieSprites[ZombieType.UltimateSnowZombie];
            customLevelData.ZombieList = () => new List<ZombieType>()
            {
                ZombieType.UltimateSnowZombie,
                ZombieType.SnowConeZombie,
                ZombieType.SnowDrownZombie,
                ZombieType.LevatationZombie,
                ZombieType.SnowShieldZombie,
                ZombieType.SnowGunZombie,
                ZombieType.SnowMonsterZombie,
                ZombieType.SuperSnowMonsterZombie,
                ZombieType.TallIceNutZombie,
                ZombieType.MiniSnowMonster
            };
            customLevelData.NeedSelectCard = false;
            levelID = CustomCore.RegisterCustomLevel(customLevelData);
        }
    }

    public class UltimateSnowZombieLevel : MonoBehaviour
    {
        public UltimateSnowZombie zombie => gameObject.GetComponent<UltimateSnowZombie>();
    }

    [HarmonyPatch(typeof(CheckAdv), nameof(CheckAdv.Start))]
    public static class CheckAdvPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CheckAdv __instance)
        {
            if (__instance.theLevel == Core.levelID && __instance.transform.FindChild("Window/Name").GetComponent<TextMeshProUGUI>().text == "我是僵尸·陨冬雪影")
            {
                __instance.transform.FindChild("Window/Name").GetComponent<TextMeshProUGUI>().text = "陨冬雪影";
            }
        }
    }

    [HarmonyPatch(typeof(InGameUI), nameof(InGameUI.Update))]
    public class ShowTextPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InGameUI __instance)
        {
            if (GameAPP.theBoardLevel == Core.levelID && GameAPP.theBoardType == Utils.CustomLevelType && GameAPP.theGameStatus == GameStatus.InGame &&
                !__instance.GetData<bool>("UltimateWinterLevel_LevelInit"))
            {
                CreateZombie.Instance.SetZombieWithMindControl(2, ZombieType.UltimateSnowZombie, -7.5f).GetComponent<UltimateSnowZombie>().
                    SetLevelBoss().AddComponent<UltimateSnowZombieLevel>();
                InGameText.Instance.ShowText("WASD控制移动，JKL使用技能，注意别让操控的究极雪原女皇死亡", 5f);
                __instance.SetData("UltimateWinterLevel_LevelInit", true);
            }
        }
    }

    [HarmonyPatch(typeof(UltimateSnowZombie))]
    public static class UltimateSnowZombiePatch
    {
        [HarmonyPatch(nameof(UltimateSnowZombie.MoveUpdate))]
        [HarmonyPrefix]
        public static bool PreMoveUpdate(UltimateSnowZombie __instance)
        {
            if (__instance.IsLevelBoss())
            {
                __instance.theSpeed = 0f;
                
                return false;
            }
            return true;
        }
    }

    public static class UltimateWinterLevelExt
    {
        public static bool IsLevelBoss(this UltimateSnowZombie zombie) => zombie.GetData<bool>("UltimateWinterLevel_IsLevel");
        public static UltimateSnowZombie SetLevelBoss(this UltimateSnowZombie zombie)
        {
            zombie.SetData("UltimateWinterLevel_IsLevel", true);
            return zombie;
        }
    }
}
