using System;
using RhythmZombie.Scripts.Audio.Loaders;
using UnityEngine;

namespace RhythmZombie.Scripts.Managers
{
    [DefaultExecutionOrder(-5)]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioLoader loader;
        
        private AudioSource _audioSource;
        private bool _isTrackLoaded;

        public event Action<AudioClip, string> OnTrackLoaded;

        public bool IsTrackLoaded => _isTrackLoaded;

        public float SongTime => _audioSource.time;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void OnAudioLoaded(AudioClip clip, string filePath)
        {
            if (clip)
            {
                _audioSource.clip = clip;
                _isTrackLoaded = true;
                OnTrackLoaded?.Invoke(clip, filePath);
                Debug.Log("Аудио успешно импортировано.");
            }
            else
            {
                Debug.LogError("Не удалось извлечь AudioClip из запроса.");
            }
        }

        public void LoadTrack()
        {
            loader.OnAudioLoaded += OnAudioLoaded;
            loader.LoadAudioAsync();
        }

        public void PlayTrack()
        {
            if (_isTrackLoaded)
            {
                _audioSource.Play();
                Debug.Log("Аудио воспроизводится.");
            }
        }
    }
}