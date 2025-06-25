using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ZombieBoss2Remake.MelonLoader
{
    [RegisterTypeInIl2Cpp]
    public class BlackHole : MonoBehaviour
    {
        public BlackHole() : base(ClassInjector.DerivedConstructorPointer<BlackHole>()) => ClassInjector.DerivedConstructorBody(this);

        public BlackHole(IntPtr i) : base(i)
        {
        }

        [HideFromIl2Cpp]
        public IEnumerator Explode()
        {
            int m = 0;
            for (int i = 0; i < 600; i++)
            {
                if (gameObject is null || gameObject.IsDestroyed()) yield break;
                Cloud?.Rotate(0, 0, -5);
                if (m % 3 is 0)
                {
                    foreach (var p in Board.Instance.plantArray)
                    {
                        if (p is not null && p.thePlantHealth > 40)
                        {
                            p.thePlantHealth -= 3;
                            p.UpdateText();
                        }
                    }
                }
                m++;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            for (int j = 0; j < 10; j++)
            {
                if (gameObject is null || gameObject.IsDestroyed()) yield break;
                transform.localScale *= 0.8f;
                yield return new WaitForSeconds(Time.deltaTime);
            }
            if (gameObject is null || gameObject.IsDestroyed() || Board.Instance is null || Board.Instance.IsDestroyed()) yield break;
            foreach (var p in Board.Instance.plantArray)
            {
                if (p is not null)
                {
                    p.thePlantMaxHealth = p.thePlantHealth;
                    p.UpdateText();
                    //Board.Instance.SetDoom(p.thePlantColumn, p.thePlantRow, false, damage: 0);
                    CreateParticle.SetParticle(98, p.transform.position, p.thePlantRow);
                    GameAPP.PlaySound(41, 1.8f);
                }
            }
            ScreenShake.TriggerShake(0.4f);
            for (int j = Board.Instance.plantArray.Count - 1; j >= 0; j--)
            {
                if (Board.Instance.plantArray[j] is not null && Board.Instance.plantArray[j].thePlantType is PlantType.GoldMelon or PlantType.SuperSunNut)
                {
                    Board.Instance.plantArray[j].Die();
                }
            }
            gameObject.active = false;
            Destroy(gameObject);
        }

        public void FixedUpdate()
        {
            int radius = 18;

            // 检测范围内的子弹
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position,
                radius,
                LayerMask.GetMask("Bullet")
            );

            foreach (Collider2D col in colliders)
            {
                if (col.TryGetComponent(out Bullet bullet))
                {
                    bullet.PostionUpdate();
                    // 修改子弹状态
                    bullet.theStatus = (BulletStatus)2;
                    bullet.theMovingWay = 99;
                    // 计算方向向量
                    Vector3 direction = transform.position - bullet.transform.position;
                    Vector2 normalizedDir = direction.normalized;

                    // 计算距离相关参数
                    float distance = direction.magnitude;
                    Vector2 tangent = new Vector2(-direction.y, direction.x).normalized * 0.5f;
                    float distanceFactor = radius * 18.0f / (distance * distance);
                    Vector2 attractionForce = normalizedDir * distanceFactor + tangent;

                    // 应用物理效果
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 newVelocity = rb.velocity + attractionForce * Time.fixedDeltaTime;
                        bullet.GetComponent<Rigidbody2D>().velocity = newVelocity;
                    }

                    float absorbThreshold = transform.localScale.x * 0.5f + 0.5f;
                    if (distance < absorbThreshold)
                    {
                        // 吸收子弹
                        bullet.Die();
                    }
                }
            }
        }

        public void Start()
        {
            Cloud = gameObject.transform.FindChild("cloud");
            placeHolder = gameObject.AddComponent<Il2Cpp.BlackHole>();
            placeHolder.enabled = false;
            placeHolder.gold = true;
            MelonCoroutines.Start(Explode());
        }

        public Transform? Cloud { get; set; }
        public Il2Cpp.BlackHole? placeHolder { get; set; }
    }
}