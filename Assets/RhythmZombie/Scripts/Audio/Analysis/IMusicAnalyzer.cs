using System;
using UnityEngine;

namespace RhythmZombie.Scripts.Audio.Analysis
{
    public interface IMusicAnalyzer
    {
        float BPM { get; }
        
        /// <summary>
        /// Событие на каждый бит
        /// </summary>
        event Action<float> OnBeat;
        
        void Analyze(AudioClip clip);
    }
}