using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.Scripts.Objects.AI.Zombie
{
    public class ZombieController : MonoBehaviour
    {
        [SerializeField, Range(0, 1f)] private float mood = 1f;
        [SerializeField] private Image moodSlider;

        private void Start()
        {
            moodSlider = GetComponentInChildren<Image>();

            if (moodSlider)
                moodSlider.fillAmount = mood;
        }

        private void UpdateAppearance()
        {
            if (moodSlider)
                moodSlider.fillAmount = mood;
            
            moodSlider.color = Color.Lerp(Color.red, Color.green, mood);
        }

        public void ChangeMood(float delta)
        {
            mood = Mathf.Clamp01(mood + delta);
            UpdateAppearance();

            if (mood <= 0)
            {
                Debug.Log($"Зомби {name} нападает!");
                //TODO: триггер GameOver
            }
        }
    }
}