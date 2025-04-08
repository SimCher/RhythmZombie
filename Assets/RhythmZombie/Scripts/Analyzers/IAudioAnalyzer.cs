using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Analyzers
{
    public interface IAudioAnalyzer
    {
        void Analyze(AudioClip clip);
        event System.Action<Beatmap> OnAnalysisComplete;
    }
}