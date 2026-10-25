using UnityEngine;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // Ground obstacle (e.g. cactus)
    public class GroundObstacle : Interactable
    {
        [SerializeField] private int damageAmount = 1;

        public override void Interact(PlayerController player)
        {
            player.TakeDamage(damageAmount);
            Destroy(gameObject);
        }
    }
}