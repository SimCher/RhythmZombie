using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.UI.Assets.Scripts
{
    public class BeatVisualizer : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = 10f;

        private Image _renderer;
        private Color _targetColor;
        private Color _currentColor;

        private void Awake()
        {
            _renderer = GetComponent<Image>();
            _targetColor = Color.white;
            _currentColor = Color.white;
            _renderer.color = Color.white;
        }

        private void Update()
        {
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * fadeSpeed);
            _renderer.color = _currentColor;
        }

        private void SetColor(Color color)
        {
            _targetColor = color;
            _currentColor = color;
        }

        public void OnBeat()
        {
            SetColor(Color.yellow);
        }

        public void OnHit(bool success)
        {
            SetColor(success ? Color.green : Color.red);
        }

        public void OnMiss()
        {
            SetColor(Color.red);
        }

        public void ResetIndicator()
            => SetColor(Color.white);
    }
}
