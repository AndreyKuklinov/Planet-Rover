using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateRoomWindow : EditorWindow
{
    int width = 17, height = 9;
    float standardTime = 30f;
    bool addRover = true, addBorder = true;

    [MenuItem("Assets/Create/Rooms/Room", false, 10)]
    static void ShowWindow()
    {
        var window = GetWindow<CreateRoomWindow>(true, "Create Room");
        window.minSize = new Vector2(360f, 260f);
        window.Show();
    }

    [MenuItem("Assets/Create/Rooms/Room", true)]
    static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Room size (cells)", EditorStyles.boldLabel);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        standardTime = EditorGUILayout.FloatField("Standard time (seconds)", standardTime);
        addRover = EditorGUILayout.Toggle("Add rover", addRover);
        addBorder = EditorGUILayout.Toggle("Fill border", addBorder);
        EditorGUILayout.HelpBox("Creates a room prefab with border markers and grid, then opens it for editing. Timing, reset, and both random reflections start enabled in the Room Inspector.", MessageType.Info);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || width < 3 || height < 3
            || width > 200 || height > 200 || standardTime <= 0 || float.IsNaN(standardTime) || float.IsInfinity(standardTime)))
            if (GUILayout.Button("Create Room")) Create();
    }

    void Create()
    {
        string folder = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!AssetDatabase.IsValidFolder(folder)) folder = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(folder)) folder = "Assets/Prefabs/Levels";
        string path = EditorUtility.SaveFilePanelInProject("Create Room", "New Room", "prefab", "Choose a filename for the new room.", folder);
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            var room = RoomAuthoringTools.CreateRoom(path, width, height, standardTime, addRover, addBorder);
            Selection.activeObject = room;
            AssetDatabase.OpenAsset(room);
            Close();
        }
        catch (Exception ex) { EditorUtility.DisplayDialog("Create Room", ex.Message, "OK"); }
    }
}
