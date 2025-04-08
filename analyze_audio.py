import sys
import librosa
import numpy as np
import json

def analyze_track(audio_path):
    y, sr = librosa.load(audio_path)
    
    tempo, beat_frames = librosa.beat.beat_track(y=y, sr=sr)
    beat_times = librosa.frames_to_time(beat_frames, sr=sr)
    
    onset_frames = librosa.onset.onset_detect(y=y, sr=sr)
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)
    
    easy_times = beat_times[::2]
    medium_times = beat_times
    hard_times = np.concatenate([beat_times, onset_times])
    hard_times = np.unique(np.sort(hard_times))
    
    levels = {
        "easy":easy_times.tolist(),
        "medium":medium_times.tolist(),
        "hard":hard_times.tolist()
    }
    
    output_path = "beatmap.json"
    with open(output_path, "w") as f:
        json.dump(levels, f, indent=4)
    
    return output_path

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Укажи путь к аудиофайлу как аргумент!")
        sys.exit(1)
    
    audio_path = sys.argv[1]
    result = analyze_track(audio_path)
    print(f"Анализ завершён, результат сохранён в {result}")