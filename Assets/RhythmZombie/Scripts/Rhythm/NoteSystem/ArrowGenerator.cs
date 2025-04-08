using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using RhythmZombie.Scripts.Objects.Poolers;

public class ArrowGenerator : MonoBehaviour
{
    private IObjectPooler pooler;
    private AudioSource audioSource;
    private double audioStartDspTime;
    private List<float> beats;
    private float lineWidth;
    private float spawnOffset;

    public void Initialize(IObjectPooler pooler, AudioSource audioSource, double audioStartDspTime)
    {
        this.pooler = pooler;
        this.audioSource = audioSource;
        this.audioStartDspTime = audioStartDspTime;
        CalculateOffsets();
    }

    private void CalculateOffsets()
    {
        lineWidth = 1920; // Примерное значение
        spawnOffset = lineWidth / 2000f; // Примерное значение (0.96 сек)
        Debug.Log($"Ширина линии: {lineWidth}, SpawnOffset: {spawnOffset} сек");
    }

    public void StartGenerating(List<float> beats)
    {
        this.beats = beats;
        Debug.Log($"StartGenerating: {beats.Count} битов, Первый бит: {beats[0]}, Второй бит: {(beats.Count > 1 ? beats[1] : 0)}");
        StartCoroutine(PreWarmArrows());
    }

    private IEnumerator PreWarmArrows()
    {
        double currentTrackTime = AudioSettings.dspTime - audioStartDspTime;

        foreach (float beatTime in beats)
        {
            // Время, когда стрелка должна появиться на экране (относительно начала трека)
            float targetTime = beatTime - spawnOffset;
            if (targetTime < 0) continue; // Пропускаем биты до начала трека

            // Время до спавна стрелки
            float timeUntilSpawn = targetTime - (float)currentTrackTime;

            if (timeUntilSpawn <= 0)
            {
                // Если момент уже прошёл, спавним сразу
                Debug.Log($"Стрелка спавнится немедленно, Цель: {beatTime:F3} сек");
                SpawnArrow(beatTime);
            }
            else
            {
                // Если момент в будущем, ждём
                Debug.Log($"Стрелка спавнится через {timeUntilSpawn:F3} сек, Цель: {beatTime:F3} сек");
                yield return new WaitForSeconds(timeUntilSpawn);
                SpawnArrow(beatTime);
            }

            // Обновляем текущее время трека
            currentTrackTime = AudioSettings.dspTime - audioStartDspTime;
        }
        Debug.Log("Предзагрузка выполнена.");
    }

    private void SpawnArrow(float beatTime)
    {
        // Логика спавна стрелки (заглушка)
    }
}