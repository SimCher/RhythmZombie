using System;

namespace RhythmZombie.Scripts.Utilities.Events
{
    public static class EventManager
    {
        public static event Action<int> OnNoteHit;

        public static void TriggerNoteHit(int section)
        {
            OnNoteHit?.Invoke(section);
        }
    }
}