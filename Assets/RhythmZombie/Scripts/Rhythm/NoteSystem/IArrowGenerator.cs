using System.Collections.Generic;
using RhythmZombie.Scripts.Objects.Poolers;
using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    public interface IArrowGenerator
    {
        void Initialize(IObjectPooler pooler, AudioSource source, double audioStartDspTime);
        void StartGenerating(List<float> beatTimes);
    }
}