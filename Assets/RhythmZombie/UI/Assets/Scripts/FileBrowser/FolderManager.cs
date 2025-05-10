using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RhythmZombie.Scripts.Common.Progress;
using RhythmZombie.Scripts.Events;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.UI.Assets.Scripts.FileBrowser
{
    public class FolderListManager : MonoBehaviour
    {
        [Serializable]
        private class FolderList
        {
            public List<FolderData> folders = new();
        }
        
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject folderItemPrefab;
        [SerializeField] private Button addFolderBtn;
        [SerializeField] private TMP_Text noFoldersText;

        private const int MaxFolders = 10;
        private const string SaveKey = "TrackedFolders";

        private List<FolderData> _folders = new();
        private FolderListItem _selectedItem;

        public Action<FolderData> onFolderSelected;

        private void Start()
        {
            GameEvents.OnDirectoryListChanged += 
            LoadFolders();
            RefreshUI();
            addFolderBtn.onClick.AddListener(AddFolder);
        }

        private void LoadFolders()
        {
            _folders = SaveManager.Load<FolderList>(SaveKey).folders;
        }

        private void SaveFolders()
        {
            var wrapper = new FolderList {folders = _folders};
            SaveManager.Save(SaveKey, wrapper);
        }

        private void RefreshUI()
        {
            foreach(Transform child in contentRoot)
                Destroy(child.gameObject);
            
            noFoldersText.gameObject.SetActive(_folders.Count == 0);

            for (int i = 0; i < _folders.Count; i++)
            {
                var go = Instantiate(folderItemPrefab, contentRoot);
                var item = go.GetComponent<FolderListItem>();
                item.Setup(_folders[i], OnClickItem, OnDeleteItem);
            }
        }

        private void AddFolder()
        {
            if (_folders.Count >= MaxFolders)
            {
                GameEvents.RaiseNotification("Максимум 10 папок!", NotificationType.Warning);
                return;
            }

#if UNITY_STANDALONE || UNITY_EDITOR
            var path = StandaloneFileBrowser.OpenFolderPanel("Выберите папку с треками", "", false)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(path) || _folders.Any(f => f.path == path))
                return;

            var folderData = new FolderData
            {
                path = path,
                trackCount = ScanTracksInFolder(path)
            };
            _folders.Add(folderData);
            SaveFolders();
            RefreshUI();
            GameEvents.RaiseNotification("Папка успешно добавлена!", NotificationType.Success);
#endif
        }

        private void OnClickItem(FolderListItem item, FolderData data)
        {
            if(_selectedItem)
                _selectedItem.SetSelected(false);

            _selectedItem = item;
            _selectedItem.SetSelected(true);
            onFolderSelected?.Invoke(data);
        }

        private void OnDeleteItem(FolderListItem item, FolderData data)
        {
            if (_selectedItem == item)
                _selectedItem = null;

            _folders.Remove(data);
            SaveFolders();
            RefreshUI();
            GameEvents.RaiseNotification("Папка успешно удалена!", NotificationType.Success);
        }

        private int ScanTracksInFolder(string folderPath)
        {
            string[] extensions = {".mp3", ".wav", ".ogg", ".flac"};
            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Count(f => extensions.Contains(Path.GetExtension(f).ToLower()));
        }
    }
}