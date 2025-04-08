import librosa
import numpy as np
import json
import os

audio_folder = "RhythmZombie/Audio"
output_file = "RhythmZombie/beatmap.json"

audio_files = [f for f in os.listdir(audio_folder) if f.endswith('.mp3')]
if not audio_files:
    print("Нет MP3 файлов в папке!")
    exit()
audio_path = os.path.join(audio_folder, audio_files[0])

y, sr = librosa.load(audio_path)

tempo, beats = librosa.beat.beat_track(y=y, sr=sr)
beat_times = librosa.frames_to_time(beats, sr=sr)

onset_frames = librosa.onset.onset_detect(y=y, sr=sr)
onset_times = librosa.frames_to_time(onset_frames, sr=sr)

combined_times = np.sort(np.concatenate((beat_times, onset_times)))

difficulty_levels = {
    "easy": combined_times[::4].tolist(),
    "medium": combined_times[::2].tolist(),
    "hard": combined_times.tolist(),
    "impossible": (combined_times * 2).tolist()
}

with open(output_file, 'w') as f:
    json.dump(difficulty_levels, f, indent=4)
    
print(f"Beatmap сохранён в {output_file}")