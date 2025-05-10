using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace RhythmZombie.UI.Assets.Scripts.SongSelection
{
    public class SongScanner
    {
        public List<SongSelectionManager.SongData> Songs { get; set; } = new();

        public event System.Action OnScanStarted;
        public event System.Action OnScanCompleted;

        private SongCache _songCache;

        public SongScanner(SongCache cache)
        {
            _songCache = cache;
        }

        private string GenerateBeatmapPath(string songPath)
        {
            var beatmapDir = Path.Combine(Application.persistentDataPath, "Beatmaps");
            if (!Directory.Exists(beatmapDir)) Directory.CreateDirectory(beatmapDir);

            var beatmapFile = Path.GetFileNameWithoutExtension(songPath) + "_beatmap.json";
            var beatmapPath = Path.Combine(beatmapDir, beatmapFile);
            
            if(!File.Exists(beatmapPath))
                File.WriteAllText(beatmapPath, "[]");

            return beatmapPath;
        }

        public async Task ScanFoldersAsync(List<string> folders, bool forceRescan)
        {
            if (!forceRescan && !_songCache.HasFoldersChanged(folders, Songs))
            {
                OnScanCompleted?.Invoke();
                return;
            }
            
            OnScanStarted?.Invoke();
            Songs.Clear();

            await Task.Run(() =>
            {
                string[] supportedFormats = {"*.mp3", "*.wav", "*.ogg"};
                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) continue;
                    foreach (var format in supportedFormats)
                    {
                        var files = Directory.GetFiles(folder, format, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            Songs.Add(new SongSelectionManager.SongData
                            {
                                filePath = file,
                                title = Path.GetFileNameWithoutExtension(file),
                                artist = "Unknown",
                                album = "Unknown",
                                year = "Unknown",
                                duration = 180f,
                                hitCount = 100,
                                difficulty = "Средняя",
                                beatmapPath = GenerateBeatmapPath(file)
                            });
                        }
                    }
                }
            });

            _songCache.SaveCache(folders, Songs);
            OnScanCompleted?.Invoke();
        }
    }
}