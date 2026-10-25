using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Scene-view overlays belong to the open prefab, rather than the selected Inspector.
[InitializeOnLoad]
public static class RoomOverlapHighlighter
{
    static GUIStyle labelStyle;

    static RoomOverlapHighlighter()
    {
        SceneView.duringSceneGui += Draw;
        EditorApplication.hierarchyChanged += SceneView.RepaintAll;
        Undo.undoRedoPerformed += SceneView.RepaintAll;
        Selection.selectionChanged += SceneView.RepaintAll;
    }

    public static Room[] GetRoomsToDraw()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return new Room[0];
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null) return stage.prefabContentsRoot.GetComponentsInChildren<Room>(true);
        var rooms = new HashSet<Room>();
        foreach (var go in Selection.gameObjects)
        {
            if (EditorUtility.IsPersistent(go)) continue;
            var parent = go.GetComponentInParent<Room>();
            if (parent != null) rooms.Add(parent);
            foreach (var child in go.GetComponentsInChildren<Room>(true)) rooms.Add(child);
        }
        var result = new Room[rooms.Count];
        rooms.CopyTo(result);
        return result;
    }

    static void Draw(SceneView view)
    {
        if (Event.current.type != EventType.Repaint) return;
        var previousDepth = Handles.zTest;
        var previousColor = Handles.color;
        try
        {
            Handles.zTest = CompareFunction.Always;
            foreach (var room in GetRoomsToDraw())
                foreach (var overlap in RoomDuplicateUtility.FindOverlaps(room)) DrawCell(room, overlap);
        }
        finally
        {
            Handles.zTest = previousDepth;
            Handles.color = previousColor;
        }
    }

    static void DrawCell(Room room, RoomDuplicateUtility.Overlap overlap)
    {
        var center = room.Grid.CellToWorld(overlap.Cell);
        var x = (room.Grid.CellToWorld(overlap.Cell + Vector2Int.right) - center) * 0.48f;
        var y = (room.Grid.CellToWorld(overlap.Cell + Vector2Int.up) - center) * 0.48f;
        var corners = new[] { center - x - y, center - x + y, center + x + y, center + x - y };
        Handles.DrawSolidRectangleWithOutline(corners, new Color(1f, 0.05f, 0.05f, 0.3f), Color.yellow);
        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(6f, corners[0], corners[1], corners[2], corners[3], corners[0]);
        Handles.DrawAAPolyLine(4f, corners[0], corners[2]);
        Handles.DrawAAPolyLine(4f, corners[1], corners[3]);
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                padding = new RectOffset(6, 6, 4, 4)
            };
            labelStyle.normal.textColor = Color.yellow;
        }
        Handles.BeginGUI();
        try
        {
            var content = new GUIContent($"OVERLAP: {overlap.Objects.Count} objects\nCell {overlap.Cell}");
            var size = labelStyle.CalcSize(content);
            var point = HandleUtility.WorldToGUIPoint(center + y);
            var rect = new Rect(point.x - size.x * 0.5f, point.y - size.y, size.x, size.y);
            EditorGUI.DrawRect(rect, new Color(0.05f, 0.05f, 0.05f, 0.95f));
            GUI.Label(rect, content, labelStyle);
        }
        finally { Handles.EndGUI(); }
    }
}
