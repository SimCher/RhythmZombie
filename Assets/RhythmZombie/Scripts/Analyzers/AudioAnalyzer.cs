using System.Collections;
using System.Collections.Generic;
using RhythmZombie.Scripts.Rhythm.NoteSystem;
using UnityEngine;

namespace RhythmZombie.Scripts.Analyzers
{
    public class AudioAnalyzer : MonoBehaviour
    {
        [SerializeField] private float energyThreshold = 0.1f;
        [SerializeField] private float minBeatInterval = 0.2f;
        [SerializeField] private float minBPM = 60f;
        [SerializeField] private float maxBPM = 200f;
        [SerializeField] private float bpmAnalysisDuration = 10f;

        public delegate void BeatmapDetectedHandler(Beatmap beatmap);

        public event BeatmapDetectedHandler OnBeatmapDetected;

        public void DetectBeatsAsync(AudioClip clip)
        {
            StartCoroutine(DetectBeatsCoroutine(clip));
        }

        private IEnumerator DetectBeatsCoroutine(AudioClip clip)
        {
            Debug.Log("Начинается анализ трека...");
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            Debug.Log($"Сэмплы загружены: {samples.Length}");

            // Определяем BPM
            float bpm = DetectBPM(samples, clip.frequency);
            Debug.Log($"Определённый BPM: {bpm}");
            yield return null;

            // Генерируем биты
            List<float> beats = GenerateBeats(bpm, clip.length);
            Debug.Log($"Сгенерировано битов: {beats.Count}");
            yield return null;

            // Генерируем онсеты
            List<float> onsets = DetectOnsets(samples, clip.frequency);
            Debug.Log($"Сгенерировано онсетов: {onsets.Count}");
            yield return null;

            // Формируем уровни сложности
            Beatmap beatmap = new Beatmap
            {
                Easy = GenerateEasyLevel(beats),
                Medium = beats,
                Hard = CombineBeatsAndOnsets(beats, onsets)
            };

            Debug.Log(
                $"Beatmap готов: Easy={beatmap.Easy.Count}, Medium={beatmap.Medium.Count}, Hard={beatmap.Hard.Count}");
            OnBeatmapDetected?.Invoke(beatmap);
        }

private float DetectBPM(float[] samples, int sampleRate)
    {
        Debug.Log("Определение BPM...");
        int windowSize = (int)(sampleRate * bpmAnalysisDuration);
        if (windowSize > samples.Length) windowSize = samples.Length;

        // Увеличиваем размер FFT для лучшего разрешения
        int fftSize = 4096; // Увеличили с 1024 до 4096
        int hopSize = fftSize / 4;
        float[] magnitudes = new float[fftSize / 2];
        Dictionary<float, float> bpmScores = new Dictionary<float, float>();

        // Темпограмма: анализируем спектр во времени
        for (int i = 0; i < windowSize - fftSize; i += hopSize)
        {
            float[] window = new float[fftSize];
            for (int j = 0; j < fftSize; j++)
            {
                window[j] = samples[i + j] * (0.54f - 0.46f * Mathf.Cos(2 * Mathf.PI * j / (fftSize - 1))); // Окно Хэмминга
            }

            // Вычисляем FFT
            for (int k = 0; k < fftSize / 2; k++)
            {
                float sumReal = 0f, sumImag = 0f;
                for (int j = 0; j < fftSize; j++)
                {
                    float angle = 2 * Mathf.PI * k * j / fftSize;
                    sumReal += window[j] * Mathf.Cos(angle);
                    sumImag -= window[j] * Mathf.Sin(angle);
                }
                magnitudes[k] = Mathf.Sqrt(sumReal * sumReal + sumImag * sumImag);
            }

            // Ищем пики в низких частотах
            for (int k = 0; k < fftSize / 2; k++)
            {
                float freq = k * sampleRate / (float)fftSize;
                float bpm = freq * 60f;
                if (bpm >= minBPM && bpm <= maxBPM)
                {
                    if (!bpmScores.ContainsKey(bpm))
                        bpmScores[bpm] = 0f;
                    bpmScores[bpm] += magnitudes[k];
                    Debug.Log($"Частота: {freq:F2} Гц, BPM: {bpm:F2}, Амплитуда: {magnitudes[k]:F2}");
                }
            }
        }

        // Ищем BPM с максимальным "весом"
        float bestBPM = 0f;
        float maxScore = 0f;
        foreach (var pair in bpmScores)
        {
            if (pair.Value > maxScore)
            {
                maxScore = pair.Value;
                bestBPM = pair.Key;
            }
        }

        // Корректируем гармоники
        float correctedBPM = bestBPM;
        if (bestBPM < 100) // Если BPM слишком низкий, проверяем удвоение
        {
            float doubledBPM = bestBPM * 2;
            if (doubledBPM >= minBPM && doubledBPM <= maxBPM)
            {
                correctedBPM = doubledBPM;
                Debug.Log($"BPM удвоен: {bestBPM} -> {correctedBPM}");
            }
        }
        else if (bestBPM > 160) // Если BPM слишком высокий, проверяем деление
        {
            float halvedBPM = bestBPM / 2;
            if (halvedBPM >= minBPM && halvedBPM <= maxBPM)
            {
                correctedBPM = halvedBPM;
                Debug.Log($"BPM поделён: {bestBPM} -> {correctedBPM}");
            }
        }

        return correctedBPM;
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

        private List<float> DetectOnsets(float[] audioData, int sampleRate)
        {
            Debug.Log("Обнаружение онсетов...");
            List<float> onsets = new List<float>();
            int windowSize = sampleRate / 20;
            float lastOnsetTime = -minBeatInterval;
            float[] spectralFlux = new float[audioData.Length / windowSize];

            // Вычисляем спектральный поток
            for (int i = 0; i < audioData.Length - windowSize; i += windowSize)
            {
                float energy = CalculateEnergy(audioData, i, windowSize);
                float prevEnergy = i == 0 ? 0 : CalculateEnergy(audioData, i - windowSize, windowSize);
                spectralFlux[i / windowSize] = energy - prevEnergy;
            }

            // Ищем пики в спектральном потоке
            for (int i = 1; i < spectralFlux.Length - 1; i++)
            {
                float currentTime = (i * windowSize) / (float) sampleRate;
                if (spectralFlux[i] > energyThreshold && spectralFlux[i] > spectralFlux[i - 1] &&
                    spectralFlux[i] > spectralFlux[i + 1] && currentTime - lastOnsetTime >= minBeatInterval)
                {
                    onsets.Add(currentTime);
                    lastOnsetTime = currentTime;
                    Debug.Log($"Онсет обнаружен в {currentTime:F3} сек");
                }
            }

            return onsets;
        }

        private List<float> GenerateEasyLevel(List<float> beats)
        {
            List<float> easyBeats = new List<float>();
            for (int i = 0; i < beats.Count; i += 2)
            {
                easyBeats.Add(beats[i]);
            }

            return easyBeats;
        }

        private List<float> CombineBeatsAndOnsets(List<float> beats, List<float> onsets)
        {
            List<float> combined = new List<float>(beats);
            combined.AddRange(onsets);
            combined.Sort();

            List<float> filtered = new List<float> {combined[0]};
            for (int i = 1; i < combined.Count; i++)
            {
                if (combined[i] - filtered[filtered.Count - 1] >= minBeatInterval)
                {
                    filtered.Add(combined[i]);
                }
            }

            return filtered;
        }

        private float CalculateEnergy(float[] audioData, int startIndex, int length)
        {
            float energy = 0f;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                energy += audioData[i] * audioData[i];
            }

            return energy / length;
        }
    }
    //     [SerializeField] private float energyThreshold = 0.1f;
    //     [SerializeField] private float onsetThreshold = 0.05f;
    //     [SerializeField] private float minBeatInterval = 0.2f;
    //     [SerializeField] private float minOnsetInterval = 0.1f;
    //
    //     private float CalculateOnsetDifference(float[] audioData, int start, int windowSize)
    //     {
    //         var diff = 0f;
    //         for (int j = 1; j < windowSize; j++)
    //             diff += Math.Abs(audioData[start + j] - audioData[start + j - 1]);
    //
    //         return diff / windowSize;
    //     }
    //
    //     private float CalculateEnergy(float[] audioData, int start, int windowSize)
    //     {
    //         var energy = 0f;
    //         for (int j = 0; j < windowSize; j++)
    //             energy += audioData[start + j] * audioData[start + j];
    //
    //         return energy / windowSize;
    //     }
    //
    //     private List<float> DetectBeatsAndOnsets(float[] audioData, int sampleRate, List<float> beats)
    //     {
    //         var onsets = new List<float>();
    //         var windowSize = sampleRate / 20;
    //         var lastOnsetTime = -minOnsetInterval;
    //
    //         for (int i = 0; i < audioData.Length - windowSize; i += windowSize / 2)
    //         {
    //             var diff = CalculateOnsetDifference(audioData, i, windowSize);
    //             var currentTime = i / (float) sampleRate;
    //
    //             if (diff > onsetThreshold && currentTime - lastOnsetTime >= minOnsetInterval)
    //             {
    //                 onsets.Add(currentTime);
    //                 lastOnsetTime = currentTime;
    //             }
    //         }
    //
    //         var combined = new List<float>(beats);
    //         combined.AddRange(onsets);
    //         combined.Sort();
    //         return combined;
    //     }
    //
    //     private List<float> DetectBeats(float[] audioData, int sampleRate)
    //     {
    //         var beats = new List<float>();
    //         var windowSize = sampleRate / 10;
    //         var lastBeatTime = -minBeatInterval;
    //
    //         for (int i = 0; i < audioData.Length - windowSize; i += windowSize / 4)
    //         {
    //             var energy = CalculateEnergy(audioData, i, windowSize);
    //             var currentTime = i / (float) sampleRate;
    //
    //             if (energy > energyThreshold && currentTime - lastBeatTime >= minBeatInterval)
    //             {
    //                 beats.Add(currentTime);
    //                 lastBeatTime = currentTime;
    //                 i += (int) (minBeatInterval * sampleRate);
    //             }
    //         }
    //
    //         return beats;
    //     }
    //
    //     private Beatmap GenerateBeatmap(float[] audioData, int sampleRate)
    //     {
    //         var beatTimes = DetectBeats(audioData, sampleRate);
    //         var hardTimes = DetectBeatsAndOnsets(audioData, sampleRate, beatTimes);
    //
    //         return new Beatmap
    //         {
    //             Easy = beatTimes.GetRange(0, beatTimes.Count / 2),
    //             Medium = beatTimes,
    //             Hard = hardTimes
    //         };
    //     }
    //     public void Analyze(AudioClip clip)
    //     {
    //         var audioData = new float[clip.samples * clip.channels];
    //         clip.GetData(audioData, 0);
    //         var sampleRate = clip.frequency;
    //
    //         var beatmap = GenerateBeatmap(audioData, sampleRate);
    //         OnAnalysisComplete?.Invoke(beatmap);
    //     }
    //
    //     public event Action<Beatmap> OnAnalysisComplete;
    // }
}