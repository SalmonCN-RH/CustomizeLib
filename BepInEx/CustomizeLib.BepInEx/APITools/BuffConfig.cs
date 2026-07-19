using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public struct BuffConfig
    {
        /// <summary>
        /// ID，不指定为自动分配
        /// </summary>
        public int? ID;
        /// <summary>
        /// 描述
        /// </summary>
        public string desc = "";
        /// <summary>
        /// 词条类型
        /// </summary>
        public BuffType type;
        /// <summary>
        /// 显示的植物
        /// </summary>
        public PlantType iconPlant;
        /// <summary>
        /// 显示的僵尸
        /// </summary>
        public ZombieType iconZombie;
        /// <summary>
        /// 最大等级
        /// </summary>
        public int maxLevel = 1;
        /// <summary>
        /// 解锁条件
        /// </summary>
        public Func<bool> unlock = () => true;
        /// <summary>
        /// 价格
        /// </summary>
        public int cost = 5000;
        /// <summary>
        /// 背景
        /// </summary>
        public BuffBgType backGround = BuffBgType.Day;
        /// <summary>
        /// 绑定的弱究
        /// </summary>
        public PlantType lockPlantType;
        /// <summary>
        /// 弱究增加概率
        /// </summary>
        public bool probably = false;

        public BuffConfig() { }
    }
}
