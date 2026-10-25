using UnityEngine;
using UnityEngine.UI;
using ChromeDino.Player;

namespace ChromeDino.UI
{
    // Manages player health display in UI
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private GameObject[] heartIcons;

        private void OnEnable()
        {
            // Subscribe to player health change event
            PlayerController.OnHealthChanged += UpdateHealthDisplay;
        }

        private void OnDisable()
        {
            // Unsubscribe on disable
            PlayerController.OnHealthChanged -= UpdateHealthDisplay;
        }

        // Update health slider or heart icons
        private void UpdateHealthDisplay(int currentHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (heartIcons != null)
            {
                for (int i = 0; i < heartIcons.Length; i++)
                {
                    if (heartIcons[i] != null)
                    {
                        heartIcons[i].SetActive(i < currentHealth);
                    }
                }
            }
        }
    }
}
