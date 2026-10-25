using System.Collections;
using UnityEngine;
using ChromeDino.Core;

namespace ChromeDino.Environment
{
    // Spawner for random obstacles and coins
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject[] groundObstaclePrefabs;
        [SerializeField] private GameObject[] airObstaclePrefabs;
        [SerializeField] private GameObject coinPrefab;

        [Header("Spawn Position Settings")]
        [SerializeField] private float spawnX = 15f;
        [SerializeField] private float groundY = -3.2f;
        [SerializeField] private float cactusYOffset = 0.6f;
        [SerializeField] private float[] airYOptions = new float[] { -1f, 0.5f };

        [Header("Timing Settings")]
        [SerializeField] private float minSpawnTime = 1.2f;
        [SerializeField] private float maxSpawnTime = 2.5f;
        [SerializeField] private float difficultyRampRate = 0.005f;

        private void Start()
        {
            // Start spawn loop
            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                // Stop spawning on game over
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                {
                    yield break;
                }

                // Spawn random obstacle or coin
                SpawnRandomItem();

                // Ramp up difficulty over time
                minSpawnTime = Mathf.Max(0.8f, minSpawnTime - difficultyRampRate);
                maxSpawnTime = Mathf.Max(1.4f, maxSpawnTime - difficultyRampRate);

                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);
            }
        }

        private void SpawnRandomItem()
        {
            float roll = Random.value;

            // 45% chance for ground obstacle
            if (roll < 0.45f && groundObstaclePrefabs != null && groundObstaclePrefabs.Length > 0)
            {
                int index = Random.Range(0, groundObstaclePrefabs.Length);
                Instantiate(groundObstaclePrefabs[index], new Vector3(spawnX, groundY + cactusYOffset, 0f), Quaternion.identity);
            }
            // 30% chance for air obstacle
            else if (roll < 0.75f && airObstaclePrefabs != null && airObstaclePrefabs.Length > 0)
            {
                int index = Random.Range(0, airObstaclePrefabs.Length);
                float yPos = airYOptions.Length > 0 ? airYOptions[Random.Range(0, airYOptions.Length)] : 0f;
                Instantiate(airObstaclePrefabs[index], new Vector3(spawnX, yPos, 0f), Quaternion.identity);
            }
            // 25% chance for coin
            else if (coinPrefab != null)
            {
                float coinY = Random.value > 0.5f ? groundY + 0.5f : groundY + 2f;
                Instantiate(coinPrefab, new Vector3(spawnX, coinY, 0f), Quaternion.identity);
            }
        }
    }
}
