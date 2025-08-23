using MelonLoader;
using UnityEngine;
using Il2Cpp;
using CustomizeLib.MelonLoader;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Runtime.Remoting.Messaging;
using HarmonyLib;
using Unity.VisualScripting;

[assembly: MelonInfo(typeof(MScardyShroom.Core), "MScardyShroom", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace MScardyShroom
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "mscaredyshroom");
            CustomCore.RegisterCustomPlant<ScaredyShroom, MScardyShroom>(MScardyShroom.PlantID, ab.GetAsset<GameObject>("MScaredyShroomPrefab"),
                ab.GetAsset<GameObject>("MScaredyShroomPreview"), [], 1f, 0f, 300, 300, 15f, 250);
            CustomCore.AddPlantAlmanacStrings(MScardyShroom.PlantID, $"猫娘胆小菇({MScardyShroom.PlantID})",
                "根据场上僵尸的数量，提高子弹伤害。\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>300/1秒</color>\n<color=#3D1400>特点：</color><color=red>当场上每生成1只僵尸时，永久增加10点伤害，猫娘胆小菇之间不共享伤害。</color>\n\n<color=#3D1400>“总所周知，红眼巨人十分脆弱，三两下就干倒了，真的太逊了。”猫娘</color>\n花费：<color=red>250</color>\n冷却时间：<color=red>15秒</color>\n\n<color=#3D1400>胆小菇说。至于为什么不直接上场战斗，“哎，你得先喂我亿点金坷垃在上场吧。”</color>\n\n\n\n\n\n\n\n\n花费：<color=red>250</color>\n冷却时间：<color=red>15秒</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)MScardyShroom.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class MScardyShroom : MonoBehaviour
    {
        public static int PlantID = 1917;
        public static List<MScardyShroom> ms = new List<MScardyShroom>();
        public bool add = false;
        public int damageAdder = 0;

        public void Awake()
        {
            plant.shoot = gameObject.transform.FindChild("Shoot");
            ms.Add(this);
        }

        public void Update()
        {
            if ((plant == null)) return;
            if (add)
            {
                damageAdder++;
                add = false;
            }
            int attackDamage = 300 + damageAdder * 10;
            plant.attackDamage = attackDamage;
        }

        public Bullet AnimShoot_MScaredyShroom()
        {
            if (CreateBullet.Instance == null) return null;
            if (plant.shoot == null)           return null;

            Bullet bullet = CreateBullet.Instance.SetBullet(
                plant.shoot.position.x + 0.1f,
                plant.shoot.position.y,
                plant.thePlantRow,
                BulletType.Bullet_puff,
                BulletMoveWay.MoveRight,
                false);

            if (bullet == null)               return null;

            bullet.Damage = plant.attackDamage;

            GameAPP.PlaySound(57, 0.5f, 1.0f);

            return bullet;
        }

        public void OnDestroy()
        {
            ms.Remove(this);
        }

        public MScardyShroom() : base(ClassInjector.DerivedConstructorPointer<MScardyShroom>()) => ClassInjector.DerivedConstructorBody(this);

        public MScardyShroom(IntPtr i) : base(i)
        {
        }

        public ScaredyShroom plant => gameObject.GetComponent<ScaredyShroom>();
    }

    [HarmonyPatch(typeof(CreateZombie), nameof(CreateZombie.SetZombie))]
    public class CreateZombie_SetZombie_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref GameObject __result)
        {
            if (__result == null) return;
            foreach (var ms in MScardyShroom.ms)
                ms.add = true;
        }
    }

    [HarmonyPatch(typeof(CreateZombie), nameof(CreateZombie.SetZombieWithMindControl))]
    public class CreateZombie_SetZombieWithMindCtrl_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref GameObject __result)
        {
            if (__result == null) return;
            foreach (var ms in MScardyShroom.ms)
                ms.add = true;
        }
    }
}