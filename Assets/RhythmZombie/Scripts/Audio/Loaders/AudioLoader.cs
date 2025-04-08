using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace RhythmZombie.Scripts.Audio.Loaders
{
    public class AudioLoader : MonoBehaviour, IAudioLoader
    {
        private string _audioFolder;
        
        public event Action<AudioClip, string> OnAudioLoaded;

        private void Awake()
        {
            InitializeAudioFolder();
        }

        private void InitializeAudioFolder()
        {
#if UNITY_EDITOR
            _audioFolder = Path.Combine(Application.dataPath, "RhythmZombie/Audio");
#else
            _audioFolder = Path.Combine(Application.persistentDataPath, "RhythmZombie/Audio");
#endif
            if (!Directory.Exists(_audioFolder))
            {
                Directory.CreateDirectory(_audioFolder);
                Debug.Log($"Папка Audio создана по пути: {_audioFolder}");
            }
        }

        private string[] GetAudioFiles()
        {
            var mp3Files = Directory.GetFiles(_audioFolder, "*.mp3");
            if (mp3Files.Length > 0) return mp3Files;
            return Directory.GetFiles(_audioFolder, "*.wav");
        }

        private IEnumerator LoadAudioCoroutine(string filePath)
        {
            using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var clip = DownloadHandlerAudioClip.GetContent(request);
                    OnAudioLoaded?.Invoke(clip, filePath);
                }
                else
                {
                    Debug.LogError($"Сбой загрузки аудио: {request.error}");
                }
            }
        }

        public void LoadAudioAsync()
        {
            var audioFiles = GetAudioFiles();
            if (audioFiles.Length == 0)
            {
                Debug.LogError($"Нет аудиофайлов в папке {_audioFolder}");
                return;
            }

            StartCoroutine(LoadAudioCoroutine(audioFiles[0]));
        }
    }
}
