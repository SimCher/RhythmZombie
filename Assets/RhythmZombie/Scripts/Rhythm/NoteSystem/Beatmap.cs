using System;
using System.Collections.Generic;
using RhythmZombie.Scripts.GameCore;
using RhythmZombie.Scripts.Managers;
using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    [System.Serializable]
    public class Beatmap
    {
        [field: SerializeField] public List<float> Easy { get; set; }
        [field: SerializeField] public List<float> Medium { get; set; }
        [field: SerializeField] public List<float> Hard { get; set; }
        [field: SerializeField] public List<float> Impossible { get; set; }

        public List<float> GetBeatsByDifficulty(DifficultyManager.DifficultyLevel difficulty) =>
            difficulty switch
            {
                DifficultyManager.DifficultyLevel.Easy => Easy,
                DifficultyManager.DifficultyLevel.Medium => Medium,
                DifficultyManager.DifficultyLevel.Hard => Hard,
                DifficultyManager.DifficultyLevel.Impossible => Impossible,
                _ => Medium
            };
    }
}