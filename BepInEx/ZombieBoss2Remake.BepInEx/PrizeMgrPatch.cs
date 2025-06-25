using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.Video;

namespace ZombieBoss2Remake.BepInEx
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
            return false;
        }
    }

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