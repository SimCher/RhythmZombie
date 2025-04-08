using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RhythmZombie.Scripts.GameCore;
using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    public class BeatmapManager : MonoBehaviour
    {
        private Beatmap _beatmap;

        private Queue<float> _beatsQueue;

        [SerializeField] private DifficultyManager difficultyManager;

        public Queue<float> CurrentBeats => _beatsQueue;

        public void LoadBeatmap(string audioFilePath)
        {
            var jsonPath = Path.Combine(Application.dataPath, "RhythmZombie/beatmap.json");
            if (!File.Exists(jsonPath)) return;

            var json = File.ReadAllText(jsonPath);
            _beatmap = JsonConvert.DeserializeObject<Beatmap>(json);

            _beatsQueue = new Queue<float>(_beatmap.GetBeatsByDifficulty(difficultyManager.CurrentDifficulty));
        }
    }
}