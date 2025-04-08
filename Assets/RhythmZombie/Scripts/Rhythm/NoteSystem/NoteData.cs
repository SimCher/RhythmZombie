using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    [CreateAssetMenu(fileName = "NoteData", menuName = "RhythmZombo/Note Data")]
    public class NoteData : ScriptableObject
    {
        public KeyCode[] inputKeys;
        public GameObject notePrefab;
    }
}