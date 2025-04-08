using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    [CreateAssetMenu(menuName = "Audio/Latency Settings")]
    public class LatencySettings : ScriptableObject
    {
        [Header("Платформенные настройки")]
        public float PC = 0.03f;

        public float Mobile = 0.07f;
        public float WebGL = 0.12f;

        public float GetPlatformLatency()
        {
#if UNITY_WEBGL
            return WebGL;
#elif UNITY_IOS || UNITY_ANDROID
            return Mobile;
#else
            return PC;
#endif
        }
    }
}