using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Панели канваса")]
        [SerializeField] private Canvas gameplayCanvas;

        [SerializeField] private Canvas preGameCanvas;

        [Header("UI элементы")]
        [SerializeField] private Button readyBtn;

        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private TMP_Text loadingText;

        [Header("Игровые контроллеры")]
        [SerializeField] private AudioManager audioManager;

        public event Action OnReadyBtnClicked;

        private readonly WaitForSeconds _countdownDelay = new(1f);

        private void Awake()
        {
            if(gameplayCanvas) gameplayCanvas.gameObject.SetActive(false);
            if(preGameCanvas) preGameCanvas.gameObject.SetActive(true);

            if (readyBtn)
            {
                readyBtn.interactable = false;
                readyBtn.onClick.AddListener(() => OnReadyBtnClicked?.Invoke());
            }

            if (countdownText)
                countdownText.text = string.Empty;
        }

        private void Update()
        {
            if (audioManager.IsTrackLoaded && !readyBtn.interactable)
                readyBtn.interactable = true;
        }

        private IEnumerator CountdownCoroutine(Action onCountdownFinished)
        {
            for (int i = 3; i >= 0; i--)
            {
                countdownText.text = i > 0 ? i.ToString() : "GO!";
                yield return _countdownDelay;
            }

            countdownText.text = string.Empty;
            onCountdownFinished?.Invoke();
        }

        public void StartCountdown(Action onCountdownFinished)
        {
            readyBtn.interactable = false;
            readyBtn.gameObject.SetActive(false);

            StartCoroutine(CountdownCoroutine(onCountdownFinished));
        }

        public void SwitchToGameCanvas()
        {
            gameplayCanvas.gameObject.SetActive(true);
            preGameCanvas.gameObject.SetActive(false);
        }
    }
}