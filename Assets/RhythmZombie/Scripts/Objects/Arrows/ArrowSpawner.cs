using System.Collections;
using System.Collections.Generic;
using RhythmZombie.Scripts.Audio;
using RhythmZombie.Scripts.GameCore;
using RhythmZombie.Scripts.Objects.Poolers;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Objects.Arrows
{
    public class ArrowSpawner : MonoBehaviour
    {
        [SerializeField] private ArrowPooler pooler;
        [SerializeField] private RectTransform targetZone;
        [SerializeField] private RectTransform spawnPoint;
        [SerializeField] private RectTransform lane;

        [SerializeField] private BeatmapManager beatmapManager;
        [SerializeField] private DifficultyManager difficultyManager;
        [SerializeField] private SongTimer songTimer;

        private float _lastSpawnTime = -Mathf.Infinity;
        
        private float MinInterval
        {
            get
            {
                var speed = difficultyManager.Speed;
                var arrowWidth = pooler.ArrowWidth;
                var minInterval = arrowWidth / speed;

                return minInterval * difficultyManager.SpacingMultiplier;
            }
        }

        private void PrecheckBeats()
        {
            var queue = beatmapManager.CurrentBeats;
            if (queue == null || queue.Count == 0) return;

            var speed = difficultyManager.Speed;
            var travelDistance = spawnPoint.anchoredPosition.x - targetZone.anchoredPosition.x;
            var travelTime = travelDistance / speed;

            var tempQueue = new Queue<float>(queue);
            while (tempQueue.Count > 0)
            {
                var nextBeat = tempQueue.Peek();
                var spawnTime = nextBeat - travelTime;

                if (spawnTime <= 0)
                {
                    var arrow = SpawnArrow(nextBeat, speed);
                    arrow.gameObject.SetActive(false);
                    queue.Dequeue();
                    tempQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (!songTimer.IsPlaying) yield return null;

                var currentTime = songTimer.CurrentTime;
                var queue = beatmapManager.CurrentBeats;
                if (queue.Count > 0)
                {
                    var nextBeat = queue.Peek();
                    var speed = difficultyManager.Speed;
                    var travelDistance = spawnPoint.anchoredPosition.x - targetZone.anchoredPosition.x;
                    var travelTime = travelDistance / speed;
                    var spawnTime = nextBeat - travelTime;

                    if (currentTime >= spawnTime)
                    {
                        if (currentTime - _lastSpawnTime >= MinInterval)
                        {
                            SpawnArrow(nextBeat, speed);
                            _lastSpawnTime = currentTime;
                            queue.Dequeue();
                        }
                        else
                        {
                            queue.Dequeue();
                        }
                    }
                }

                yield return null;
            }
        }

        private GameObject SpawnArrow(float beatTime, float speed)
        {
            var arrow = pooler.NextPooled;
            var arrowBehaviour = arrow.GetComponent<ArrowBehaviour>();
            var spawnPos = new Vector2(spawnPoint.anchoredPosition.x, lane.anchoredPosition.y);
            arrowBehaviour.Initialize(beatTime, speed, targetZone, spawnPos, pooler);

            return arrow;
        }

        public void PrepareSpawning()
        {
            PrecheckBeats();
            StartCoroutine(SpawnRoutine());
        }
    }
}