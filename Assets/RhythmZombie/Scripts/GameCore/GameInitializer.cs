using RhythmZombie.Scripts.Audio;
using RhythmZombie.Scripts.Audio.Loaders;
using RhythmZombie.Scripts.Managers;
using RhythmZombie.Scripts.Objects.Arrows;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.GameCore
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private BeatmapManager beatmapManager;
        [SerializeField] private ArrowSpawner arrowSpawner;
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private SongTimer songTimer;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        private AudioClip _loadedClip;
        private string _loadedPath;

        private void Start()
        {
            audioManager.OnTrackLoaded += OnAudioReady;
            audioManager.LoadTrack();

            uiManager.OnReadyBtnClicked += OnReadyBtnClicked;
        }

        private void OnAudioReady(AudioClip clip, string path)
        {
            _loadedClip = clip;
            _loadedPath = path;
            // songTimer.SetAudioClip(clip);
            // beatmapManager.LoadBeatmap(path);
            // arrowSpawner.PrepareSpawning();
            // songTimer.Play();
        }

        private void OnReadyBtnClicked()
            => uiManager.StartCountdown(StartGame);

        private void StartGame()
        {
            songTimer.SetAudioClip(_loadedClip);
            beatmapManager.LoadBeatmap(_loadedPath);
            arrowSpawner.PrepareSpawning();
            songTimer.Play();
            
            uiManager.SwitchToGameCanvas();
        }
    }
}