using System.Collections.Generic;
using UnityEngine;

namespace RhythmZombie.Scripts.GameCore
{
    public class DifficultyManager : MonoBehaviour
    {
        public enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard,
            Impossible
        }

        [field: SerializeField] public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Hard;

        private readonly Dictionary<DifficultyLevel, float> _difficultySpeeds = new()
        {
            {DifficultyLevel.Easy, 300f},
            {DifficultyLevel.Medium, 400f},
            {DifficultyLevel.Hard, 500f},
            {DifficultyLevel.Impossible, 600f}
        };

        private readonly Dictionary<DifficultyLevel, float> _spacingMultipliers = new()
        {
            {DifficultyLevel.Easy, 1.5f},
            {DifficultyLevel.Medium, 1.3f},
            {DifficultyLevel.Hard, 1.2f},
            {DifficultyLevel.Impossible, 1.1f}
        };

        public float Speed => _difficultySpeeds[CurrentDifficulty];
        public float SpacingMultiplier => _spacingMultipliers[CurrentDifficulty];
    }
}