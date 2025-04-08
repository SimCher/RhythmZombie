using System;
using UnityEngine;

namespace RhythmZombie.Scripts.Audio
{
    public class SongTimer : MonoBehaviour
    {
        private AudioSource _audioSource;

        public float CurrentTime => _audioSource.time;
        public bool IsPlaying => _audioSource.isPlaying;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void SetAudioClip(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.time = 0f;
        }

        public void Play() => _audioSource.Play();
    }
}