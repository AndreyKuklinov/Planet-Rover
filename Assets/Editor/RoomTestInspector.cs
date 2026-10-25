using UnityEditor;
using UnityEngine;
using System.Linq;

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
        DrawOverlaps((Room)target);
    }

    void DrawOverlaps(Room room)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var overlaps = RoomDuplicateUtility.FindOverlaps(room);
        if (overlaps.Count > 0)
        {
            EditorGUILayout.HelpBox($"{overlaps.Count} overlapping cell(s). These will block a game build.\n"
                + RoomDuplicateUtility.Describe(room, overlaps.Take(8))
                + (overlaps.Count > 8 ? "\nMore overlapping cells are highlighted in Scene view." : ""), MessageType.Error);
            if (GUILayout.Button("Select Overlaps"))
            {
                var editable = RoomAuthoringTools.GetEditableRoom(room);
                if (editable != null)
                    Selection.objects = RoomDuplicateUtility.FindOverlaps(editable).SelectMany(overlap => overlap.Objects)
                        .Select(obj => (Object)obj.gameObject).ToArray();
                SceneView.lastActiveSceneView?.FrameSelected();
            }
        }
    }
}
