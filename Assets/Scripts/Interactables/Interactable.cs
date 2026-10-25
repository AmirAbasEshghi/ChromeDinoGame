using UnityEngine;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // Base class for interactable obstacles and items
    [RequireComponent(typeof(Collider2D))]
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] protected float speed = 6f;
        [SerializeField] protected float despawnX = -15f;

        protected virtual void Update()
        {
            // Move left
            transform.Translate(Vector3.left * (speed * Time.deltaTime), Space.World);

            // Despawn off-screen
            if (transform.position.x <= despawnX)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>() ?? other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                Interact(player);
            }
        }

        // Abstract interact method for derived classes
        public abstract void Interact(PlayerController player);
    }
}