using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChromeDino.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreChanged;
        public static event Action OnGameOver;

        [Header("Score Settings")]
        [SerializeField] private int currentScore = 0;
        private int highScore = 0;

        private bool isGameOver = false;
        private bool isPaused = false;

        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load high score from PlayerPrefs
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private float scoreTimer = 0f;
        [SerializeField] private float scoreRate = 10f;

        private void Start()
        {
            Time.timeScale = 1f;
            OnScoreChanged?.Invoke(currentScore);
            OnHighScoreChanged?.Invoke(highScore);
        }

        private void Update()
        {
            if (isGameOver || isPaused) return;

            // Increase score continuously while running
            scoreTimer += Time.deltaTime * scoreRate;
            if (scoreTimer >= 1f)
            {
                int pointsToAdd = Mathf.FloorToInt(scoreTimer);
                scoreTimer -= pointsToAdd;
                AddScore(pointsToAdd);
            }
        }

        // Add score and update high score
        public void AddScore(int amount)
        {
            if (isGameOver || amount <= 0) return;

            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore);

            // Save new high score if beaten
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
                OnHighScoreChanged?.Invoke(highScore);
            }
        }

        // Game over
        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
        }

        // Toggle pause
        public void TogglePause()
        {
            if (isGameOver) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }

        // Restart current game
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Return to main menu
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}