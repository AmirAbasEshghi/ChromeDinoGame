using UnityEngine;
using ChromeDino.Core;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // Collectible coin item
    public class CoinItem : Interactable
    {
        [SerializeField] private int scoreValue = 50;
        [SerializeField] private float rotateSpeed = 180f;
        [SerializeField] private AudioClip collectSound;

        protected override void Update()
        {
            base.Update();

            // Rotate coin around Y axis
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }

        public override void Interact(PlayerController player)
        {
            // Add score on pickup
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }
            else if (player != null)
            {
                player.AddScore(scoreValue);
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}