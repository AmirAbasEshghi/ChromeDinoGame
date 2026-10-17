using UnityEngine;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // کلاس پایه برای تمام موانع و آیتم‌های تعاملی بازی
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected float speed = 6f;
        [SerializeField] protected float despawnX = -15f;

        protected virtual void Update()
        {
            // حرکت به سمت چپ صحنه
            transform.Translate(Vector3.left * (speed * Time.deltaTime), Space.World);

            // حذف آبجکت پس از خروج از کادر دید دوربین
            if (transform.position.x <= despawnX)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                Interact(player);
            }
        }

        // متد انتزاعی تعامل برای پیاده‌سازی در کلاس‌های فرزند
        public abstract void Interact(PlayerController player);
    }
}