using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateRoomWindow : EditorWindow
{
    const string MenuPath = "Assets/Create/Levels/Room";
    const string AspectRatioPreferenceKey = "PlanetRover.CreateRoom.KeepAspectRatio";
    const int MaxOddDimension = (RoomAuthoringTools.MaxRoomDimension - 1) | 1;

    int width = 17, height = 9;
    float standardTime = 30f;
    bool keepAspectRatio, addRover = true, addBorder = true;

    [MenuItem(MenuPath, false, 10)]
    static void ShowWindow()
    {
        var window = GetWindow<CreateRoomWindow>(true, "Create Room");
        window.minSize = new Vector2(360f, 280f);
        window.Show();
    }

    [MenuItem(MenuPath, true)]
    static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

    void OnEnable()
    {
        keepAspectRatio = EditorPrefs.GetBool(AspectRatioPreferenceKey);
        if (keepAspectRatio) height = ClosestAspectDimension(width, editingWidth: true);
    }

    void OnGUI()
    {
        bool lockRatio = EditorGUILayout.Toggle("Keep 16:9 aspect ratio", keepAspectRatio);
        if (lockRatio != keepAspectRatio)
        {
            keepAspectRatio = lockRatio;
            EditorPrefs.SetBool(AspectRatioPreferenceKey, keepAspectRatio);
            if (keepAspectRatio) height = ClosestAspectDimension(width, editingWidth: true);
        }

        using (var change = new EditorGUI.ChangeCheckScope())
        {
            int enteredWidth = EditorGUILayout.DelayedIntField("Width", width);
            if (change.changed)
            {
                width = ClampToOddDimension(enteredWidth);
                if (keepAspectRatio) height = ClosestAspectDimension(width, editingWidth: true);
            }
        }

        using (var change = new EditorGUI.ChangeCheckScope())
        {
            int enteredHeight = EditorGUILayout.DelayedIntField("Height", height);
            if (change.changed)
            {
                height = ClampToOddDimension(enteredHeight);
                if (keepAspectRatio) width = ClosestAspectDimension(height, editingWidth: false);
            }
        }

        standardTime = EditorGUILayout.FloatField("Standard time (seconds)", standardTime);
        addRover = EditorGUILayout.Toggle("Add rover", addRover);
        addBorder = EditorGUILayout.Toggle("Fill border", addBorder);
        EditorGUILayout.HelpBox("Creates a room prefab with border markers and grid, then opens it for editing. Timing, reset, and both random reflections start enabled in the Room Inspector.", MessageType.Info);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode
            || standardTime <= 0 || float.IsNaN(standardTime) || float.IsInfinity(standardTime)))
            if (GUILayout.Button("Create Room")) Create();
    }

    static int ClampToOddDimension(int dimension) => Mathf.Clamp(dimension, 3, MaxOddDimension) | 1;

    static int ClosestAspectDimension(int editedDimension, bool editingWidth)
    {
        const double targetRatio = 16d / 9d;
        int closest = 3;
        double smallestError = double.PositiveInfinity;
        for (int candidate = 3; candidate <= MaxOddDimension; candidate += 2)
        {
            double ratio = editingWidth ? (double)editedDimension / candidate : (double)candidate / editedDimension;
            double error = Math.Abs(ratio - targetRatio);
            if (error >= smallestError) continue;
            smallestError = error;
            closest = candidate;
        }

        return closest;
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
