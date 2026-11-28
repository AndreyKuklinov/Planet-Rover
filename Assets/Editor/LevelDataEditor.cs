using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData)), CanEditMultipleObjects]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var beatAllRooms = serializedObject.FindProperty("beatAllRooms");
        if (beatAllRooms.boolValue && !beatAllRooms.hasMultipleDifferentValues)
            DrawPropertiesExcluding(serializedObject, "roomCountToComplete");
        else
            DrawPropertiesExcluding(serializedObject);
        serializedObject.ApplyModifiedProperties();
        DrawTags(targets.Cast<LevelData>().ToArray());
        if (!serializedObject.isEditingMultipleObjects && GUILayout.Button("Choose Roomsâ€¦"))
            RoomLibraryWindow.Open((LevelData)target);
    }

    internal static void DrawTags(LevelData[] levels)
    {
        var catalog = RoomLibraryTags.instance;
        GUILayout.Label("Level tags", EditorStyles.boldLabel);
        foreach (var category in catalog.Categories)
        {
            foreach (var tag in category.tags)
            {
                int count = levels.Count(tag.IsAssigned);
                if (count == 0) continue;
                string coverage = count == levels.Length ? "" : $" ({count}/{levels.Length})";
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.DrawRect(GUILayoutUtility.GetRect(5, 15, GUILayout.Width(5)), category.color);
                    GUILayout.Label(category.name + ":" + tag.name + coverage, EditorStyles.miniLabel);
                }
            }
        }
        if (GUILayout.Button("Add or remove level tags…"))
            catalog.ShowAssignmentMenu(levels.Length, tag => levels.Count(tag.IsAssigned),
                (tag, add) => AssignTags(levels, tag, add));
    }

    internal static void AssignTags(LevelData[] levels, RoomLibraryTags.Tag tag, bool add)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(add ? "Tag levels" : "Remove level tag");
        foreach (var level in levels)
        {
            var asset = new SerializedObject(level);
            tag.SetAssigned(asset, add);
            if (asset.ApplyModifiedProperties()) AssetDatabase.SaveAssetIfDirty(level);
        }
        Undo.CollapseUndoOperations(group);
    }
}
