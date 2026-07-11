using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using CustomizeLib.BepInEx;

namespace UltimateDoomMinigunScaredy.BepInEx
{
    [BepInPlugin("salmon.ultimatedoomminigunscaredy", "UltimateDoomMinigunScaredy", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<UltimateDoomMinigunScaredy>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "ultimatedoomminigunscaredy");
            CustomCore.RegisterCustomPlant<GatlingDoomScaredy, UltimateDoomMinigunScaredy>((int)UltimateDoomMinigunScaredy.PlantID, ab.GetAsset<GameObject>("UltimateDoomMinigunScaredyPrefab"),
                ab.GetAsset<GameObject>("UltimateDoomMinigunScaredyPreview"), [], 0.5f, 0f, 300, 300, 90f, 1000);
            CustomCore.AddPlantAlmanacStrings((int)UltimateDoomMinigunScaredy.PlantID, $"究极速射毁灭机枪胆小菇({(int)UltimateDoomMinigunScaredy.PlantID})",
                "发射毁灭菇的加特林速射机枪胆小菇\n" +
                "<color=#0000FF>毁灭机枪胆小菇的限定形态</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>①融合或转化毁灭机枪胆小菇时有2%概率变异\n" +
                "②神秘模式\n" +
                "*可使用胆小菇切回毁灭机枪胆小菇\n" +
                "*可使用豌豆射手切换究极毁灭速射机枪</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300/0.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每攻击1次减少0.02秒攻击间隔，最低0.1秒\n" +
                "②启动射击需要预热1秒\n" +
                "③每次发射有5%概率发射大毁灭菇，每第16发为大毁灭菇</color>\n\n" +
                "<color=#3D1400>究极毁灭速射机枪胆小菇经营着植物界最大的服装店，“一株植物，根据他的穿着就能看出他的性格或是爱好，我喜欢有个性的植物，他们勇敢又正义。”他曾荣获植物界服装设计绿叶奖，这是所有设计师们梦寐以求的奖项，每当有人问起，他总会说“不知道啊，我去参赛他们给我的～”</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add(UltimateDoomMinigunScaredy.PlantID, CardLevel.Red);
            CustomCore.AddFusion((int)PlantType.GatlingDoomScaredy, (int)UltimateDoomMinigunScaredy.PlantID, (int)PlantType.ScaredyShroom);
            CustomCore.AddFusion((int)PlantType.GatlingDoomScaredy, (int)PlantType.ScaredyShroom, (int)UltimateDoomMinigunScaredy.PlantID);
            CustomCore.AddFusion((int)UltimateDoomMinigunScaredy.PlantID, 1933, (int)PlantType.ScaredyShroom);
            CustomCore.AddFusion((int)UltimateDoomMinigunScaredy.PlantID, (int)PlantType.ScaredyShroom, 1933);
        }
    }

    public class UltimateDoomMinigunScaredy : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1953;

        public GatlingDoomScaredy plant => gameObject.GetComponent<GatlingDoomScaredy>();

        public void Awake()
        {
            plant.shoot = transform.FindChild("Shoot");
        }

        public void ScaredyShoot()
        {
            if (plant.thePlantAttackInterval > 0.1f)
            {
                plant.thePlantAttackInterval -= 0.02f;
                plant.anim.speed += 0.375f;
            }

            plant.thePlantAttackInterval = Mathf.Min(0.5f, plant.thePlantAttackInterval);
            plant.anim.speed = 1 + (0.5f - (Mathf.Min(plant.thePlantAttackInterval, 0.5f))) / 0.02f * 0.375f;

            plant.doomTimes++;

            if ((plant.doomTimes % ((Lawnf.TravelAdvanced((AdvBuff)3)) ? 4 : 16) == 0) || (UnityEngine.Random.Range(1, 101) <= 5))
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, BulletType.Bullet_doom_big,
                    BulletMoveWay.MoveRight);
                bullet.Damage = plant.attackDamage * 6;
                bullet.theStatus = BulletStatus.Doom_big;
                bullet.normalSpeed *= 2;
                plant.doomTimes = 0;
            }
            else
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, BulletType.Bullet_doom,
                    BulletMoveWay.MoveRight);
                bullet.Damage = plant.attackDamage;
                bullet.normalSpeed *= 2;
            }

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
        }
    }

    [HarmonyPatch(typeof(ScaredyDoom))]
    public static class ScaredyDoomPatch
    {
        [HarmonyPatch(nameof(ScaredyDoom.ScaredEvent))]
        [HarmonyPrefix]
        public static void PreScaredEvent(ScaredyDoom __instance)
        {
            if (__instance.thePlantType == UltimateDoomMinigunScaredy.PlantID)
            {
                if (__instance.thePlantAttackInterval <= 0.1f) __instance.thePlantAttackInterval = 0.2f;
            }
        }
    }

    [HarmonyPatch(typeof(ScaredyShroom), nameof(ScaredyShroom.Shootable))]
    public static class ScaredyShroomPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ScaredyShroom __instance, ref bool __result)
        {
            if (__instance.thePlantType == UltimateDoomMinigunScaredy.PlantID)
            {
                __instance.anim.SetBool("shooting", __result);
                if (!__result)
                {
                    __instance.anim.speed = 1f;
                }
                if (Lawnf.TravelAdvanced((AdvBuff)2) && __result)
                    __instance.anim.Play("shooting");
                var clipInfo = __instance.anim.GetCurrentAnimatorClipInfo(1);
                if (clipInfo.Length > 0 && clipInfo[0].clip.name == "shooting" && __instance.thePlantAttackInterval <= 0.1f)
                    __instance.anim.speed = 2f;
            }
        }
    }

    [HarmonyPatch(typeof(CreatePlant), nameof(CreatePlant.CheckMix))]
    public static class CreatePlant_CheckMix_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(CreatePlant __instance, ref Plant __result)
        {
            if (__result != null && __result.GetComponent<Plant>().thePlantType == PlantType.GatlingDoomScaredy && UnityEngine.Random.Range(0, 100) <= 1)
            {
                var plant = __result.GetComponent<Plant>();
                __instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, UltimateDoomMinigunScaredy.PlantID, null, default, true);
                plant.Die();
            }
        }
    }
}