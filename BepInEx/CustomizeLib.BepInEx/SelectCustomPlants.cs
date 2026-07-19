using Il2CppInterop.Runtime;
using NewTravel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CustomizeLib.BepInEx
{
    public class SelectCustomPlants : MonoBehaviour
    {
        public static SelectCustomPlants Instance = null!;
        public static GameObject CustomButton = null!;
        public static GameObject CustomPage = null!;

        public static int PageCardMax => 6 * 9;
        public static Board board => Board.Instance;
        public static List<PlantType> GetPlants() => [.. GameAPP.resourcesManager.allPlants.ToArray().Where(t => !Enum.IsDefined(t))];

        public bool init = false;

        public static void InitButton()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                if (board == null) return;
                GameObject customButton = null!;
                if (!board.boardTag.isIZ)
                {
                    customButton = Instantiate(InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/ShowCardLayout/ColorCards"),
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/ShowCardLayout")).gameObject;
                }
                else
                {
                    customButton = Instantiate(IZBottomMenu.Instance.plantLibrary.transform.FindChild("Buttons/NextPage"),
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Buttons")).gameObject;
                }
                customButton.name = "SelectCustom";
                customButton.SetActive(true);
                customButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "二创植物";
                Destroy(customButton.GetComponent<UIButton>());
                Instance = customButton.AddComponent<SelectCustomPlants>();
                CustomButton = customButton;
            }
            catch (Exception) { }
        }

        public void OpenPlantsCard()
        {
            try
            {
                if (board == null) return;
                if (init && CustomPage != null)
                {
                    if (!board.boardTag.isIZ)
                        for (int i = 0; i < InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").childCount; i++)
                            InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.SetActive(false);
                    else
                        for (int i = 0; i < IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").childCount; i++)
                            IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").GetChild(i).gameObject.SetActive(false);
                    CustomPage.SetActive(true);
                    return;
                }
                if (!board.boardTag.isIZ)
                {
                    for (int i = 0; i < InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").childCount; i++)
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.SetActive(false);
                    var page = Instantiate(InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards"),
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer")).gameObject;
                    CustomPage = page;
                    page.name = "CustomCards";
                    page.SetActive(true);
                    var list = GetPlants();
                    int count = list.Count;
                    int pageNum = count / PageCardMax + (count % PageCardMax > 0 ? 1 : 0); // 计算需要的页数
                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject); // 销毁除第一页以外的所有页
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject); // 销毁除第一张卡以外的所有卡
                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;
                    int remain = count;

                    page.transform.GetChild(0).gameObject.name = "SampleGrid_1";
                    for (int i = 1; i < pageNum; i++) // 已经有第一个了，可以少实例化一个
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform); // 实例化页面
                        tmp.name = $"SampleGrid_{i + 1}";
                    }

                    int listIndex = 0; // 循环变量，位于list的哪一个索引
                    for (int i = 0; i < pageNum; i++) // 实例化卡，要从第一个开始实例化
                    {
                        var parent = page.transform.GetChild(i);
                        for (int j = 0; j < PageCardMax; j++)
                        {
                            var pt = list[listIndex];
                            var cardObj = Instantiate(startCard, parent);
                            var card = cardObj.transform.GetChild(1).GetComponent<CardUI>();
                            var packet = cardObj.transform.GetChild(0);
                            packet.localPosition = card.transform.localPosition;
                            packet.localRotation = card.transform.localRotation;
                            packet.localScale = card.transform.localScale;
                            cardObj.SetActive(true);

                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, card);

                            //背景图片
                            Image image = card.transform.GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                            packet.GetChild(0).GetComponent<Image>().sprite = image.sprite;
                            // image.SetNativeSize();
                            packet.GetChild(0).GetComponent<RectTransform>().sizeDelta = card.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
                            // packet.GetChild(0).GetComponent<Image>().SetNativeSize();

                            //设置价格
                            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();
                            packet.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();

                            cardObj.gameObject.SetActive(true);

                            card.GetComponent<BoxCollider2D>().enabled = true;

                            //设置数据
                            card.thePlantType = pt;
                            card.theSeedType = (int)pt;
                            card.theSeedCost = PlantDataManager.PlantData_Default[pt].cost;
                            card.fullCD = PlantDataManager.PlantData_Default[pt].cd;
                            cardObj.name = pt.ToString();

                            listIndex++;
                            // 如果没了就结束循环
                            remain--;
                            if (remain == 0) break;
                        }
                    }
                    Destroy(startCard);
                }
                else
                {
                    for (int i = 0; i < IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").childCount; i++)
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").GetChild(i).gameObject.SetActive(false);
                    var page = Instantiate(IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/全部植物"),
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid")).gameObject;
                    CustomPage = page;
                    page.name = "二创植物";
                    page.SetActive(true);
                    var list = GetPlants();
                    int count = list.Count;
                    int pageNum = count / PageCardMax + (count % PageCardMax > 0 ? 1 : 0); // 计算需要的页数
                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject); // 销毁除第一页以外的所有页
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject); // 销毁除第一张卡以外的所有卡
                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;
                    int remain = count;

                    page.transform.GetChild(0).gameObject.name = "PlantCardPage_1";
                    for (int i = 1; i < pageNum; i++) // 已经有第一个了，可以少实例化一个
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform); // 实例化页面
                        tmp.name = $"PlantCardPage_{i + 1}";
                    }

                    int listIndex = 0; // 循环变量，位于list的哪一个索引
                    for (int i = 0; i < pageNum; i++) // 实例化卡，要从第一个开始实例化
                    {
                        var parent = page.transform.GetChild(i);
                        for (int j = 0; j < PageCardMax; j++)
                        {
                            var pt = list[listIndex];
                            var cardObj = Instantiate(startCard, parent);
                            var card = cardObj.transform.GetChild(0).GetComponent<CardUI>();
                            cardObj.SetActive(true);

                            //背景图片
                            Image image = card.transform.GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();

                            //设置价格
                            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                PlantDataManager.PlantData_Default[pt].cost.ToString();

                            card.gameObject.SetActive(true);

                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, card);

                            card.GetComponent<BoxCollider2D>().enabled = true;

                            //设置数据
                            card.thePlantType = pt;
                            card.theSeedType = (int)pt;
                            card.theSeedCost = 0;
                            card.fullCD = 0f;
                            cardObj.name = pt.ToString();

                            listIndex++;
                            // 如果没了就结束循环
                            remain--;
                            if (remain == 0) break;
                        }
                    }
                    Destroy(startCard);
                }
                init = true;
            }
            catch (Il2CppException) { }
        }

        public void Update()
        {
            //判断鼠标按下
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (Input.GetMouseButtonDown(0) && CustomButton != null)
            {
                //击中二创植物Button
                if (hit.collider != null && hit.collider.gameObject == CustomButton)
                    OpenPlantsCard();
            }

            //设置鼠标特效
            if (CustomButton != null && hit.collider != null && hit.collider.gameObject == CustomButton)
                CursorChange.SetClickCursor();
        }
    }
}
