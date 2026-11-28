using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(Room))]
public class RoomTestInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var room = RoomTestRunner.ResolveRoom(target);
        if (room != null && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            var inspected = (Room)target;
            DrawRoomTags(PrefabStageUtility.GetPrefabStage(inspected.gameObject) != null ? inspected : room);
        }
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

    static void DrawRoomTags(Room room)
    {
        var catalog = RoomLibraryTags.instance;
        GUILayout.Label("Room tags", EditorStyles.boldLabel);
        foreach (var category in catalog.Categories)
        foreach (var tag in category.tags.Where(t => t.IsAssigned(room)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(5, 15, GUILayout.Width(5)), category.color);
                GUILayout.Label(category.name + ":" + tag.name, EditorStyles.miniLabel);
            }
        }
        if (GUILayout.Button("Add or remove room tags…"))
            catalog.ShowAssignmentMenu(1, tag => tag.IsAssigned(room) ? 1 : 0, (tag, add) =>
            {
                if (room == null) return;
                var stage = PrefabStageUtility.GetPrefabStage(room.gameObject);
                if (stage == null)
                {
                    RoomLibraryWindow.Assign(new[] { room }, tag.id, add);
                    return;
                }
                var settings = new SerializedObject(room);
                tag.SetAssigned(settings, add);
                if (settings.ApplyModifiedProperties())
                    EditorSceneManager.MarkSceneDirty(stage.scene);
            });
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
