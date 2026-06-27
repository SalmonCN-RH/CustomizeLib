using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx
{
    public class SkinMgr
    {
        public static bool IsPlantSkinEnable(PlantType plantType)
        {
            if (CustomCore.EnableSkin.ContainsKey(plantType))
                return CustomCore.EnableSkin[plantType];
            else
            {
                CustomCore.EnableSkin.Add(plantType, false);
                return false;
            }
        }
    }
}
