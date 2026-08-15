using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CustomizeLib.BepInEx
{
    public static class PlantExtensions
    {
        public static void TakeDamage(this Zombie zombie, DmgType theDamageType, int theDamage, PlantType reportType = PlantType.Nothing, bool fix = false)
            => zombie.TakeDamage(theDamage, CustomDamageMaker.DamageMaker, (DamageType)(int)theDamageType, reportType, fix);
        public static void TakeDamage(this Plant plant, int damage, int damageType = 0) =>
            plant.TakeDamage(damage, CustomDamageMaker.DamageMaker, (DamageType)damageType);

        public static void DisableDisMix(this Plant plant) => (plant.firstParent, plant.secondParent) = (PlantType.Nothing, PlantType.Nothing);

        //递归，找shoot，但是一些奇怪的植物不行
        public static void FindShoot(this Plant plant, Transform parent)
        {
            String name = parent.name.ToLower();
            if (name == "shoot" || name == "shoot1")
                plant.shoot = parent;
            if (name == "shoot2")
                plant.shoot2 = parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                plant.FindShoot(parent.GetChild(i));
            }
        }

        public static int GetTotalMaxHealth(this Zombie zombie) => 
            (int)zombie.theMaxHealth + zombie.theFirstArmorMaxHealth + zombie.theSecondArmorMaxHealth;
        public static int GetTotalHealth(this Zombie zombie) => (int)zombie.theHealth + zombie.theFirstArmorHealth + zombie.theSecondArmorHealth;

        public static TextMeshProUGUI RegisterText(this Plant plant, Color color, Func<string> func, Vector2? size = null)
        {
            if (func == null) return null;
            if (plant == null || plant.healthSlider == null) return null;
            var healthText = plant.healthSlider.healthTextContainer.gameObject.AddComponent<CustomHealthText>();
            var text = UnityEngine.Object.Instantiate(plant.healthSlider.healthText, plant.healthSlider.healthTextContainer).GetComponent<TextMeshProUGUI>();
            text.color = color;
            text.gameObject.SetActive(true);
            text.text = func.Invoke();
            healthText.registedTexts.Add(text, func);
            if (size != null)
                text.GetComponent<RectTransform>().sizeDelta = size.Value;
            return text;
        }

        public static void ClearAllText(this Plant plant)
        {
            if (plant.healthSlider == null) return;
            foreach (var kvp in plant.healthSlider.registedTexts)
                UnityEngine.Object.Destroy(kvp.Key.gameObject);
            plant.healthSlider.registedTexts.Clear();
        }
    }
}
