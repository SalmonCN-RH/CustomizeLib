using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable
namespace CustomizeLib.BepInEx
{
    public static class InterfaceExtensions
    {
        // 植物
        public static bool IsPlant(this IDamageable damageable, out Plant plant)
        {
            if (damageable.TryCast<Plant>() != null)
            {
                plant = damageable.TryCast<Plant>();
                return true;
            }
            plant = null;
            return false;
        }
        public static bool IsPlant(this IDamageMaker damageable, out Plant plant)
        {
            if (damageable.TryCast<Plant>() != null)
            {
                plant = damageable.TryCast<Plant>();
                return true;
            }
            plant = null;
            return false;
        }

        // 僵尸
        public static bool IsZombie(this IDamageable damageable, out Zombie zombie)
        {
            if (damageable.TryCast<Zombie>() != null)
            {
                zombie = damageable.TryCast<Zombie>();
                return true;
            }
            zombie = null;
            return false;
        }
        public static bool IsZombie(this IDamageMaker damageable, out Zombie zombie)
        {
            if (damageable.TryCast<Zombie>() != null)
            {
                zombie = damageable.TryCast<Zombie>();
                return true;
            }
            zombie = null;
            return false;
        }

        // 子弹
        public static bool IsBullet(this IDamageable damageable, out Bullet bullet)
        {
            if (damageable.TryCast<Bullet>() != null)
            {
                bullet = damageable.TryCast<Bullet>();
                return true;
            }
            bullet = null;
            return false;
        }
        public static bool IsBullet(this IDamageMaker damageable, out Bullet bullet)
        {
            if (damageable.TryCast<Bullet>() != null)
            {
                bullet = damageable.TryCast<Bullet>();
                return true;
            }
            bullet = null;
            return false;
        }

        public static IDamageable ToIDamageable(this Entity entity) => entity.Cast<IDamageable>();
        public static IDamageMaker ToIDamageMaker(this Entity entity) => entity.Cast<IDamageMaker>();
        public static IDamageMaker ToIDamageMaker(this Bullet entity) => entity.Cast<IDamageMaker>();

        // 新版调用兼容
        #region 新版受伤方法
        public static void TakeDamage(this Zombie zombie, int theDamage, Entity damageFrom, DamageType theDamageType, PlantType reportType = PlantType.Nothing, bool fix = false) =>
            zombie.TakeDamage(theDamage, damageFrom.ToIDamageMaker(), theDamageType, reportType, fix);
        public static void TakeDamage(this Zombie zombie, int theDamage, Bullet damageFrom, DamageType theDamageType, PlantType reportType = PlantType.Nothing, bool fix = false) =>
            zombie.TakeDamage(theDamage, damageFrom.ToIDamageMaker(), theDamageType, reportType, fix);

        public static void TakeDamage(this Plant plant, int damage, Entity damageFrom, DamageType damageType = DamageType.Normal, PlantType reportType = PlantType.Nothing, bool fix = false) =>
            plant.TakeDamage(damage, damageFrom.ToIDamageMaker(), damageType, reportType, fix);

        public static void TakeDamage(this Plant plant, int damage, Bullet damageFrom, DamageType damageType = DamageType.Normal, PlantType reportType = PlantType.Nothing, bool fix = false) =>
            plant.TakeDamage(damage, damageFrom.ToIDamageMaker(), damageType, reportType, fix);
        #endregion
    }
}
