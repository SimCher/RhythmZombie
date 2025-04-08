using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RhythmZombie.Scripts.Audio.Loaders;
using RhythmZombie.Scripts.Objects.Arrows;
using RhythmZombie.Scripts.Objects.Poolers;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private AudioSource _audioSource;

        private Beatmap _beatmap;

        [SerializeField] private AudioLoader loader;
        [SerializeField] private ArrowPooler pooler;
        [SerializeField] private RectTransform targetZone;
        [SerializeField] private RectTransform spawnPoint;
        [SerializeField] private RectTransform lane;
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Easy;

        private Dictionary<DifficultyLevel, float> _difficultySpeeds;
        private Queue<float> _currentBeatsQueue = new();
        
        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();

            loader.OnAudioLoaded += GenerateBeatmap;

            _difficultySpeeds = new Dictionary<DifficultyLevel, float>
            {
                {DifficultyLevel.Easy, 300f},
                {DifficultyLevel.Medium, 400f},
                {DifficultyLevel.Hard, 500f},
                {DifficultyLevel.Impossible, 600f}
            };
            
            loader.LoadAudioAsync();
        }

        private void GenerateBeatmap(AudioClip clip, string filePath)
        {
            var jsonPath = Path.Combine(Application.dataPath, "RhythmZombie/beatmap.json");
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                _beatmap = JsonConvert.DeserializeObject<Beatmap>(json);
                Debug.Log($"Загружен Beatmap с {_beatmap.Easy.Count} простых битов");

                // _currentBeatsQueue = new Queue<float>(_beatmap.GetBeatsByDifficulty(currentDifficulty));
            }
        
            if (clip)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
                Debug.Log("Аудио успешно загружено и воспроизводится.");
            }
            else
            {
                Debug.LogError("Не удалось извлечь AudioClip из запроса.");
            }
        }

        private void Update()
        {
            if (_audioSource.isPlaying)
            {
                var songTime = _audioSource.time;
                CheckBeats(songTime, currentDifficulty);
            }
        }

        private void SpawnArrow(float beatTime, DifficultyLevel difficulty)
        {
            Debug.Log("Spawn");
            var arrow = pooler.NextPooled;
            var arrowBehaviour = arrow.GetComponent<ArrowBehaviour>();
            var speed = _difficultySpeeds[difficulty];
            var spawnPos = new Vector2(spawnPoint.anchoredPosition.x, lane.anchoredPosition.y);
            arrowBehaviour.Initialize(beatTime, speed, targetZone, spawnPos, pooler);
        }

        private void CheckBeats(float currentTime, DifficultyLevel difficulty)
        {
            if (_currentBeatsQueue.Count > 0)
            {
                var nextBeat = _currentBeatsQueue.Peek();
                var speed = _difficultySpeeds[difficulty];
                var travelDistance = spawnPoint.anchoredPosition.x - targetZone.anchoredPosition.x;
                var travelTime = travelDistance / speed;
                var spawnTime = nextBeat - travelTime;

                if (currentTime >= spawnTime)
                {
                    SpawnArrow(nextBeat, difficulty);
                    _currentBeatsQueue.Dequeue();
                }
            }
        }
        public enum DifficultyLevel { Easy, Medium, Hard, Impossible }
        //
        // [SerializeField] private AudioManager audioManager;
        // [SerializeField] private BeatDetector beatDetector;
        // [SerializeField] private ArrowPooler arrowPooler;
        // [SerializeField] private ArrowGenerator arrowGenerator;
        // [SerializeField] private DifficultyLevel difficulty = DifficultyLevel.Medium;
        // [SerializeField] private float arrowSpawnOffset = 1f;
        //
        // private AudioSource audioSource;
        // private double audioStartDspTime;
        // private bool isInitialized;
        // private bool isGameReady;
        // private Beatmap currentBeatmap;
        //
        // private void Awake()
        // {
        //     ValidateDependencies();
        //     audioSource = audioManager.AudioSource;
        //     RegisterEvents();
        // }
        //
        // private void ValidateDependencies()
        // {
        //     if (audioManager == null)
        //         Debug.LogError("AudioManager not assigned in GameManager!");
        //     if (beatDetector == null)
        //         Debug.LogError("BeatDetector not assigned in GameManager!");
        //     if (arrowPooler == null)
        //         Debug.LogError("ArrowPooler not assigned in GameManager!");
        //     if (arrowGenerator == null)
        //         Debug.LogError("ArrowGenerator not assigned in GameManager!");
        // }
        //
        // private void RegisterEvents()
        // {
        //     audioManager.OnAudioLoaded += HandleAudioLoaded;
        //     audioManager.OnAudioPlay += HandleAudioPlay;
        //     beatDetector.OnBeatmapDetected += HandleBeatmapDetected;
        // }
        //
        // private void Start()
        // {
        //     audioManager.LoadAndPlay();
        // }
        //
        // private void HandleAudioLoaded(AudioClip clip)
        // {
        //     if (!isInitialized)
        //     {
        //         arrowGenerator.Initialize(arrowPooler, audioSource, arrowSpawnOffset);
        //         isInitialized = true;
        //     }
        //     beatDetector.AnalyzeTrackAsync(clip);
        // }
        //
        // private void HandleAudioPlay(double startDspTime)
        // {
        //     audioStartDspTime = startDspTime;
        //     
        //     if (isGameReady && currentBeatmap != null)
        //     {
        //         StartGameplay();
        //     }
        // }
        //
        // private void HandleBeatmapDetected(Beatmap beatmap)
        // {
        //     currentBeatmap = beatmap;
        //     isGameReady = true;
        //
        //     if (audioSource.isPlaying)
        //     {
        //         StartGameplay();
        //     }
        // }
        //
        // private void HandleAnalysisFailed(string error)
        // {
        //     Debug.LogError($"Beatmap analysis failed: {error}");
        //     // Fallback: use default beatmap or retry
        // }
        //
        // private void StartGameplay()
        // {
        //     List<float> beatsToUse = GetBeatsForDifficulty();
        //     arrowGenerator.StartGenerating(beatsToUse);
        // }
        //
        // private List<float> GetBeatsForDifficulty()
        // {
        //     if (currentBeatmap == null) return new List<float>();
        //
        //     return difficulty switch
        //     {
        //         DifficultyLevel.Easy => currentBeatmap.Easy,
        //         DifficultyLevel.Medium => currentBeatmap.Medium,
        //         DifficultyLevel.Hard => currentBeatmap.Hard,
        //         _ => currentBeatmap.Medium
        //     };
        // }
        //
        // private void OnDestroy()
        // {
        //     UnregisterEvents();
        // }
        //
        // private void UnregisterEvents()
        // {
        //     if (audioManager != null)
        //     {
        //         audioManager.OnAudioLoaded -= HandleAudioLoaded;
        //         audioManager.OnAudioPlay -= HandleAudioPlay;
        //     }
        //
        //     if (beatDetector != null)
        //     {
        //         beatDetector.OnBeatmapDetected -= HandleBeatmapDetected;
        //     }
        // }
    }
}