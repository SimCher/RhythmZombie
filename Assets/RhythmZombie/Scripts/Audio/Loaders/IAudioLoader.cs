using UnityEngine;

namespace RhythmZombie.Scripts.Audio.Loaders
{
    public interface IAudioLoader
    {
        void LoadAudioAsync();
        event System.Action<AudioClip, string> OnAudioLoaded;
    }
}