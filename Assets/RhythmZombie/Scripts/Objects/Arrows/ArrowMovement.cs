using RhythmZombie.Scripts.Objects.Poolers;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Objects.Arrows
{
    public class ArrowMovement : MonoBehaviour
    {
        private float _targetTime;

        private double _audioStartDspTime;

        private IObjectPooler _pooler;

        private RectTransform _rectTransform;

        private ArrowDirection _direction;

        private float _laneWidth;

        private float _spawnOffset;

        public ArrowDirection Direction => _direction;

        private void Update()
        {
            var currentTime = (float) (AudioSettings.dspTime - _audioStartDspTime);
            var timeUntilTarget = _targetTime - currentTime;

            var t = 1f - (timeUntilTarget / _spawnOffset);
            var x = Mathf.Lerp(_laneWidth / 2f, -_laneWidth / 2f, t);
            _rectTransform.localPosition = new Vector3(x, 0, 0);

            if (Mathf.Abs(_rectTransform.localPosition.x) < 1f && Mathf.Abs(timeUntilTarget) < 0.01f)
            {
                Debug.Log(
                    $"Стрелка в центре: Время={currentTime:F3}, Цель={_targetTime:F3}, Ошибка={timeUntilTarget * 1000:F1}ms");
            }

            if (_rectTransform.localPosition.x < -_laneWidth / 2f)
            {
                _pooler.ReturnObject(gameObject);
            }
        }

        public void Initialize(float targetTime, double audioStartDspTime, float spawnOffset, IObjectPooler pooler,
            float laneWidth, ArrowDirection direction = ArrowDirection.Up)
        {
            _targetTime = targetTime;
            _audioStartDspTime = audioStartDspTime;
            _spawnOffset = spawnOffset;
            _pooler = pooler;
            _laneWidth = laneWidth;
            _direction = direction;

            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.localPosition = new Vector3(_laneWidth / 2f, 0, 0);
            gameObject.SetActive(true);

            _rectTransform.localRotation = direction switch
            {
                ArrowDirection.Up => Quaternion.Euler(0, 0, 0),
                ArrowDirection.Left => Quaternion.Euler(0, 0, 90),
                ArrowDirection.Down => Quaternion.Euler(0,0, 180),
                ArrowDirection.Right => Quaternion.Euler(0,0,-90),
                _ => _rectTransform.localRotation
            };
        }
        // private float _targetTime;
        // private float _speed;
        // private IObjectPooler _arrowPooler;
        // private RectTransform _rectTransform;
        // private ArrowDirection _direction;
        //
        // public ArrowDirection Direction => _direction;
        // public IObjectPooler Pooler => _arrowPooler;
        //
        // private void Update()
        // {
        //     _rectTransform.localPosition -= new Vector3(_speed * Time.deltaTime, 0, 0);
        //     var leftBoundary = -(Pooler as ArrowPooler).Lane.rect.width / 2f;
        //     if(_rectTransform.localPosition.x < leftBoundary)
        //         _arrowPooler.ReturnObject(gameObject);
        // }
        //
        // public void Initialize(float time, float moveSpeed, IObjectPooler pooler,
        //     ArrowDirection direction = ArrowDirection.Up)
        // {
        //     _targetTime = time;
        //     _speed = moveSpeed;
        //     _arrowPooler = pooler;
        //     _direction = direction;
        //
        //     _rectTransform = GetComponent<RectTransform>();
        //     var startX = (pooler as ArrowPooler).Lane.rect.width / 2f;
        //     _rectTransform.anchoredPosition = new Vector2(startX, 0);
        //
        //     _rectTransform.anchorMin = new Vector2(0, 0.5f);
        //     _rectTransform.anchorMax = new Vector2(0, 0.5f);
        //     gameObject.SetActive(true);
        //
        //     _rectTransform.localRotation = direction switch
        //     {
        //         ArrowDirection.Up => Quaternion.Euler(0, 0, 0),
        //         ArrowDirection.Left => Quaternion.Euler(0, 0, 90),
        //         ArrowDirection.Down => Quaternion.Euler(0,0, 180),
        //         ArrowDirection.Right => Quaternion.Euler(0,0,-90),
        //         _ => _rectTransform.localRotation
        //     };
        // }
    }
}