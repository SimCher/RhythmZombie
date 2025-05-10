namespace RhythmZombie.Scripts.Data
{
    [System.Serializable]
    public class AudioAnalysisResult
    {
        public float[] peaks;
        public float bpm;
        public float duration;
        public string error;
    }
}