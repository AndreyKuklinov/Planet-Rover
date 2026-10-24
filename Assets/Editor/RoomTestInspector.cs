using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomTestInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var room = RoomTestRunner.ResolveRoom(target);
        using (new EditorGUI.DisabledScope(room == null || EditorApplication.isPlayingOrWillChangePlaymode))
            if (GUILayout.Button("Test Room")) RoomTestRunner.Launch(room);
    }
}
