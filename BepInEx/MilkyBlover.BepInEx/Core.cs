using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using System.Collections;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;

namespace MilkyBlover.BepInEx
{
    [HarmonyPatch(typeof(Blover), "AnimBlow")]
    public static class BloverPatch
    {
        public static void Postfix(Blover __instance)
        {
            if (__instance.gameObject.TryGetComponent<MilkyBlover>(out var p) && Lawnf.TravelAdvanced(45))
            {
                __instance.StartCoroutine(p.CreateStar());
            }
        }
    }

    [HarmonyPatch(typeof(InitBoard), "RightMoveCamera")]
    public static class InitBoardPatch
    {
        public static void Postfix()
        {
            try
            {
                var template = GameObject.Find("Blover");
                var card = UnityEngine.Object.Instantiate(template, template.transform.parent.parent.parent.GetChild(1).GetChild(0));
                card.name = "MilkyBlover";
                var mkbBg = card.transform.GetChild(0).gameObject;
                Lawnf.ChangeCardSprite((PlantType)169, mkbBg);
                mkbBg.GetComponent<Image>().sprite = GameAPP.spritePrefab[208];
                mkbBg.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantData[169].field_Public_Int32_1.ToString();
                if (Board.Instance is not null && (Board.Instance.boardTag.enableTravelPlant || Board.Instance.boardTag.enableAllTravelPlant || GameAPP.developerMode))
                {
                    var mkb1 = card.transform.GetChild(2).gameObject;
                    Lawnf.ChangeCardSprite((PlantType)169, mkb1);
                    mkb1.GetComponent<Image>().sprite = GameAPP.spritePrefab[208];
                    mkb1.GetComponent<CardUI>().CD = PlantDataLoader.plantData[169].field_Public_Single_2;
                    mkb1.GetComponent<CardUI>().theSeedCost = PlantDataLoader.plantData[169].field_Public_Int32_1;
                    mkb1.GetComponent<CardUI>().thePlantType = (PlantType)169;
                    mkb1.GetComponent<CardUI>().theSeedType = 169;
                    InGameUI.Instance.cards.Add(mkb1.GetComponent<CardUI>());

                    var mkb2 = card.transform.GetChild(1).gameObject;
                    Lawnf.ChangeCardSprite((PlantType)169, mkb2);
                    mkb2.GetComponent<Image>().sprite = GameAPP.spritePrefab[208];
                    mkb2.GetComponent<CardUI>().CD = PlantDataLoader.plantData[169].field_Public_Single_2;
                    mkb2.GetComponent<CardUI>().theSeedCost = PlantDataLoader.plantData[169].field_Public_Int32_1;
                    mkb2.GetComponent<CardUI>().thePlantType = (PlantType)169;
                    mkb2.GetComponent<CardUI>().theSeedType = 169;
                    InGameUI.Instance.cards.Add(mkb2.GetComponent<CardUI>());
                }
                else
                {
                    UnityEngine.Object.Destroy(card.transform.GetChild(1).gameObject);
                    UnityEngine.Object.Destroy(card.transform.GetChild(2).gameObject);
                }
            }
            catch { }
        }
    }

    [BepInPlugin("inf75.milkyblover", "MilkyBlover", "1.0")]
    public class Core : BasePlugin//169
    {
        public override void Load()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<MilkyBlover>();
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "milkyblover");
            CustomCore.RegisterCustomPlant<Blover, MilkyBlover>(169, ab.GetAsset<GameObject>("MilkyBloverPrefab"),
                ab.GetAsset<GameObject>("MilkyBloverPreview"), [], 3, 0, 80, 300, 60f, 500);
            CustomCore.RegisterCustomSprite(208, ab.GetAsset<Sprite>("SeedPacket_MilkyBlover"));
            CustomCore.AddPlantAlmanacStrings(169, "银河三叶草(169)", "似乎只是个比较可爱的三叶草...吗???\n<color=#3D1400>贴图作者：@Just Eris</color>\n<color=#3D1400>特点：</color><color=red>二创彩蛋植物，不参与融合，一般情况下同三叶草，当词条星神合一解锁时每0.3s召唤一个究极陨星/阳光陨星，3s后消失</color>\n<color=#3D1400>花费：</color><color=red>500</color>\n<color=#3D1400>冷却时间：</color><color=red>60s</color>\n<color=#3D1400>快看，是专为「沉睡戴夫」工作的特别列车长\n驾驶穿梭于银河的梦之推车的特别三叶草\n噗噗!列车出发~今天的银河三叶草也格外闪耀呢\n虽然大多数时候有些懒散 不过在面对僵尸的捣乱时，她会二话不说出面处理。</color>");
        }
    }

    public class MilkyBlover : MonoBehaviour
    {
        public MilkyBlover() : base(ClassInjector.DerivedConstructorPointer<MilkyBlover>()) => ClassInjector.DerivedConstructorBody(this);

        public MilkyBlover(IntPtr i) : base(i)
        {
        }

        [HideFromIl2Cpp]
        public IEnumerator CreateStar()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (plant is not null && !plant.IsDestroyed() && Board.Instance is not null && !Board.Instance.IsDestroyed() && Lawnf.TravelAdvanced(45))
                    {
                        Board.Instance.CreateUltimateMateorite();
                    }
                    else
                    {
                        break;
                    }
                }
                catch { break; }
                yield return new WaitForSeconds(0.3f);
            }
        }

        public Blover? plant => gameObject.TryGetComponent<Blover>(out var p) ? p : null;
    }
}