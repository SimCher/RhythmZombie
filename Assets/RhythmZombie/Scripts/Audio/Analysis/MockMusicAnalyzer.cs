using System;
using UnityEngine;

namespace RhythmZombie.Scripts.Audio.Analysis
{
    public class MockMusicAnalyzer : MonoBehaviour, IMusicAnalyzer
    {
        [SerializeField] private float bpm = 120;

        public float BPM => bpm;

        public event Action<float> OnBeat;

        private void Start()
        {
            InvokeRepeating(nameof(TriggerBeat), 0f, 60f / bpm);
        }

        private void TriggerBeat()
        {
            OnBeat?.Invoke(Time.time);
        }

        public void Analyze(AudioClip clip)
        {
            throw new NotImplementedException();
        }
    }
}