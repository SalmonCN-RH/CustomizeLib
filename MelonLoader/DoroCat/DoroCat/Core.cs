using CustomizeLib.MelonLoader;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;
using HarmonyLib;

[assembly: MelonInfo(typeof(DoroCat.Core), "DoroCat", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace DoroCat
{
    public class Core : MelonMod
    {
        public static bool[] whiteList = new bool[2048];
        public static bool isInit = false;
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "dorocat");
            CustomCore.RegisterCustomBullet<Bullet_cactus, Bullet_doroCat>((BulletType)Bullet_doroCat.BulletID, ab.GetAsset<GameObject>("DoroCatBullet"));
            CustomCore.RegisterCustomPlant<CattailPlant, DoroCat>(DoroCat.PlantID, ab.GetAsset<GameObject>("DoroCatPrefab"),
                ab.GetAsset<GameObject>("DoroCatPreview"), [], 1.5f, 0, 150, 300, 30f, 325);
            CustomCore.AddPlantAlmanacStrings(DoroCat.PlantID, $"Doro({DoroCat.PlantID})", "发射子弹驱逐一般僵尸，对于无法驱逐的僵尸造成8倍伤害\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>伤害：</color><color=red>150/1200*2/1.5秒</color>\n\n<color=#3D1400>哦润吉汁水丰富，气味芬香，便宜，也十分好保存。Doro曾想把哦润吉分享给僵尸们，最终都以僵尸快速远离而收尾。或许那群食脑者都不明白哦润吉的美味？</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)DoroCat.PlantID);
            CreateWhiteListZombieFile();
        }

        public static void CreateWhiteListZombieFile()
        {
            String directory = Path.Combine(Directory.GetCurrentDirectory(), "SalmonPlantsConfig");
            String path = Path.Combine(Directory.GetCurrentDirectory(), "SalmonPlantsConfig", "DoroCatWhiteList.txt");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
             
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine("0,2,3,4,5,7,8,9,10,11,12,13,15,17,19,21,22,23,24,25,27,28,29,30,35,36,37,40,43,45,47,48,53,55,58,59,103,105,106,107,109,110,111,204,207,208,210,213,215,227");
                }
            }
        }

        public static bool ReadWhiteList(Zombie zombie)
        {
            int zombieType = (int)zombie.theZombieType;
            bool isInWhiteList = false;
            String path = Path.Combine(Directory.GetCurrentDirectory(), "SalmonPlantsConfig", "DoroCatWhiteList.txt");
            if (!isInit)
            {
                if (File.Exists(path))
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        String line = sr.ReadToEnd();
                        if (line == "")
                        {
                            for (int i = 0; i < whiteList.Length; i++)
                            { whiteList[i] = true; }
                            isInit = true;
                            return true;
                        }
                        String[] whiteListStr = line.Split(',');
                        for (int i = 0; i < whiteListStr.Length; i++)
                        {
                            int id = int.Parse(whiteListStr[i]);
                            whiteList[id] = true;
                        }
                        if (whiteList[zombieType] == true)
                            isInWhiteList = true;
                        isInit = true;
                    }
                }
            }
            else
            {
                if (whiteList[zombieType] == true)
                    isInWhiteList = true;
            }
            return isInWhiteList;
        }
    }

    [RegisterTypeInIl2Cpp]
    public class DoroCat : MonoBehaviour
    {
        public DoroCat() : base(ClassInjector.DerivedConstructorPointer<DoroCat>()) => ClassInjector.DerivedConstructorBody(this);

        public DoroCat(IntPtr i) : base(i) { }

        public static int PlantID = 1903;

        public Bullet AnimShoot_DoroCat()
        {
            Bullet bullet = Board.Instance.GetComponent<CreateBullet>().SetBullet((float)(plant.shoot.position.x), (float)plant.shoot.position.y, plant.thePlantRow, (BulletType)Bullet_doroCat.BulletID, (int)BulletMoveWay.Track);
            bullet.Damage = plant.attackDamage;
            int soundId = UnityEngine.Random.Range(3, 5);
            GameAPP.PlaySound(
                soundId,
                0.5f,
                pitch: 1.0f
            );
            return bullet;
        }

        public void Start()
        {
            plant.shoot = plant.gameObject.transform.FindChild("Shoot");
        }

        public CattailPlant plant => gameObject.GetComponent<CattailPlant>();
    }

    [RegisterTypeInIl2Cpp]
    public class Bullet_doroCat : MonoBehaviour
    {
        public Bullet_doroCat() : base(ClassInjector.DerivedConstructorPointer<Bullet_doroCat>()) => ClassInjector.DerivedConstructorBody(this);

        public Bullet_doroCat(IntPtr i) : base(i)
        {
        }

        public static int BulletID = 1900;


        public Bullet_cactus bullet => gameObject.GetComponent<Bullet_cactus>();
    }

    [HarmonyPatch(typeof(Bullet_cactus), "HitZombie")]
    public class Bullet_HitZombie
    {
        public static bool Prefix(Bullet_cactus __instance, ref Zombie zombie)
        {
            if (__instance.theBulletType == (BulletType)Bullet_doroCat.BulletID)
            {
                int randomValue = UnityEngine.Random.Range(0, 3);
                int zombieType = (int)zombie.theZombieType;
                /*MelonLogger.Msg(randomValue);
                MelonLogger.Msg(zombieType);*/
                bool isEnableZombie = Core.ReadWhiteList(zombie);
                // && 
                if (randomValue == 0 && isEnableZombie)
                {
                    zombie.transform.rotation = Quaternion.Euler(0, 180, 0);
                    // MelonLogger.Msg(zombie.transform.rotation.y);
                }
                // MelonLogger.Msg(__instance.Damage);
                if (!isEnableZombie)
                    zombie.TakeDamage(DmgType.Normal, __instance.Damage * 8);
                else
                    zombie.TakeDamage(DmgType.Normal, __instance.Damage);
                __instance.PlaySound(zombie);
                __instance.Die();
                return false;
            }
            return true;
        }
    }
}