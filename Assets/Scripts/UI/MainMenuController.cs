using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

namespace ChromeDino.UI
{
    // Main menu controller and button handlers
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Settings")]
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private string gameSceneName = "SampleScene";

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            Time.timeScale = 1f;

            // Hook button click listeners
            if (playButton != null) playButton.onClick.AddListener(PlayGame);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            // Display saved high score from PlayerPrefs
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {highScore}";
            }
        }

        private void Update()
        {
            // Quick start game with Space or Enter
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                PlayGame();
            }
        }

        // Load gameplay scene
        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        // Quit application
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
