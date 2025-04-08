using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using RhythmZombie.Scripts.Rhythm.NoteSystem;

public class BeatDetector : MonoBehaviour
{
    [Header("BPM Detection Settings")]
    [SerializeField, Tooltip("Минимальный BPM для анализа")]
    private float minBPM = 60f;
    [SerializeField, Tooltip("Максимальный BPM для анализа")]
    private float maxBPM = 200f;
    [SerializeField, Tooltip("Длительность анализа для BPM (сек)")]
    private float bpmAnalysisDuration = 10f; // Увеличено до 10 секунд
    [SerializeField, Tooltip("Ручное указание BPM (0 = автоопределение)")]
    private float overrideBPM = 0f;

    [Header("Onset Detection Settings")]
    [SerializeField, Tooltip("Порог спектрального потока для онсетов")]
    private float onsetThreshold = 0.005f; // Снижено до 0.005
    [SerializeField, Tooltip("Минимальный интервал между онсетами (сек)")]
    private float minOnsetInterval = 0.1f;

    [Header("Debug Settings")]
    [SerializeField, Tooltip("Включить подробное логирование")]
    private bool enableDebugLogs = true;

    private const int DOWNSAMPLE_RATE = 11025;
    private const float BPM_WEIGHT_BOOST_RANGE_MIN = 140f;
    private const float BPM_WEIGHT_BOOST_RANGE_MAX = 160f;
    private const float BPM_WEIGHT_BOOST_FACTOR = 1.5f;
    private const float LOW_PASS_CUTOFF = 200f; // Частота среза для низких частот (200 Гц)

    public delegate void BeatmapDetectedHandler(Beatmap beatmap);
    public event BeatmapDetectedHandler OnBeatmapDetected;

    public delegate void ProgressUpdatedHandler(float progress);
    public event ProgressUpdatedHandler OnProgressUpdated;

    private float startTime;

    public void AnalyzeTrackAsync(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip не передан в BeatDetector!");
            OnBeatmapDetected?.Invoke(null);
            return;
        }

        StartCoroutine(AnalyzeTrackCoroutine(clip));
    }

    private IEnumerator AnalyzeTrackCoroutine(AudioClip clip)
    {
        startTime = Time.realtimeSinceStartup;
        Log("Начинается анализ трека...");

        // Этап 1: Загрузка и даунсэмплинг сэмплов
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        Log($"Сэмплы загружены: {samples.Length}, Частота: {clip.frequency} Гц");

        float[] monoSamples = ConvertToMono(samples, clip.channels);
        samples = null;
        Log($"Конвертировано в моно: {monoSamples.Length} сэмплов");

        float[] downsampled = Downsample(monoSamples, clip.frequency, DOWNSAMPLE_RATE);
        int downsampledRate = DOWNSAMPLE_RATE;
        Log($"Даунсэмплинг завершён: {downsampled.Length} сэмплов, Частота: {downsampledRate} Гц");
        OnProgressUpdated?.Invoke(0.2f);
        yield return null;

        // Применяем фильтр низких частот
        float[] filtered = ApplyLowPassFilter(downsampled, downsampledRate, LOW_PASS_CUTOFF);
        Log("Фильтр низких частот применён");

        // Этап 2: Определение BPM
        float bpm;
        if (overrideBPM > 0)
        {
            bpm = overrideBPM;
            Log($"Используется ручной BPM: {bpm}");
        }
        else
        {
            Log("Определение BPM...");

            // Вычисляем энергию сигнала
            int bpmWindowSize = downsampledRate / 20; // Окно 50 мс для BPM
            int analysisSamples = (int)(downsampledRate * bpmAnalysisDuration);
            if (analysisSamples > filtered.Length) analysisSamples = filtered.Length;
            float[] energy = new float[analysisSamples / bpmWindowSize];
            for (int i = 0; i < energy.Length; i++)
            {
                energy[i] = CalculateEnergy(filtered, i * bpmWindowSize, bpmWindowSize);
            }
            Log($"Энергия сигнала вычислена: {energy.Length} окон");

            // Нормализация энергии
            float maxEnergy = 0f;
            for (int i = 0; i < energy.Length; i++)
            {
                if (energy[i] > maxEnergy) maxEnergy = energy[i];
            }
            if (maxEnergy > 0)
            {
                for (int i = 0; i < energy.Length; i++)
                {
                    energy[i] /= maxEnergy;
                }
            }

            // Автокорреляция
            int maxLag = (int)(downsampledRate * 60f / minBPM); // Максимальный лаг (для 60 BPM)
            int minLag = (int)(downsampledRate * 60f / maxBPM); // Минимальный лаг (для 200 BPM)
            float[] autocorrelation = new float[maxLag];
            float maxAutocorrelation = 0f;
            for (int lag = minLag; lag < maxLag; lag++)
            {
                float sum = 0f;
                for (int i = 0; i < energy.Length - lag; i++)
                {
                    sum += energy[i] * energy[i + lag];
                }
                autocorrelation[lag] = sum;
                if (sum > maxAutocorrelation) maxAutocorrelation = sum;
            }

            // Нормализация автокорреляции
            if (maxAutocorrelation > 0)
            {
                for (int lag = minLag; lag < maxLag; lag++)
                {
                    autocorrelation[lag] /= maxAutocorrelation;
                }
            }
            Log("Автокорреляция завершена");

            // Ищем пики
            Dictionary<float, float> bpmScores = new Dictionary<float, float>();
            for (int lag = minLag; lag < maxLag; lag++)
            {
                // Условие пика: значение выше порога 0.2
                if (autocorrelation[lag] > 0.2f)
                {
                    float interval = lag * bpmWindowSize / (float)downsampledRate; // Интервал в секундах
                    float bpmCandidate = 60f / interval;
                    if (bpmCandidate >= minBPM && bpmCandidate <= maxBPM)
                    {
                        if (!bpmScores.ContainsKey(bpmCandidate))
                            bpmScores[bpmCandidate] = 0f;
                        bpmScores[bpmCandidate] += autocorrelation[lag];
                        Log($"Кандидат BPM: {bpmCandidate:F2}, Сила: {autocorrelation[lag]:F2}");
                    }
                }
            }

            // Выбираем лучший BPM
            float bestBPM = 0f;
            float maxScore = 0f;
            foreach (var pair in bpmScores)
            {
                float bpmCandidate = pair.Key;
                float score = pair.Value;

                if (bpmCandidate >= BPM_WEIGHT_BOOST_RANGE_MIN && bpmCandidate <= BPM_WEIGHT_BOOST_RANGE_MAX)
                    score *= BPM_WEIGHT_BOOST_FACTOR;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestBPM = bpmCandidate;
                }
            }

            bpm = bestBPM;
            if (bestBPM < 100)
            {
                float doubledBPM = bestBPM * 2;
                if (doubledBPM >= minBPM && doubledBPM <= maxBPM)
                {
                    bpm = doubledBPM;
                    Log($"BPM удвоен: {bestBPM} -> {bpm}");
                }
            }
            else if (bestBPM > 160)
            {
                float halvedBPM = bestBPM / 2;
                if (halvedBPM >= minBPM && halvedBPM <= maxBPM)
                {
                    bpm = halvedBPM;
                    Log($"BPM поделён: {bestBPM} -> {bpm}");
                }
            }

            if (bpm == 0)
            {
                bpm = 120; // Значение по умолчанию
                Log("BPM не определён, используется значение по умолчанию: 120");
            }

            Log($"Определённый BPM: {bpm}");
        }
        OnProgressUpdated?.Invoke(0.5f);
        yield return null;

        // Этап 3: Генерация битов
        Log("Генерация битов...");
        List<float> beats = GenerateBeats(bpm, clip.length);
        Log($"Сгенерировано битов: {beats.Count}");
        OnProgressUpdated?.Invoke(0.7f);
        yield return null;

        // Этап 4: Обнаружение онсетов
        Log("Обнаружение онсетов...");
        List<float> onsets = new List<float>();
        int onsetWindowSize = downsampledRate / 40; // Окно 25 мс для онсетов
        float lastOnsetTime = -minOnsetInterval;
        float[] spectralFlux = new float[downsampled.Length / onsetWindowSize];
        int totalWindows = spectralFlux.Length;
        int processedWindows = 0;

        for (int i = 0; i < downsampled.Length - onsetWindowSize; i += onsetWindowSize)
        {
            float energy = CalculateEnergy(downsampled, i * onsetWindowSize, onsetWindowSize);
            float prevEnergy = i == 0 ? 0 : CalculateEnergy(downsampled, i * onsetWindowSize - onsetWindowSize, onsetWindowSize);
            spectralFlux[i / onsetWindowSize] = Mathf.Max(0, energy - prevEnergy);

            processedWindows++;
            if (processedWindows % 100 == 0)
            {
                OnProgressUpdated?.Invoke(0.7f + 0.2f * processedWindows / totalWindows);
                yield return null;
            }
        }

        // Нормализация спектрального потока
        float maxFlux = 0f;
        for (int i = 0; i < spectralFlux.Length; i++)
        {
            if (spectralFlux[i] > maxFlux) maxFlux = spectralFlux[i];
        }
        if (maxFlux > 0)
        {
            for (int i = 0; i < spectralFlux.Length; i++)
            {
                spectralFlux[i] /= maxFlux;
            }
        }

        for (int i = 1; i < spectralFlux.Length - 1; i++)
        {
            float currentTime = (i * onsetWindowSize) / (float)downsampledRate;
            if (spectralFlux[i] > onsetThreshold && spectralFlux[i] > spectralFlux[i - 1] && spectralFlux[i] > spectralFlux[i + 1] && currentTime - lastOnsetTime >= minOnsetInterval)
            {
                onsets.Add(currentTime);
                lastOnsetTime = currentTime;
                Log($"Онсет обнаружен в {currentTime:F3} сек");
            }
        }

        downsampled = null;
        Log($"Обнаружено онсетов: {onsets.Count}");
        OnProgressUpdated?.Invoke(0.9f);
        yield return null;

        // Этап 5: Формирование уровней сложности
        Log("Формирование уровней сложности...");
        Beatmap beatmap = new Beatmap
        {
            Easy = GenerateLevel(beats, 2),
            Medium = beats,
            Hard = CombineBeatsAndOnsets(beats, onsets)
        };
        Log($"Beatmap готов: Easy={beatmap.Easy.Count}, Medium={beatmap.Medium.Count}, Hard={beatmap.Hard.Count}");
        OnProgressUpdated?.Invoke(1f);

        Log("Вызов OnBeatmapDetected...");
        OnBeatmapDetected?.Invoke(beatmap);
        Log($"Анализ завершён. Общее время: {(Time.realtimeSinceStartup - startTime):F2} сек");
    }

    private float[] ApplyLowPassFilter(float[] samples, int sampleRate, float cutoffFrequency)
    {
        float[] filtered = new float[samples.Length];
        float rc = 1.0f / (cutoffFrequency * 2 * Mathf.PI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (rc + dt);

        filtered[0] = samples[0];
        for (int i = 1; i < samples.Length; i++)
        {
            filtered[i] = filtered[i - 1] + alpha * (samples[i] - filtered[i - 1]);
        }
        return filtered;
    }

    private List<float> GenerateBeats(float bpm, float duration)
    {
        List<float> beats = new List<float>();
        float interval = 60f / bpm;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            beats.Add(currentTime);
            currentTime += interval;
        }

        return beats;
    }

    private List<float> GenerateLevel(List<float> beats, int step)
    {
        List<float> levelBeats = new List<float>();
        for (int i = 0; i < beats.Count; i += step)
        {
            levelBeats.Add(beats[i]);
        }
        return levelBeats;
    }

    private List<float> CombineBeatsAndOnsets(List<float> beats, List<float> onsets)
    {
        List<float> combined = new List<float>(beats);
        combined.AddRange(onsets);
        combined.Sort();

        List<float> filtered = new List<float> { combined[0] };
        for (int i = 1; i < combined.Count; i++)
        {
            if (combined[i] - filtered[filtered.Count - 1] >= minOnsetInterval)
            {
                filtered.Add(combined[i]);
            }
        }

        return filtered;
    }

    private float CalculateEnergy(float[] samples, int startIndex, int length)
    {
        float energy = 0f;
        int endIndex = Mathf.Min(startIndex + length, samples.Length);
        for (int i = startIndex; i < endIndex; i++)
        {
            energy += samples[i] * samples[i];
        }
        return energy / length;
    }

    private float[] ConvertToMono(float[] samples, int channels)
    {
        if (channels == 1) return samples;

        float[] mono = new float[samples.Length / channels];
        for (int i = 0; i < mono.Length; i++)
        {
            float sum = 0f;
            for (int c = 0; c < channels; c++)
            {
                sum += samples[i * channels + c];
            }
            mono[i] = sum / channels;
        }
        return mono;
    }

    private float[] Downsample(float[] samples, int originalRate, int targetRate)
    {
        int factor = originalRate / targetRate;
        if (factor <= 1) return samples;

        int newLength = samples.Length / factor;
        float[] downsampled = new float[newLength];
        for (int i = 0; i < newLength; i++)
        {
            downsampled[i] = samples[i * factor];
        }
        return downsampled;
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[BeatDetector] {message} (Время: {(Time.realtimeSinceStartup - startTime):F2} сек)");
    }
}