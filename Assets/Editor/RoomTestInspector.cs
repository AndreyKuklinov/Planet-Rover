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
        using (new EditorGUI.DisabledScope(!RoomAuthoringTools.CanFill((Room)target) || EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("Fill Room")) RoomAuthoringTools.FillSelectedRoom((Room)target, false);
            if (GUILayout.Button("Fill Border")) RoomAuthoringTools.FillSelectedRoom((Room)target, true);
        }
        EditorGUILayout.HelpBox("Fill adds walls to empty cells between the border markers. Existing objects are kept. Use Undo to revert.", MessageType.Info);
    }
}
