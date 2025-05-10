using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmZombie.UI.Assets.Scripts.FileBrowser
{
    public class FolderListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text folderNameText;
        [SerializeField] private TMP_Text trackCountText;
        [SerializeField] private TMP_Text fullPathText;
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image underline;

        private FolderData _data;
        private Action<FolderListItem, FolderData> _onClick;
        private Action<FolderListItem, FolderData> _onDelete;

        public void Setup(FolderData data, Action<FolderListItem, FolderData> onClick,
            Action<FolderListItem, FolderData> onDelete)
        {
            _data = data;
            _onDelete = onDelete;
            _onClick = onClick;

            folderNameText.text = Path.GetFileName(data.path);
            trackCountText.text = $"Треков: {data.trackCount}";
            fullPathText.text = data.path;

            underline.enabled = data.trackCount == 0;
            
            button.onClick.AddListener(() => onClick?.Invoke(this, data));
            deleteButton.onClick.AddListener(() => onDelete?.Invoke(this, data));
        }

        public void SetSelected(bool isSelected)
        {
            background.color = isSelected
                ? new Color(0.5f, 0f, 1f, 0.2f)
                : Color.clear;
        }
    }
}