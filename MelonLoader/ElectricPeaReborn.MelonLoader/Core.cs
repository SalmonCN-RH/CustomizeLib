using CustomizeLib.MelonLoader;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;

[assembly: MelonInfo(typeof(ElectricPeaReborn.MelonLoader.Core), "ElectricPeaReborn", "1.0", "Infinite75", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace ElectricPeaReborn.MelonLoader
{
    [RegisterTypeInIl2Cpp]
    public class Bullet_electricPea : MonoBehaviour
    {
        public Bullet_electricPea() : base(ClassInjector.DerivedConstructorPointer<Bullet_electricPea>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_electricPea(IntPtr i) : base(i)
        {
        }

        public void Start()
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }

        public void Update()
        {
            if (GameAPP.theGameStatus is (int)GameStatus.InGame)
            {
                bullet.normalSpeed = 3;
                var pos = bullet.transform.position;
                LayerMask layermask = bullet.zombieLayer.m_Mask;
                var array = Physics2D.OverlapCircleAll(new(pos.x, pos.y), 1.5f);
                foreach (var z in array)
                {
                    if (z is not null && !z.IsDestroyed() && z.TryGetComponent<Zombie>(out var zombie) && zombie is not null && !zombie.isMindControlled && !zombie.IsDestroyed())
                    {
                        zombie.TakeDamage(DmgType.Normal, bullet.Damage);
                        GameAPP.PlaySound(UnityEngine.Random.RandomRange(0, 3));
                        CreateParticle.SetParticle(53, new(zombie.axis.position.x, zombie.axis.position.y + 0.5f, zombie.axis.position.z), zombie.theZombieRow);
                        if (Lawnf.TravelAdvanced(ElectricPea.Buff))
                        {
                            zombie.BodyTakeDamage((int)(0.05 * (zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth)));
                        }
                    }
                }
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        public Bullet bullet => gameObject.GetComponent<Bullet>();
    }

    public class Core : MelonMod//960
    {
        public override void OnInitializeMelon()
        {
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "electricpea");
            CustomCore.RegisterCustomBullet<Bullet, Bullet_electricPea>((BulletType)903, ab.GetAsset<GameObject>("ProjectileElectricPea"));
            CustomCore.RegisterCustomPlant<Shooter, ElectricPea>(960, ab.GetAsset<GameObject>("ElectricPeaPrefab"),
                ab.GetAsset<GameObject>("ElectricPeaPreview"), [(1005, 1103), (1103, 1005)], 3, 0, 20, 300, 7.5f, 300);
            CustomCore.AddPlantAlmanacStrings(960, "电能豌豆（重生）(960)", "电能豌豆发射具有穿透和帧伤能力的强力电能子弹。\n<color=#3D1400>伤害：</color><color=red>20/3x3帧伤</color>\n<color=#3D1400>融合配方：</color><color=red>超级樱桃射手+磁力仙人掌</color>\n<color=#3D1400>词条：</color><color=red>电涌穿透：电能豌豆的子弹每次攻击对本体额外造成总血量5%的伤害</color>\n<color=#3D1400>本是版本的弃子，本是时代的眼泪。电能豌豆本该在蓝飘飘的回收站里永远沉睡。没有人知道，某个夜晚，有个叫什么玩意75的人把它翻了出来拿去当做实验品。没有人知道，这个不该、不应、也不配被人们知道的废稿，成为了引领身后那些新面孔们进入这个游戏里的先驱。没有人知道，它才是融合世界第一个有自己id的二创植物：代号，960。</color>");
            ElectricPea.Buff = CustomCore.RegisterCustomBuff("电涌穿透：电能豌豆的子弹每次攻击对本体额外造成总血量5%的伤害", BuffType.AdvancedBuff, () => Board.Instance.ObjectExist<ElectricPea>(), 36100, "red", (PlantType)960);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class ElectricPea : MonoBehaviour
    {
        public ElectricPea() : base(ClassInjector.DerivedConstructorPointer<ElectricPea>()) => ClassInjector.DerivedConstructorBody(this);

        public ElectricPea(IntPtr i) : base(i)
        {
        }

        public Bullet AnimShooting()
        {
            Bullet bullet = Board.Instance.GetComponent<CreateBullet>().SetBullet((float)(plant.shoot.position.x + 0.1f), plant.shoot.position.y, plant.thePlantRow, (BulletType)903, 0);
            bullet.Damage = plant.attackDamage;
            return bullet;
        }

        public void Start()
        {
            plant.shoot = plant.gameObject.transform.FindChild("Shoot");
        }

        public static int Buff { get; set; } = -1;
        public Shooter plant => gameObject.GetComponent<Shooter>();
    }
}