using System;
using RhythmZombie.Scripts.Audio.Analysis;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RhythmZombie.Scripts.Rhythm.NoteSystem
{
    [RequireComponent(typeof(IMusicAnalyzer))]
    public class NoteManager : MonoBehaviour
    {
        [SerializeField] private NoteData noteData;
        [SerializeField] private Transform[] spawnPoints;

        private IMusicAnalyzer _musicAnalyzer;

        private void Start()
        {
            _musicAnalyzer = GetComponent<IMusicAnalyzer>();
            _musicAnalyzer.OnBeat += SpawnNote;
        }

        private void SpawnNote(float time)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("SpawnPoints не назначен или пуст!");
                return;
            }

            if (noteData.inputKeys.Length < spawnPoints.Length)
            {
                Debug.LogError(
                    $"Недостаточно кнопок ввода! Нужно {spawnPoints.Length}, но получено {noteData.inputKeys.Length}");
                return;
            }
            
            Debug.Log($"Спавн ноты. Спавн точки: {spawnPoints?.Length}. Клавиши: {noteData.inputKeys?.Length}");
            
            var section = Random.Range(0, spawnPoints.Length);
            var noteObj = Instantiate(noteData.notePrefab, spawnPoints[section]);

            var note = noteObj.GetComponent<Note>();
            if (note != null)
                note.Initialize(noteData.inputKeys[section], section);
            else
            {
                Debug.LogError("Note prefab is missing Note component!");
            }
        }

        private void OnDestroy()
        {
            if (_musicAnalyzer != null)
                _musicAnalyzer.OnBeat -= SpawnNote;
        }
    }
}