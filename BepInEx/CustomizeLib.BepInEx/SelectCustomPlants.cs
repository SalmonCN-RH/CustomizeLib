using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomizeLib.BepInEx
{
    public class SelectCustomPlants : MonoBehaviour
    {
        /// <summary>
        /// 隐藏二创植物界面
        /// </summary>
        public static void CloseCustomPlantCards()
        {
            if (MyPageParent != null)
            {
                MyPageParent.SetActive(false);
                // Destroy(MyPageParent);
                // MyPageParent = null;
            }
        }

        /// <summary>
        /// 初始化二创植物Button
        /// </summary>
        public static void InitCustomCards()
        {
            //控制台支持中文
            Console.OutputEncoding = Encoding.UTF8;
            //用正常植物Button创建二创植物Button
            if (Board.Instance is not null && !Board.Instance.isIZ)
            {
                MyShowCustomPlantsButton = Instantiate(
                    Resources.Load<GameObject>("ui/prefabs/InGameUI").transform.FindChild("Bottom/SeedLibrary/ShowNormal")
                        .gameObject, InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary"));
                //设置位置
                MyShowCustomPlantsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(580, -230);
                MyShowCustomPlantsButton.GetComponent<RectTransform>().position = new Vector3(
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.x,
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.y,
                    InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/ShowNormal").position.z);
                //激活
                MyShowCustomPlantsButton.SetActive(true);

                //摧毁UIButton组件
                if (MyShowCustomPlantsButton.GetComponent<UIButton>() != null)
                {
                    Destroy(MyShowCustomPlantsButton.GetComponent<UIButton>());
                    MyShowCustomPlantsButton.AddComponent<SelectCustomPlants>();
                }

                //修改文字
                for (int i = 0; i < MyShowCustomPlantsButton.transform.childCount; i++)
                {
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().m_text = "二创植物";
                    //MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().fontSize = 18 - 6;
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 初始化IZ下的二创植物Button
        /// </summary>
        public static void InitCustomCards_IZ()
        {
            //控制台支持中文
            Console.OutputEncoding = Encoding.UTF8;
            //用正常植物Button创建二创植物Button
            if (Board.Instance is not null && Board.Instance.isIZ)
            {
                MyShowCustomPlantsButton = Instantiate(
                    IZBottomMenu.Instance.zombieLibary.transform.FindChild("LastPage").gameObject,
                    IZBottomMenu.Instance.plantLibrary.transform);
                //设置位置
                MyShowCustomPlantsButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300, 240);
                MyShowCustomPlantsButton.GetComponent<RectTransform>().position = new Vector3(
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.x,
                    MyShowCustomPlantsButton.GetComponent<RectTransform>().position.y,
                    IZBottomMenu.Instance.zombieLibary.transform.FindChild("LastPage").position.z);
                //激活
                MyShowCustomPlantsButton.SetActive(true);

                //摧毁UIButton组件
                if (MyShowCustomPlantsButton.GetComponent<UIButton>() != null)
                {
                    Destroy(MyShowCustomPlantsButton.GetComponent<UIButton>());
                    MyShowCustomPlantsButton.AddComponent<SelectCustomPlants>();
                }

                //修改文字
                for (int i = 0; i < MyShowCustomPlantsButton.transform.childCount; i++)
                {
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().m_text = "二创植物";
                    //MyShowCustomPlantsButton.transform.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>().fontSize = 18 - 6;
                    MyShowCustomPlantsButton.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 打开二创植物界面
        /// </summary>
        public void OpenCustomPlantCards()
        {
            //基础植物和彩色植物界面隐藏
            if (Board.Instance != null && !Board.Instance.isIZ)
            {
                InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/Pages").gameObject
                    .SetActive(false);
                InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/ColorfulCards").gameObject
                    .SetActive(false);
            }
            else if (Board.Instance != null && Board.Instance.isIZ)
            {
                IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/Pages").gameObject.SetActive(false);
                IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards").gameObject.SetActive(false);
                IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards_1").gameObject.SetActive(false);
            }
            //如果二创植物界面已经创建就激活，没有就创建
            if (MyPageParent != null)
            {
                MyPageParent.SetActive(true);
            }
            else
            {
                GameObject? MyPage = null;
                GameObject? MyCard = null;
                int index = 0;
                if (Board.Instance != null && !Board.Instance.isIZ)
                {
                    MyPageParent = Instantiate(
                        InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid/ColorfulCards")
                            .gameObject
                            .gameObject, InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid"));
                    MyPageParent.gameObject.SetActive(true);
                    MyPage = MyPageParent.transform.GetChild(0).gameObject;
                    MyPage.gameObject.SetActive(true);
                    MyCard = MyPage.transform.GetChild(0).gameObject;
                    MyCard.gameObject.SetActive(false);
                }
                else if (Board.Instance != null && Board.Instance.isIZ)
                {
                    MyPageParent = Instantiate(
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/ColorfulCards").gameObject,
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid"));
                    MyPageParent.gameObject.SetActive(true);
                    MyPage = MyPageParent.transform.GetChild(0).gameObject;
                    MyPage.gameObject.SetActive(true);
                    MyCard = MyPage.transform.GetChild(0).gameObject;
                    MyCard.gameObject.SetActive(false);
                }
                /*                try
                                {
                                    //使用彩色植物界面创建二创植物界面

                                }
                                catch (NullReferenceException)
                                {
                                    GetCardGUI(ref MyPage, ref MyCard, ref index);
                                }*/
                if (MyPage == null)
                    throw new NullReferenceException("找不到Page");
                if (MyCard == null)
                    throw new NullReferenceException("找不到Card");
                MyCard.gameObject.SetActive(false);
                for (int i = 0; i < MyPage.transform.childCount; i++)
                {
                    if (i != index)
                    {
                        //销毁Page上的所有Card
                        Destroy(MyPage.transform.GetChild(i).gameObject);
                    }
                }

                List<PlantType> plantTypes = [];
                foreach (PlantType plantType in GameAPP.resourcesManager.allPlants)
                {
                    //如果不是融合版植物，就加载
                    if (!Enum.IsDefined(typeof(PlantType), plantType) &&
                        PlantDataLoader.plantDatas[plantType] != null)
                    {
                        plantTypes.Add(plantType);
                    }
                }

                List<PlantType> cardsOnSeedBank = new List<PlantType>();
                if (Board.Instance != null && !Board.Instance.isIZ)
                {
                    GameObject seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
                    for (int i = 0; i < seedGroup.transform.childCount; i++)
                    {
                        GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                        if (seed.transform.childCount > 0)
                            cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                    }
                }
                else if (Board.Instance != null && Board.Instance.isIZ)
                {
                    GameObject seedGroup = InGameUI_IZ.Instance.transform.GetChild(0).GetChild(0).gameObject;
                    for (int i = 0; i < seedGroup.transform.childCount; i++)
                    {
                        GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                        if (seed.transform.childCount > 0)
                            cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                    }
                }

                //创建卡片
                for (int i = PageNum * CardInPage; i < (PageNum + 1) * CardInPage; i++)
                {
                    if (i >= plantTypes.Count) break;
                    GameObject TempCard = Instantiate(MyCard);
                    if (TempCard != null)
                    {
                        //设置父节点
                        TempCard.transform.SetParent(MyPage.transform);
                        //激活
                        TempCard.SetActive(true);
                        //设置位置
                        TempCard.transform.position = MyCard.transform.position;
                        TempCard.transform.localPosition = MyCard.transform.localPosition;
                        TempCard.transform.localScale = MyCard.transform.localScale;
                        TempCard.transform.localRotation = MyCard.transform.localRotation;
                        //背景图片
                        // 设置背景植物图标
                        Image image = TempCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                        image.sprite = GameAPP.resourcesManager.plantPreviews[plantTypes[i]].GetComponent<SpriteRenderer>().sprite;
                        image.SetNativeSize();
                        // 设置背景价格
                        TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[plantTypes[i]].field_Public_Int32_1.ToString();
                        //卡片
                        CardUI component = TempCard.transform.GetChild(1).GetComponent<CardUI>();
                        component.gameObject.SetActive(true);
                        //修改图片
                        Mouse.Instance.ChangeCardSprite(plantTypes[i], component);
                        // 修改缩放
                        TempCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                        RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                        RectTransform packetRect = TempCard.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();
                        bgRect.localScale = packetRect.localScale;
                        bgRect.sizeDelta = packetRect.sizeDelta;
                        //设置数据
                        component.thePlantType = plantTypes[i];
                        component.theSeedType = (int)plantTypes[i];
                        component.theSeedCost = PlantDataLoader.plantDatas[plantTypes[i]].field_Public_Int32_1;
                        component.fullCD = PlantDataLoader.plantDatas[plantTypes[i]].field_Public_Single_2;
                        if (Board.Instance != null && Board.Instance.isIZ)
                        {
                            component.theSeedCost = 0;
                            component.fullCD = 0f;
                        }
                        if (cardsOnSeedBank.Contains(plantTypes[i]))
                            TempCard.transform.GetChild(1).gameObject.SetActive(false);
                        CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                        customComponent.card = TempCard;
                        customComponent.cardType = component.thePlantType;
                    }
                }

                PageNum++;
                if (PageNum > (plantTypes.Count - 1) / CardInPage)
                {
                    PageNum = 0;
                }
            }
        }

        /// <summary>
        /// 类似Update
        /// </summary>
        public void Update()
        {
            //判断鼠标按下
            if (Input.GetMouseButtonDown(0) && MyShowCustomPlantsButton != null)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                //击中二创植物Button
                if (hit.collider != null && hit.collider.gameObject == MyShowCustomPlantsButton)
                {
                    try
                    {
                        CloseCustomPlantCards();
                    }
                    catch (Exception e)
                    {
                        var a = e;
                    }

                    //打开二创植物页面
                    OpenCustomPlantCards();
                }
            }

            //设置鼠标特效
            if (MyShowCustomPlantsButton != null)
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == MyShowCustomPlantsButton)
                {
                    CursorChange.SetClickCursor();
                }
            }
        }

        /*public static void GetCardGUI(ref GameObject MyPage, ref GameObject MyCard, ref int Index)
        {
            if (Board.Instance != null && !Board.Instance.isIZ)
            {
                GameObject grid = InGameUI.Instance.SeedBank.transform.parent.FindChild("Bottom/SeedLibrary/Grid").gameObject;
                String wantName = "ColorfulCards"; // 原来的彩卡GameObject名
                if (grid == null)
                    throw new NullReferenceException("找不到Grid");
                for (int j = 0; j < grid.transform.childCount; j++)
                {
                    //遍历grid找界面
                    if (grid.transform.GetChild(j) != null)
                    {
                        if (grid.transform.GetChild(j).name != wantName && j < grid.transform.childCount - 1)
                        {
                            continue;
                        }
                        MyPageParent = Instantiate(grid.transform.GetChild(j).gameObject, grid.transform);
                    }
                    // 为什么窝写的代码一股AI味啊（（（
                    if (MyPageParent == null)
                        continue;
                    MyPageParent.gameObject.SetActive(true);
                    for (int i = 0; i < MyPageParent.transform.childCount; i++)
                    {
                        if (MyPageParent.transform.GetChild(i) != null)
                        {
                            MyPage = MyPageParent.transform.GetChild(i).gameObject;
                            break;
                        }
                    }
                    if (MyPage == null)
                        continue;
                    MyPage.gameObject.SetActive(true);
                    for (int i = 0; i < MyPage.transform.childCount; i++)
                    {
                        if (MyPage.transform.GetChild(i) != null && MyPage.transform.GetChild(i).childCount >= 2 &&
                            MyPage.transform.GetChild(i).GetChild(0).name == "PacketBg" && MyPage.transform.GetChild(i).GetChild(1).name == "Packet")
                        {
                            MyCard = MyPage.transform.GetChild(i).gameObject;
                            Index = i;
                            break;
                        }
                    }
                    if (MyCard != null)
                        break;
                    else
                        continue;
                }
            }
            else if (Board.Instance != null && Board.Instance.isIZ)
            {
                GameObject grid = IZBottomMenu.Instance.plantLibrary.transform.GetChild(0).gameObject;
                String wantName = "ColorfulCards"; // 原来的彩卡GameObject名
                if (grid == null)
                    throw new NullReferenceException("找不到Grid");
                for (int j = 0; j < grid.transform.childCount; j++)
                {
                    //遍历grid找界面
                    if (grid.transform.GetChild(j) != null)
                    {
                        if (grid.transform.GetChild(j).name != wantName && j < grid.transform.childCount - 1)
                        {
                            continue;
                        }
                        MyPageParent = Instantiate(grid.transform.GetChild(j).gameObject, grid.transform);
                    }
                    // 还是复制大佬
                    if (MyPageParent == null)
                        continue;
                    MyPageParent.gameObject.SetActive(true);
                    for (int i = 0; i < MyPageParent.transform.childCount; i++)
                    {
                        if (MyPageParent.transform.GetChild(i) != null)
                        {
                            MyPage = MyPageParent.transform.GetChild(i).gameObject;
                            break;
                        }
                    }
                    if (MyPage == null)
                        continue;
                    MyPage.gameObject.SetActive(true);
                    for (int i = 0; i < MyPage.transform.childCount; i++)
                    {
                        if (MyPage.transform.GetChild(i) != null && MyPage.transform.GetChild(i).childCount >= 2 &&
                            MyPage.transform.GetChild(i).GetChild(0).name == "PacketBg" && MyPage.transform.GetChild(i).GetChild(1).name == "Packet")
                        {
                            MyCard = MyPage.transform.GetChild(i).gameObject;
                            Index = i;
                            break;
                        }
                    }
                    if (MyCard != null)
                        break;
                    else
                        continue;
                }
            }
        }*/

        public static int CardInPage => 6 * 9;

        /// <summary>
        /// 二创植物页面
        /// </summary>
        public static GameObject? MyPageParent { get; set; }

        /// <summary>
        /// 二创植物Button
        /// </summary>
        public static GameObject? MyShowCustomPlantsButton { get; set; }

        public int PageNum { get; set; } = 0;
    }
}