using RhythmZombie.Scripts.Objects.Poolers;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RhythmZombie.Scripts.Objects.Arrows
{
    public class ArrowBehaviour : MonoBehaviour
    {
        [SerializeField] private ArrowDirection direction;
        
        private RectTransform _rectTransform;
        private float _speed;
        private Vector2 _targetPos;
        private float _beatTime;
        private ArrowPooler _pooler;
        private float _targetRadius;
        private float _arrowWidth;
        
        private void Update()
        {
            var currentPosX = _rectTransform.anchoredPosition.x;
            currentPosX -= _speed * Time.deltaTime;
            _rectTransform.anchoredPosition = new Vector2(currentPosX, _rectTransform.anchoredPosition.y);

            var arrowLeftEdge = currentPosX - (_arrowWidth / 2f);

            var despawnX = _targetPos.x - _targetRadius;
            if (arrowLeftEdge < despawnX)
            {
                Debug.Log(
                    $"Arrow despawned at local X={currentPosX}, leftEdge={arrowLeftEdge}, should be at {despawnX}");
                _pooler.ReturnObject(gameObject);
            }
        }

        private void SetRandomDirection()
        {
            var randomDirection = (ArrowDirection) Random.Range(0, 4);
            direction = randomDirection;
            
            _rectTransform.localRotation = randomDirection switch
            {
                ArrowDirection.Up => Quaternion.Euler(0, 0, 0),
                ArrowDirection.Left => Quaternion.Euler(0, 0, 90),
                ArrowDirection.Down => Quaternion.Euler(0,0, 180),
                ArrowDirection.Right => Quaternion.Euler(0,0,-90),
                _ => _rectTransform.localRotation
            };
        }

        public void Initialize(float beatTime, float speed, RectTransform targetZone,Vector2 spawnPos, ArrowPooler pool)
        {
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            _beatTime = beatTime;
            _speed = speed;
            _targetPos = targetZone.anchoredPosition;
            _pooler = pool;

            _targetRadius = targetZone.sizeDelta.x / 2f;
            _arrowWidth = _rectTransform.sizeDelta.x;

            _rectTransform.anchoredPosition = spawnPos;
            
            SetRandomDirection();

            Debug.Log(
                $"Стрелка проинициализирована: targetPos = {_targetPos}, targetRadius = {_targetRadius}, arrowWidth = {_arrowWidth}, spawnPos = {spawnPos}");
        }
    }
}