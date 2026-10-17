using UnityEngine;
using ChromeDino.Player;

namespace ChromeDino.Interactables
{
    // مانع هوایی با حرکت نوسانی پرواز
    public class AirObstacle : Interactable
    {
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float bobAmplitude = 0.4f;
        [SerializeField] private float bobFrequency = 3f;

        private float startY;

        private void Start()
        {
            startY = transform.position.y;
        }

        protected override void Update()
        {
            base.Update();

            // حرکت سینوسی در راستای عمودی
            float newY = startY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        public override void Interact(PlayerController player)
        {
            player.TakeDamage(damageAmount);
            Destroy(gameObject);
        }
    }
}