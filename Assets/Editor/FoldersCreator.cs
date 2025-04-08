using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class FoldersCreator : EditorWindow
{
    private static string _projectName = "RhythmZombie";
    
    [MenuItem("Assets/Create Default Folders")]
    private static void SetUpFolders()
    {
        var window = ScriptableObject.CreateInstance<FoldersCreator>();
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 150);
        window.ShowPopup();
    }

    private static void CreateAllFolders()
    {
        var folders = new List<string>
        {
            "Animations",
            "Audio",
            "Editor",
            "Materials",
            "Meshes",
            "Prefabs",
            "Scripts",
            "Scenes",
            "Shaders",
            "Textures",
            "UI"
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists("Assets/" + folder))
            {
                Directory.CreateDirectory($"Assets/{_projectName}/{folder}");
            }
        }

        var uiFolders = new List<string>
        {
            "Assets",
            "Fonts",
            "Icon"
        };
        foreach (var subfolder in uiFolders)
        {
            if (!Directory.Exists($"Assets/{_projectName}/UI/{subfolder}"))
            {
                Directory.CreateDirectory($"Assets/{_projectName}/UI/{subfolder}");
            }
        }
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Напиши название Проекта используя корневую папку");
        _projectName = EditorGUILayout.TextField($"Project Name: ", _projectName);
        Repaint();
        GUILayout.Space(70);
        if (GUILayout.Button("Сгенерировать!"))
        {
            CreateAllFolders();
            Close();
        }
    }
}
