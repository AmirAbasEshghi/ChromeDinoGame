using UnityEngine;
using ChromeDino.Core;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // سکه و آیتم‌های امتیازی
    public class CoinItem : Interactable
    {
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private float rotateSpeed = 180f;
        [SerializeField] private AudioClip collectSound;

        protected override void Update()
        {
            base.Update();

            // چرخش سکه به دور خود
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }

        public override void Interact(PlayerController player)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}