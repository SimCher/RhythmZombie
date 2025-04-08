using System;
using RhythmZombie.Scripts.Utilities.Events;
using UnityEngine;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    public class Note : MonoBehaviour
    {
        private KeyCode _targetKey;
        private int _sectionIndex;

        private void Start()
        {
            transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one, 0.2f);
        }

        public void Initialize(KeyCode key, int section)
        {
            _targetKey = key;
            _sectionIndex = section;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_targetKey))
            {
                EventManager.TriggerNoteHit(_sectionIndex);
                Destroy(gameObject);
            }
        }

        public void Hit()
        {
            LeanTween.scale(gameObject, Vector3.zero, 0.1f)
                .setOnComplete(() => Destroy(gameObject));
        }
    }
}