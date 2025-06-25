using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Video;
using static MelonLoader.MelonLogger;

namespace ZombieBoss2Remake.MelonLoader
{
    [HarmonyPatch(typeof(PrizeMgr))]
    public static class PrizeMgrPatch
    {
        [HarmonyPatch(nameof(PrizeMgr.Click))]
        [HarmonyPostfix]
        public static void PostStart(PrizeMgr __instance)
        {
            if (__instance.fake && __instance.isClicked)
            {
                CustomCore.PlaySound(Core.FakeTrophySound!);
            }
        }

        [HarmonyPatch("RestartBoss2")]
        [HarmonyPrefix]
        public static bool RestartBoss2(PrizeMgr __instance)
        {
            UnityEngine.Object.Destroy(__instance.gameObject);
            UnityEngine.Object.Instantiate(Core.FakeTrophyAnim);
            Board.Instance.ClearTheBoard();
            GameAPP.UIManager.PopAll();
            UnityEngine.Object.Destroy(GameAPP.board);
            GameAPP.board = null;

            /*
            Board.Instance.ClearTheBoard();
            GameAPP.UIManager.PopAll();
            UnityEngine.Object.Destroy(GameAPP.board);
            GameAPP.board = null;
            UIMgr.EnterGame((LevelType)66, Core.LevelID);
            */
            /*
            // 重置Board状态
            Board.Instance.boss2 = true;

            // 查找并销毁Boss相关对象
            Transform bossContainer = Board.Instance.transform.Find("ZombieBoss");
            if (bossContainer is not null)
            {
                UnityEngine.Object.Destroy(bossContainer.gameObject);
            }

            // 创建Boss僵尸
            CreateZombie.Instance.SetZombie(0, ZombieType.ZombieBoss2);

            // 播放Boss音乐
            GameAPP.Instance.PlayMusic(MusicType.Boss2);

            // 清除所有现有植物
            var allPlants = Lawnf.GetAllPlants();
            foreach (Plant plant in allPlants)
            {
                plant.Die();
            }

            // 重置传送带状态
            ConveyBeltMgr.Instance.StopAllCoroutines();
            for (int i = ConveyBeltMgr.Instance.droppedCards.Count - 1; i >= 0; i--)
            {
                ConveyBeltMgr.Instance.droppedCards[i]?.Die();
            }
            ConveyBeltMgr.Instance.droppedCards.Clear();
            InGameUI.Instance.ConveyorBelt.SetActive(false);
            InGameUI.Instance.SeedBank.SetActive(true);
            InGameUI.Instance.ShowCardBank.SetActive(true);
            InGameUI.Instance.MoneyBank.SetActive(true);
            InGameUI.Instance.SeedBank.transform.position = new(InGameUI.Instance.SeedBank.transform.position.x, 4.3f, InGameUI.Instance.SeedBank.transform.position.z);
            Board.Instance.theSun = 50000;
            var tag = Board.Instance.boardTag;
            tag.enableTravelBuff = true;
            tag.enableAllTravelPlant = true;
            Board.Instance.boardTag = tag;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    CreatePlant.Instance.SetPlant(row, col, PlantType.Pot);
                }
            }

            Glove.Instance?.gameObject.SetActive(false);
            HammerMgr.Instance?.gameObject.SetActive(false);

            Transform uiRoot = Board.Instance.transform.GetChild(0).GetChild(0);
            uiRoot.GetChild(0).gameObject.SetActive(false);
            uiRoot.GetChild(1).gameObject.SetActive(true);*/
            return false;
        }
    }

    [RegisterTypeInIl2Cpp]
    public class FakeTrophyAnim : MonoBehaviour
    {
        public FakeTrophyAnim() : base(ClassInjector.DerivedConstructorPointer<FakeTrophyAnim>()) => ClassInjector.DerivedConstructorBody(this);

        public FakeTrophyAnim(IntPtr i) : base(i)
        {
        }

        public void Start()
        {
            transform.GetChild(0).GetChild(0).localScale *= Math.Min(Screen.width / 1920f, Screen.height / 1080f);// * (2f / 3f);
            transform.GetChild(0).GetChild(0).GetComponent<VideoPlayer>().loopPointReached = new Action<VideoPlayer>((p) =>
            {
                UIMgr.EnterGame((LevelType)66, Core.LevelID);
                Destroy(p.transform.parent.parent.gameObject);
            });
        }
    }
}