using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    /// <summary>
    /// 旧版本DmgType留存实现
    /// </summary>
    public enum DmgType
    {
        Normal = 0,
        NormalAll = 1,
        Ice = 2,
        IceAll = 3,
        Shieldless = 4,
        IceShieldless = 5,
        RealDamage = 6,
        Explode = 10,
        Squash = 11,
        Carred = 12,
        Hammer = 13,
        MaxDamage = 14,
        CherryExplode = 15,
        JackboxExplode = 16,
        UltimateTallNutAll = 17,
        DoomExplode = 18,
        UltimateBamboo = 19
    }
}
