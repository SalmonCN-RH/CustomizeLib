using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace ZombieBoss2Remake.MelonLoader
{
    [RegisterTypeInIl2Cpp]
    public class Rv : MonoBehaviour
    {
        public Rv() : base(ClassInjector.DerivedConstructorPointer<Rv>()) => ClassInjector.DerivedConstructorBody(this);

        public Rv(IntPtr i) : base(i)
        {
        }

        public void AnimClose() => gameObject.active = false;

        public void AnimRv()
        {
            // 获取战场上的植物列表
            var plants = Board.Instance.plantArray;

            bool hasAttacked = false;

            // 逆向遍历植物列表（从最后往前）
            for (int i = plants.Count - 1; i >= 0; i--)
            {
                // 检查植物对象有效性
                if (plants[i] is null) continue;

                Plant plant = plants[i];

                // 检查植物位置是否在目标区域内：
                // 行范围 [targetRow, targetRow+1]
                // 列范围 [targetColumn, targetColumn+2]
                if (plant.thePlantRow >= zombie.targetRow &&
                    plant.thePlantRow <= zombie.targetRow + 1 &&
                    plant.thePlantColumn >= zombie.targetColumn &&
                    plant.thePlantColumn <= zombie.targetColumn + 2)
                {
                    plant.Crashed();
                    hasAttacked = true;
                }
            }

            // 如果有攻击行为播放音效
            if (hasAttacked)
            {
                // 生成8-10之间的随机音效ID
                int soundId = UnityEngine.Random.Range(8, 10);
                GameAPP.PlaySound(soundId);
            }

            // 无论是否攻击都播放编号120的音效
            GameAPP.PlaySound(120);
            foreach (var c in InGameUI.Instance.SeedBank.GetComponentsInChildren<CardUI>())
            {
                if (c is not null)
                {
                    c.CD = 0;
                }
            }
            remake.SummonMoney();
        }

        public ZombieBoss2Remake remake => transform.parent.GetComponent<ZombieBoss2Remake>();
        public ZombieBoss2 zombie => transform.parent.GetComponent<ZombieBoss2>();
    }
}