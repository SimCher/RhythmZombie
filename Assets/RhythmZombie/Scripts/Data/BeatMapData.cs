using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RhythmZombie.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewBeatMap", menuName = "RhythmZombo/BeatMap")]
    public class BeatMapData : ScriptableObject
    {
        public AudioClip track;
        
        public List<BeatSegment> segments = new ();

        public AudioClip Track => track;
        public List<BeatSegment> Segments => segments;

        [System.Serializable]
        public class BeatSegment
        {
            public float startTime;
            public float bpm;
            public List<float> beatOffsets;

            public List<float> GetAbsoluteBeats() 
                => beatOffsets.Select(offset => startTime + offset).ToList();
        }
    }
}
