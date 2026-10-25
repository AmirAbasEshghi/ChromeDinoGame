using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using ChromeDino.Core;

namespace ChromeDino.UI
{
    // Manages gameplay UI including score, pause menu, and game over
    public class GameUIController : MonoBehaviour
    {
        [Header("Score Texts")]
        [SerializeField] private TMP_Text currentScoreText;
        [SerializeField] private TMP_Text highScoreText;

        [Header("Pause Panel")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private UnityEngine.UI.Button resumeButton;
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private UnityEngine.UI.Button mainMenuButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text finalHighScoreText;
        [SerializeField] private UnityEngine.UI.Button retryButton;
        [SerializeField] private UnityEngine.UI.Button gameOverMainMenuButton;

        private void OnEnable()
        {
            // Subscribe to GameManager events
            GameManager.OnScoreChanged += UpdateScoreUI;
            GameManager.OnHighScoreChanged += UpdateHighScoreUI;
            GameManager.OnGameOver += ShowGameOverUI;
        }

        private void OnDisable()
        {
            // Unsubscribe from GameManager events
            GameManager.OnScoreChanged -= UpdateScoreUI;
            GameManager.OnHighScoreChanged -= UpdateHighScoreUI;
            GameManager.OnGameOver -= ShowGameOverUI;
        }

        private void Start()
        {
            // Hide panels initially
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // Attach button listeners
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
            if (retryButton != null) retryButton.onClick.AddListener(RestartGame);
            if (gameOverMainMenuButton != null) gameOverMainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        private void Update()
        {
            // Toggle pause menu with Escape key
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        // Update current score text
        private void UpdateScoreUI(int score)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = $"Score: {score}";
            }
        }

        // Update high score text
        private void UpdateHighScoreUI(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"HI: {highScore}";
            }
        }

        // Toggle pause menu
        public void TogglePause()
        {
            if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

            GameManager.Instance.TogglePause();
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(GameManager.Instance.IsPaused);
            }
        }

        // Resume game
        public void ResumeGame()
        {
            TogglePause();
        }

        // Restart game
        public void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        // Return to main menu
        public void LoadMainMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
        }

        // Show game over UI
        private void ShowGameOverUI()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (finalScoreText != null && GameManager.Instance != null)
            {
                finalScoreText.text = $"Final Score: {GameManager.Instance.CurrentScore}";
            }

            if (finalHighScoreText != null && GameManager.Instance != null)
            {
                finalHighScoreText.text = $"Best Score: {GameManager.Instance.HighScore}";
            }
        }
    }
}
