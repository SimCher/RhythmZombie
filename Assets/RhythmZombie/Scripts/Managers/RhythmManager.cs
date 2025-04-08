using System.Collections.Generic;
using RhythmZombie.Scripts.Analyzers;
using RhythmZombie.UI.Assets.Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmZombie.Scripts.Managers
{
    [RequireComponent(typeof(AudioSource))]
    public class RhythmManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float hitWindow = 0.1f;
    [SerializeField] private KeyCode inputKey = KeyCode.Space;
    
    [Header("Компоненты")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private BeatVisualizer visualizer;
    [SerializeField] private AudioAnalyzer analyzer;
    
    [Header("События")]
    public UnityEvent<string> HitFeedback;
    public UnityEvent<float> OnBPMDetected;
    public UnityEvent<bool> OnAnalysisComplete;

    private List<float> _beatTimings = new List<float>();
    private int _nextBeatIndex;
    private float _detectedBPM;
    private bool _isInitialized;
    private Coroutine _analysisRoutine;

    private void Start()
    {
        if (!audioSource) TryGetComponent(out audioSource);
        InitializeRhythm();
    }

    public void InitializeRhythm()
    {
        if (_analysisRoutine != null)
            StopCoroutine(_analysisRoutine);
        
        // _analysisRoutine = StartCoroutine(AnalyzeAndInitialize());
    }

    // private IEnumerator AnalyzeAndInitialize()
    // {
    //     _isInitialized = false;
    //     OnAnalysisComplete?.Invoke(false);
    //
    //     // Получаем аудиоклип из анализатора
    //     // var track = analyzer.GetAudioClip();
    //     // if (track == null)
    //     // {
    //     //     Debug.LogError("Аудиоклип не найден в анализаторе");
    //     //     yield break;
    //     // }
    //     //
    //     // // Загружаем аудиоданные
    //     // float[] samples = new float[track.samples * track.channels];
    //     // track.GetData(samples, 0);
    //     //
    //     // // Анализируем ритм
    //     // var rawBeats = analyzer.DetectBeats(samples, track.frequency);
    //     // _detectedBPM = analyzer.CalculateBPM(rawBeats);
    //     // OnBPMDetected?.Invoke(_detectedBPM);
    //     //
    //     // // Фильтруем биты
    //     // _beatTimings = FilterBeats(rawBeats, _detectedBPM);
    //     // _nextBeatIndex = 0;
    //     //
    //     // // Настраиваем аудиоисточник
    //     // audioSource.clip = track;
    //     // _isInitialized = true;
    //     // OnAnalysisComplete?.Invoke(true);
    //     //
    //     // Debug.Log($"Ритм инициализирован. BPM: {_detectedBPM}, Битов: {_beatTimings.Count}");
    // }

    private List<float> FilterBeats(List<float> rawBeats, float bpm)
    {
        List<float> filtered = new List<float>();
        if (rawBeats.Count == 0) return filtered;

        float beatInterval = 60f / bpm;
        float tolerance = beatInterval * 0.3f;

        filtered.Add(rawBeats[0]);

        for (int i = 1; i < rawBeats.Count; i++)
        {
            float timeSinceLast = rawBeats[i] - rawBeats[i-1];
            
            if (Mathf.Abs(timeSinceLast - beatInterval) <= tolerance || 
                Mathf.Abs(timeSinceLast - beatInterval/2) <= tolerance)
            {
                filtered.Add(rawBeats[i]);
            }
        }

        return filtered;
    }

    private void Update()
    {
        if (!_isInitialized || !audioSource.isPlaying) return;
        
        float currentTime = audioSource.time;
        CheckForMissedBeats(currentTime);
        
        if (Input.GetKeyDown(inputKey))
        {
            CheckHit(currentTime);
        }
    }

    private void CheckForMissedBeats(float currentTime)
    {
        while (_nextBeatIndex < _beatTimings.Count && 
               currentTime > _beatTimings[_nextBeatIndex] + hitWindow)
        {
            HitFeedback?.Invoke("Пропущен");
            visualizer.OnMiss();
            _nextBeatIndex++;
        }
    }

    private void CheckHit(float currentTime)
    {
        if (_nextBeatIndex >= _beatTimings.Count) return;

        // Находим ближайший бит
        int closestIndex = -1;
        float closestDiff = float.MaxValue;
        
        for (int i = Mathf.Max(0, _nextBeatIndex - 1); 
             i < _beatTimings.Count && _beatTimings[i] <= currentTime + hitWindow; 
             i++)
        {
            float diff = Mathf.Abs(currentTime - _beatTimings[i]);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                closestIndex = i;
            }
        }

        if (closestIndex >= 0 && closestDiff <= hitWindow)
        {
            string result = closestDiff < hitWindow * 0.3f ? "Идеально!" : "Хорошо";
            HitFeedback?.Invoke(result);
            visualizer.OnHit(closestDiff < hitWindow * 0.3f);
            _nextBeatIndex = closestIndex + 1;
        }
        else
        {
            HitFeedback?.Invoke("Не попал");
            visualizer.OnHit(false);
        }
    }

    public void Play()
    {
        if (_isInitialized) audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        _nextBeatIndex = 0;
    }
}
}