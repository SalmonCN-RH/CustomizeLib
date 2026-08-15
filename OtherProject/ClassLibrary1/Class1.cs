using UnityEngine;
using System;

namespace ClassLibrary1
{
    public class Class1
    {
        public static void Postfix() => GameLevel.RogueShooting.ShootingManager.randomType = GameLevel.RogueShooting.RandomZombieType.Default;
        public static void Template()
        {
            GameAPP.config.shootingData.randomDebuffs.Clear();
            GameAPP.config.shootingData.randomDebuffs.Add((TravelDebuff)10000);
            GameAPP.config.shootingData.randomDebuffs.Add((TravelDebuff)10016);
            GameAPP.config.shootingData.randomDebuffs.Add((TravelDebuff)10017);
            GameAPP.config.shootingData.randomDebuffs.Add((TravelDebuff)10019);
            GameAPP.config.shootingData.randomDebuffs.Add((TravelDebuff)10027);
        }

        public static void OnEnable()
        {
            Console.WriteLine("on enable");
        }

        public static void OnDisable()
        {
            Console.WriteLine("on disable");
        }
    }
}
