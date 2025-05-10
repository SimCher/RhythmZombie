using System;
using RhythmZombie.Scripts.Objects.Arrows;
using RhythmZombie.Scripts.Rhythm.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas gameplayCanvas;
        [SerializeField] private Canvas preGameCanvas;
        [SerializeField] private Button readyBtn;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ArrowSpawner arrowSpawner;

        public event Action OnReadyBtnClicked;

        private readonly WaitForSeconds _countdownDelay = new(1f);
        private float countdownTime = 3f;

        private void Awake()
        {
            if (!gameplayCanvas || !preGameCanvas || !readyBtn || !countdownText || !audioManager || !arrowSpawner)
            {
                Debug.LogError("Missing references in UIManager.");
                enabled = false;
                return;
            }

            preGameCanvas.gameObject.SetActive(true);
            readyBtn.gameObject.SetActive(true);
            readyBtn.onClick.AddListener(() => OnReadyBtnClicked?.Invoke());
            countdownText.text = string.Empty;
            readyBtn.interactable = true;

            // audioManager.OnTrackLoaded += clip =>
            // {
            //     Debug.Log($"Track loaded: {clip.name}. Ready button enabled.");
            //     readyBtn.interactable = true;
            // };
        }

        private void OnDestroy()
        {
            if (readyBtn) readyBtn.onClick.RemoveAllListeners();
        }

        private System.Collections.IEnumerator CountdownCoroutine(Action onCountdownFinished)
        {
            readyBtn.interactable = false;
            readyBtn.gameObject.SetActive(false);

            for (int i = (int)countdownTime; i >= 0; i--)
            {
                countdownText.text = i > 0 ? i.ToString() : "GO!";
                if (i == 2)
                {
                    
                    Debug.Log($"Arrow spawning started with countdownTime: {countdownTime:F3}s");
                }
                yield return _countdownDelay;
            }

            arrowSpawner.PrepareSpawning();
            var firstArrowTravelTime = 900f / (600f * GameState.Instance.Speed);

            countdownText.text = string.Empty;
            audioManager.PlayTrack(GameState.Instance.Track, firstArrowTravelTime);
            onCountdownFinished?.Invoke();
        }

        public void StartCountdown(Action onCountdownFinished)
        {
            if (GameState.Instance.IsCalibrationMode)
            {
                var firstArrowTravelTime = 900f / (600f * GameState.Instance.Speed);
                countdownText.text = string.Empty;
                audioManager.PlayTrack(GameState.Instance.Track, firstArrowTravelTime);
                onCountdownFinished?.Invoke();
            }
            else
            {
                StartCoroutine(CountdownCoroutine(onCountdownFinished));
            }
        }

        public void SwitchToGameCanvas()
        {
            gameplayCanvas.gameObject.SetActive(true);
            preGameCanvas.gameObject.SetActive(false);
        }
    }
}