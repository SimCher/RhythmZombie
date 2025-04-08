using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    public class Arrow : MonoBehaviour
    {
        public float speed = 5f;
        public RectTransform targetZone;

        private int _direction;
        private bool _isActive = true;
        
        [SerializeField] private RectTransform rectTransform;

        private void Update()
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                targetZone.anchoredPosition,
                speed * Time.deltaTime
            );

            if (_isActive)
            {
                if (CheckInput() && IsInHitZone())
                {
                    var distance = Vector2.Distance(rectTransform.anchoredPosition, targetZone.anchoredPosition);
                    var result = GetHitResult(distance);
                    Debug.Log(result);
                    Destroy(gameObject);
                }
            }

            if (Vector2.Distance(rectTransform.anchoredPosition, targetZone.anchoredPosition) < 10f && !_isActive)
            {
                Debug.Log("Мимо");
                Destroy(gameObject);
            }
                
        }

        private string GetHitResult(float distance)
        {
            return distance switch
            {
                < 10f => "Превосходно",
                < 25f => "Мастер",
                < 50f => "Хорошо",
                _ => "Мимо"
            };
        }

        private bool CheckInput()
            => (_direction == 0 && Input.GetKeyDown(KeyCode.UpArrow)) ||
               (_direction == 1 && Input.GetKeyDown(KeyCode.DownArrow)) ||
               (_direction == 2 && Input.GetKeyDown(KeyCode.LeftArrow)) ||
               (_direction == 3 && Input.GetKeyDown(KeyCode.RightArrow));

        private bool IsInHitZone()
        {
            var distance = Vector2.Distance(rectTransform.anchoredPosition, targetZone.anchoredPosition);
            return distance < 50f;
        }

        public void SetDirection(int dir)
        {
            _direction = dir;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, _direction * 90f);
        }
    }
}