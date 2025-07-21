# 窝是标题
Release每个大版本更新一次，有想弄最新的可以自己编译一下，想发就发

哪天实在弄不会了或者自己忙了就不更了.jpg

如果有那天让[听雨夜荷](https://github.com/CarefreeSongs712)接着更[doge]

愿融合版有更多的modder

最后纪念伟大的Infinite75，撑起了二创界的半壁江山（？）

↓芝士原来的README↓

# PVZRHCustomization
植物大战僵尸融合版二创植物与僵尸 by [@Infinite75](https://space.bilibili.com/672619350)    
适配游戏版本2.7      
已构建版本的链接在Release中    
CustomizeLib和我的模组未来想使用和适配请自便，在遵守开源协议的前提下默认授权

基于[MelonLoader](https://github.com/LavaGang/MelonLoader)与[BepInEx](https://github.com/BepInEx/BepInEx)开发      

融合版制作组：    
[@蓝飘飘fly](https://space.bilibili.com/3546619314178489) 请在此处下载游戏本体  
[@机鱼吐司](https://space.bilibili.com/85881762)   
[@梦珞呀](https://space.bilibili.com/270840380)    
[@蓝蝶蝶Starryfly](https://space.bilibili.com/27033629)    

感谢[@理科疯子](https://space.bilibili.com/237491236)(Github:[@likefengzi](https://github.com/likefengzi))的技术支持  
贴图替换部分使用了[PvZ-Fusion-Blooms](https://github.com/Dynamixus/PvZ-Fusion-Blooms)的代码，感谢Blooms开发组      
感谢[@高数带我飞](https://space.bilibili.com/1117414477)(Github:[@LibraHp](https://github.com/LibraHp/))的技术支持    

PVZ Fusion Customized Plants and Zombies    
Game Version: 2.7    
Please download in Release page    

Based on [MelonLoader](https://github.com/LavaGang/MelonLoader) and [BepInEx](https://github.com/BepInEx/BepInEx)     

PVZ Fusion Dev Team:     
[@蓝飘飘fly](https://space.bilibili.com/3546619314178489)     
[@机鱼吐司](https://space.bilibili.com/85881762)   
[@梦珞呀](https://space.bilibili.com/270840380)    
[@蓝蝶蝶Starryfly](https://space.bilibili.com/27033629)    


Thanks for [@理科疯子](https://space.bilibili.com/237491236)(Github:[@likefengzi](https://github.com/likefengzi))'s help 
Used codes of [PvZ-Fusion-Blooms](https://github.com/Dynamixus/PvZ-Fusion-Blooms) to replace texures, thanks for Blooms       
Thanks for [@高数带我飞](https://space.bilibili.com/1117414477)(Github:[@LibraHp](https://github.com/LibraHp/))'s help 


### 贴图替换 thanks to BloomsTeam
1.安装好游戏、前置和二创包并运行一遍游戏    
2.获取游戏解包资源：使用[AssetRipper](https://github.com/AssetRipper/AssetRipper/releases)工具      
3.在解包资源里找到你要修改的贴图/部件图，用PS等软件修改，然后将贴图另存至：   
游戏根目录\BepInEx\plugins\Textures(b版) 或 
游戏根目录\Mods\Textures(m版)     
要求：不可以改文件名，不可以改图片大小，必须使用png，不能用psd,psb     
4.启动游戏，检查替换是否生效     

不保证所有图片均能生效，某些贴图修改后超出原图非空像素/矩形区域的部分可能会被截掉

### 添加自定义植物 made by [@likefengzi](https://github.com/likefengzi)

#### 子弹类型

```C#

// Decompiled with JetBrains decompiler
// Type: Il2Cpp.BulletType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BA5295-8D17-4B4A-B7AA-4FDC00AA3B5E
// Assembly location: C:\STUDY\Code\Rider\PlantsVsZombiesRHCustomPlantsMelonLoader\libs\Il2CppAssemblies\Assembly-CSharp.dll

using Il2CppInterop.Common.Attributes;

#nullable disable
namespace Il2Cpp
{
  [OriginalName("Assembly-CSharp.dll", "", "BulletType")]
  public enum BulletType
  {
    Bullet_pea = 0,
    Bullet_cherry = 1,
    Bullet_nut = 2,
    Bullet_superCherry = 3,
    Bullet_zombieBlock = 4,
    Bullet_zombieBlock2 = 5,
    Bullet_zombieBlock3 = 6,
    Bullet_potato = 7,
    Bullet_smallSun = 8,
    Bullet_puff = 9,
    Bullet_puffPea = 10, // 0x0000000A
    Bullet_ironPea = 11, // 0x0000000B
    Bullet_threeSpike = 12, // 0x0000000C
    Bullet_puffRandomColor = 13, // 0x0000000D
    Bullet_puffLove = 14, // 0x0000000E
    Bullet_snowPea = 15, // 0x0000000F
    Bullet_snowPuffPea = 16, // 0x00000010
    Bullet_iceSpark = 17, // 0x00000011
    Bullet_smallIceSpark = 18, // 0x00000012
    Bullet_magicTrack = 20, // 0x00000014
    Bullet_snowPuff = 21, // 0x00000015
    Bullet_blackPuff = 22, // 0x00000016
    Bullet_doom = 23, // 0x00000017
    Bullet_iceDoom = 24, // 0x00000018
    Bullet_firePea_yellow = 25, // 0x00000019
    Bullet_firePea_orange = 26, // 0x0000001A
    Bullet_firePea_red = 27, // 0x0000001B
    Bullet_squash = 28, // 0x0000001C
    Bullet_kelp = 29, // 0x0000001D
    Bullet_fireKelp = 30, // 0x0000001E
    Bullet_squashKelp = 32, // 0x00000020
    Bullet_normalTrack = 33, // 0x00000021
    Bullet_iceTrack = 34, // 0x00000022
    Bullet_fireTrack = 35, // 0x00000023
    Bullet_cherrySquash = 36, // 0x00000024
    Bullet_cactus = 37, // 0x00000025
    Bullet_lanternCactus_glow = 38, // 0x00000026
    Bullet_star = 39, // 0x00000027
    Bullet_lanternStar = 40, // 0x00000028
    Bullet_seaStar = 41, // 0x00000029
    Bullet_cactusStar = 42, // 0x0000002A
    Bullet_jackboxStar = 43, // 0x0000002B
    Bullet_pickaxeStar = 44, // 0x0000002C
    Bullet_magnetStar = 45, // 0x0000002D
    Bullet_ironStar = 46, // 0x0000002E
    Bullet_magnetCactus = 47, // 0x0000002F
    Bullet_superStar = 48, // 0x00000030
    Bullet_ultimateStar = 49, // 0x00000031
    Bullet_firePea_small = 50, // 0x00000032
    Bullet_sword = 51, // 0x00000033
    Bullet_cabbage = 52, // 0x00000034
    Bullet_sunCabbage = 53, // 0x00000035
    Bullet_kernal = 54, // 0x00000036
    Bullet_butter = 55, // 0x00000037
    Bullet_bigKernal = 56, // 0x00000038
    Bullet_bigButter = 57, // 0x00000039
    Bullet_basketball = 58, // 0x0000003A
    Bullet_melon = 59, // 0x0000003B
    Bullet_winterMelon = 60, // 0x0000003C
    Bullet_garlicKernal = 61, // 0x0000003D
    Bullet_garlicButter = 62, // 0x0000003E
    Bullet_garlicCabbage = 63, // 0x0000003F
    Bullet_garlicMelon = 64, // 0x00000040
    Bullet_cannon = 65, // 0x00000041
    Bullet_cornMelon = 66, // 0x00000042
    Bullet_butterMelon = 67, // 0x00000043
    Bullet_fireCannon = 68, // 0x00000044
    Bullet_iceCannon = 69, // 0x00000045
    Bullet_cabbageMelon = 70, // 0x00000046
    Bullet_superMelon = 71, // 0x00000047
    Bullet_ultimateMelon = 72, // 0x00000048
    Bullet_melonCannon = 73, // 0x00000049
    Bullet_silverCabbage = 74, // 0x0000004A
    Bullet_goldCabbage = 75, // 0x0000004B
    Bullet_silverKernal = 76, // 0x0000004C
    Bullet_goldKernal = 77, // 0x0000004D
    Bullet_silverButter = 78, // 0x0000004E
    Bullet_goldButter = 79, // 0x0000004F
    Bullet_smallGoldCannon = 80, // 0x00000050
    Bullet_silverMelon = 81, // 0x00000051
    Bullet_goldMelon = 82, // 0x00000052
    Bullet_goldMelonCannon = 83, // 0x00000053
    Bullet_ultimateCannon = 84, // 0x00000054
    Bullet_ultimateKernal = 85, // 0x00000055
    Bullet_fireMelon = 86, // 0x00000056
    Bullet_firePea_super = 87, // 0x00000057
    Bullet_garlicBomb = 89, // 0x00000059
    Bullet_puffIronPea = 90, // 0x0000005A
    Bullet_lourCactus = 91, // 0x0000005B
    Bullet_puffPotato = 92, // 0x0000005C
    Bullet_steelPea = 93, // 0x0000005D
    Bullet_shulkLeaf = 94, // 0x0000005E
    Bullet_sunSpike = 95, // 0x0000005F
    Bullet_endoSun = 96, // 0x00000060
    Bullet_extremeSnowPea = 97, // 0x00000061
    Bullet_silverCoin = 98, // 0x00000062
    Bullet_goldCoin = 99, // 0x00000063
    Bullet_ultimateCactus = 100, // 0x00000064
    Bullet_snowBall = 101, // 0x00000065
    Bullet_doomCactus = 102, // 0x00000066
    Bullet_bigMelon = 103, // 0x00000067
    Bullet_fireStar = 104, // 0x00000068
    Bullet_fireCherry = 105, // 0x00000069
    Bullet_lanternCactus_dark = 106, // 0x0000006A
    Bullet_firePea_ultimate = 107, // 0x0000006B
    Bullet_redIronPea = 108, // 0x0000006C
    Bullet_normalSun = 109, // 0x0000006D
    Bullet_firePea_purple = 110, // 0x0000006E
    Bullet_springMelon = 111, // 0x0000006F
    Bullet_smallKernal = 112, // 0x00000070
    Bullet_smallButter = 113, // 0x00000071
    Bullet_portalPea = 114, // 0x00000072
    Bullet_goldSquashKelp = 115, // 0x00000073
    Bullet_ironPea_air = 116, // 0x00000074
    Bullet_kernal_magnet = 117, // 0x00000075
    Bullet_butter_magnet = 118, // 0x00000076
    Bullet_kernal_portal = 119, // 0x00000077
    Bullet_butter_portal = 120, // 0x00000078
    Bullet_kernal_iron = 121, // 0x00000079
    Bullet_butter_iron = 122, // 0x0000007A
    Bullet_pea_doom = 123, // 0x0000007B
    Bullet_pea_doom_fire = 124, // 0x0000007C
    Bullet_doom_fire = 125, // 0x0000007D
    Bullet_doom_big = 126, // 0x0000007E
    Bullet_spruce = 127, // 0x0000007F
    Bullet_spruceShulk = 128, // 0x00000080
    Bullet_icePeach = 129, // 0x00000081
    Bullet_passionFruit = 130, // 0x00000082
    Bullet_passionFruit2 = 131, // 0x00000083
    Bullet_water = 132, // 0x00000084
    Bullet_iceBlock = 133, // 0x00000085
    Bullet_water_big = 134, // 0x00000086
    Bullet_iceBlock_big = 135, // 0x00000087
    Bullet_shulkLeaf_water = 136, // 0x00000088
    Bullet_iceBlock_recover = 137, // 0x00000089
    Bullet_shulkLeaf_ice = 138, // 0x0000008A
    Bullet_lotusSpruce = 139, // 0x0000008B
    Bullet_kernal_ultimate = 140, // 0x0000008C
    Bullet_BlackHole_gold = 141, // 0x0000008D
  }
}

```

#### 子弹移动方式

```C#

// Decompiled with JetBrains decompiler
// Type: Il2Cpp.BulletMoveWay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4BA5295-8D17-4B4A-B7AA-4FDC00AA3B5E
// Assembly location: C:\STUDY\Code\Rider\PlantsVsZombiesRHCustomPlantsMelonLoader\libs\Il2CppAssemblies\Assembly-CSharp.dll

using Il2CppInterop.Common.Attributes;

#nullable disable
namespace Il2Cpp
{
  [OriginalName("Assembly-CSharp.dll", "", "BulletMoveWay")]
  public enum BulletMoveWay
  {
    //所有的子弹都依赖植物索敌
    //跳跃，不移动BUG
    Jump = -1, // 0xFFFFFFFF
    //默认
    MoveRight = 0,
    //滚动
    Roll = 1,
    //没看出来和默认的区别
    Free = 2,
    //飞行四格后消失
    Puff = 3,
    //三线射手，上路
    Three_up = 4,
    //三线射手，下路
    Three_down = 5,
    //追踪
    Track = 6,
    //飞行，只能击中空中僵尸
    Fly = 7,
    //不知道和上面那个有什么区别
    Fly2 = 8,
    //往左打
    Left = 9,
    //子弹停留在原地
    Stable = 10, // 0x0000000A
    //子弹停留在原地，没看出来和上一个的区别
    Rounding = 11, // 0x0000000B
    //地面空中都可以击中
    LittleFly = 12, // 0x0000000C
    //抛物线，已修复
    Throw = 13, // 0x0000000D
    //加农炮，修复部分，已经可以击中
    Cannon = 14, // 0x0000000E
    //没看出来和默认的区别
    Right_free = 15, // 0x0000000F
    //追踪，地面空中都可以击中，但是没伤害
    Convolute = 16, // 0x00000010
    //抛物线，已修复
    Quick_throw = 17, // 0x00000011
    //地面空中都可以击中
    Freefly = 18, // 0x00000012
    //往左打，反弹
    Split_left = 19, // 0x00000013
    //对地无伤害，对空正常
    Track_air_singleRow = 20, // 0x00000014
    //移动轨迹
    Sin = 21, // 0x00000015
    //移动轨迹
    Cos = 22, // 0x00000016
    //子弹停留在原地，没看出来和上一个的区别
    Pirouette = 23, // 0x00000017
  }
}

```

### 自动加载皮肤文件

- 皮肤文件命名"skin_"+id

- 预制体命名"Prefab"

- 预览图命名"Preview"

- 路径Mods/Skin/

- json文件命名"skin_"+id+".json"

#### json文件格式

json不支持注释，不要在json文件里写注释

```C#
//文件名中间的ID必须是要换皮的植物
{
  "CustomPlantData": {
    //植物ID，你要换魂的植物，使用他的逻辑
    //如果皮肤和灵魂的植物血缘关系太远，有可能出BUG
    //像普通豌豆射手和超级樱桃豌豆射手这种，单发射手一脉相承，逻辑上没有差太多
    //但是射手和投手就差的太多了，子弹会出问题
    //坚果类植物换魂变成射手类植物，需要重做动画
    "ID": 931,
    //植物数据
    "PlantData": {
      //攻击力
      //会受到词条影响，最终的伤害会有偏差
      "attackDamage": 100,
      //植物ID
      //这个必须和换皮植物一致
      "field_Public_PlantType_0": 931,
      //攻击间隔
      //不是攻击类植物就写0
      "field_Public_Single_0": 0,
      //生产间隔
      //不是向日葵类植物就写0
      "field_Public_Single_1": 0,
      //MaxHP
      "field_Public_Int32_0": 100000,
      //CD
      "field_Public_Single_2": 20,
      //阳光价格
      //如果非基础类植物，一般情况是用不到的
      "field_Public_Int32_1": 1000
    }
    //其实这个类还有两个成员，但是必须留空，不然无法读取
  },
  //自定义射击时用的子弹
  //可以写很多，调用CustomAnimShoot随机发射
  //key是子弹类型，value是子弹移动方式
  "CustomBulletType": {
    "0": 0,
    "0": 0
  },
  //下面的这些数据，0表示禁止该特性，1表示启用该特性
  //-1表示使用换皮植物的特性
  //不是所有的特性改成1都能生效，有些特性还是得代码配合
  "TypeMgrExtraSkin": {
    //大坚果
    "BigNut": -1,
    //大僵尸
    "BigZombie": -1,
    //双格植物
    "DoubleBoxPlants": -1,
    //领袖僵尸
    "EliteZombie": -1,
    //飞行植物
    "FlyingPlants": -1,
    //飞行僵尸
    "IsAirZombie": -1,
    //地刺植物
    "IsCaltrop": -1,
    //自定义植物
    "IsCustomPlant": -1,
    //火焰植物
    "IsFirePlant": -1,
    //寒冰植物
    "IsIcePlant": -1,
    //磁力植物
    "IsMagnetPlants": -1,
    //坚果植物
    "IsNut": -1,
    //灯笼植物
    "IsPlantern": -1,
    //花盆植物
    "IsPot": -1,
    //地雷植物
    "IsPotatoMine": -1,
    //小喷菇类
    //可以一格三个，非低矮植物
    "IsPuff": -1,
    //南瓜植物
    "IsPumpkin": -1,
    //小灯笼
    "IsSmallRangeLantern": -1,
    //特殊植物
    "IsSpecialPlant": -1,
    //超时空植物
    "IsSpickRock": -1,
    //高坚果植物
    "IsTallNut": -1,
    //缠绕海草
    "IsTangkelp": -1,
    //水生植物
    "IsWaterPlant": -1,
    //
    "NotRandomBungiZombie": -1,
    //
    "NotRandomZombie": -1,
    //究极僵尸
    "UltimateZombie": -1,
    //伞类植物
    "UmbrellaPlants": -1,
    //不可魅惑的僵尸
    "UselessHypnoZombie": -1,
    //水生僵尸
    "WaterZombie": -1
  },
  //图鉴数据
  "PlantAlmanac": {
    "ID": 3,
    "Name": "超载凯露",
    "Description": "超载凯露"
  }
}
```

#### AnimEvent

```C#
//没有参数
public void CustomAnimShoot(){}
//num是回血百分比
public void CustomAnimHeal(float num)
```

