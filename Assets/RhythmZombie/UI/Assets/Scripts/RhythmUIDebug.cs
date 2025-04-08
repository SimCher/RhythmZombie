using System;
using RhythmZombie.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace RhythmZombie.UI.Assets.Scripts
{
    public class RhythmUIDebug : MonoBehaviour
    {
        [SerializeField] private RhythmManager manager;
        [SerializeField] private TMP_Text feedbackText;

        private void Awake()
        {
            manager.HitFeedback.AddListener(UpdateFeedback);
        }

        private void OnDestroy()
        {
            manager.HitFeedback.RemoveListener(UpdateFeedback);
        }

        private void UpdateFeedback(string message)
            => feedbackText.text = message;
    }
}