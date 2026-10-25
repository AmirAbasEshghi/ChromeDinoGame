using System.Collections;
using UnityEngine;
using ChromeDino.Core;

namespace ChromeDino.Environment
{
    // Day and night cycle based on score milestones
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Cycle Settings")]
        [SerializeField] private int scoreInterval = 500;
        [SerializeField] private float transitionDuration = 2.0f;

        [Header("Sky Colors")]
        [SerializeField] private Color dayColor = new Color(0.97f, 0.97f, 0.97f);
        [SerializeField] private Color nightColor = new Color(0.13f, 0.13f, 0.15f);

        private Camera mainCam;
        private bool isNight = false;
        private int lastCycleScore = 0;
        private Coroutine transitionRoutine;

        private void Awake()
        {
            mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = dayColor;
            }
        }

        private void OnEnable()
        {
            GameManager.OnScoreChanged += CheckScoreForCycle;
        }

        private void OnDisable()
        {
            GameManager.OnScoreChanged -= CheckScoreForCycle;
        }

        // Check score milestone for day and night transition
        private void CheckScoreForCycle(int score)
        {
            if (score - lastCycleScore >= scoreInterval)
            {
                lastCycleScore = score;
                ToggleDayNight();
            }
        }

        // Toggle day and night
        public void ToggleDayNight()
        {
            isNight = !isNight;
            Color targetColor = isNight ? nightColor : dayColor;

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }
            transitionRoutine = StartCoroutine(TransitionColor(targetColor));
        }

        // Smooth background color transition
        private IEnumerator TransitionColor(Color target)
        {
            if (mainCam == null) yield break;

            Color start = mainCam.backgroundColor;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                mainCam.backgroundColor = Color.Lerp(start, target, elapsed / transitionDuration);
                yield return null;
            }

            mainCam.backgroundColor = target;
        }
    }
}
