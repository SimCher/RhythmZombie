using System.Collections.Generic;
using UnityEngine;

namespace RhythmZombie.Scripts.Objects.NPC
{
    /// <summary>
    /// Управляет синхронизацией анимаций танцующего объекта с BPM трека.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DanceAnimatorController : MonoBehaviour
    {
        [Header("Настройки танца")]
        [SerializeField] private float crossfadeDuration = 0.1f;
        [SerializeField] private int beatsPerDance = 4;
        [SerializeField] private bool autoStart = true;
        [SerializeField, Min(60f)] private float currentBpm = 120f;

        private Animator _animator;
        private Dictionary<string, AnimationClip> _danceStates = new();
        private List<string> _stateNames = new();
        
        private int _currentDanceIndex;
        private double _nextSwitchTime;
        private bool _isDancing;

        private double BeatInterval => 60.0 / currentBpm;
        private double RequiredDanceDuration => beatsPerDance * BeatInterval;
        
        public float BPM
        {
            get => currentBpm;
            set
            {
                currentBpm = Mathf.Max(value, 60f);
                UpdateNextSwitchTime();
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            InitializeDanceStates();
        }

        private void Start()
        {
            if (autoStart && _stateNames.Count > 0)
                StartDancing(currentBpm);
        }

        private void Update()
        {
            if (!_isDancing || _stateNames.Count == 0)
                return;

            if (AudioSettings.dspTime >= _nextSwitchTime)
            {
                PlayNextDanceState();
            }
        }

        private void InitializeDanceStates()
        {
            var controller = _animator.runtimeAnimatorController;
            if (!controller)
            {
                Debug.LogError("Нет назначенного AnimatorContoller.");
                return;
            }

            foreach (var clip in controller.animationClips)
            {
                var clipName = clip.name;
                if (!clipName.StartsWith("dance_"))
                    continue;

                if (_danceStates.TryAdd(clipName, clip))
                {
                    _stateNames.Add(clipName);
                }
            }
        }

        private void UpdateNextSwitchTime()
        {
            if (_stateNames.Count == 0)
                return;

            var currentStateName = _stateNames[_currentDanceIndex];
            if (!_danceStates.TryGetValue(currentStateName, out var clip) || !clip)
            {
                Debug.LogWarning($"Не найден клип для стейта {currentStateName}");
                _nextSwitchTime = AudioSettings.dspTime + 1.0;
                return;
            }

            double clipLengthInSecs = clip.length;
            var clipLengthInBeats = clipLengthInSecs / (60.0 / currentBpm);

            if (clipLengthInBeats < 1.0)
                clipLengthInBeats = 1.0;

            _nextSwitchTime = AudioSettings.dspTime + clipLengthInSecs;
        }

        private void PlayNextDanceState()
        {
            if (_stateNames.Count == 0)
                return;

            var nextStateName = _stateNames[_currentDanceIndex];
            if (!_danceStates.TryGetValue(nextStateName, out var clip))
            {
                Debug.LogWarning($"Клип {nextStateName} не найден.");
                return;
            }

            var clipLength = clip.length;
            var requiredDuration = RequiredDanceDuration;

            var adjustedSpeed = (float) (clipLength / requiredDuration / 4f);
            _animator.speed = adjustedSpeed;
            
            _animator.CrossFade(nextStateName, crossfadeDuration, 0, 0f);

            _nextSwitchTime = AudioSettings.dspTime + requiredDuration;

            _currentDanceIndex = Random.Range(0, _stateNames.Count);
        }

        public void StartDancing(float bpm)
        {
            if (_stateNames.Count == 0)
            {
                Debug.LogWarning("Нет доступных танцевальных стейтов для проигрывания!");
                return;
            }

            BPM = bpm;

            _isDancing = true;
            _currentDanceIndex = 0;
            PlayNextDanceState();
        }

        public void StopDancing()
        {
            _isDancing = false;
            _animator.speed = 1f;
        }
    }
}