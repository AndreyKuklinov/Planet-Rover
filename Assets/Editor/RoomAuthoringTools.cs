using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RoomAuthoringTools
{
    public const string DefaultConfigPath = "Assets/Scriptable Objects/Configs/Room configs/Default Room Config.asset";
    const string TemplatePath = "Assets/Prefabs/Levels/Empty Level.prefab";
    const string WallPath = "Assets/Prefabs/Level Objects/Obstacle.prefab";
    const string RoverPath = "Assets/Prefabs/Rover/Rover.prefab";

    public static bool CanFill(Room room) => room != null && room.Grid != null && room.Grid.Bounds != null;

    // Asset selections are opened in Prefab Mode so edits remain visible and undoable.
    public static void FillSelectedRoom(Room selected, bool borderOnly)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || selected == null) return;
        try
        {
            var room = GetEditableRoom(selected);
            if (room != null) Fill(room, borderOnly, true);
        }
        catch (Exception ex) { EditorUtility.DisplayDialog("Fill Room", ex.Message, "OK"); }
    }

    public static Room GetEditableRoom(Room selected)
    {
        if (selected == null || EditorApplication.isPlayingOrWillChangePlaymode) return null;
        if (!EditorUtility.IsPersistent(selected)) return selected;
        string path = AssetDatabase.GetAssetPath(selected);
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null || stage.assetPath != path)
        {
            AssetDatabase.OpenAsset(selected.gameObject);
            stage = PrefabStageUtility.GetCurrentPrefabStage();
        }
        return stage != null && stage.assetPath == path ? stage.prefabContentsRoot.GetComponent<Room>() : null;
    }

    public static int Fill(Room room, bool borderOnly, bool useUndo = true)
    {
        var cells = GetCells(room);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallPath);
        if (prefab == null) throw new InvalidOperationException("Wall prefab missing: " + WallPath);
        var occupied = new HashSet<Vector2Int>();
        foreach (var obj in room.GetComponentsInChildren<GridObject>(true))
            occupied.Add(room.Grid.WorldToCell(obj.transform.position));
        var missing = new List<Vector2Int>();
        foreach (var cell in cells)
        {
            if (occupied.Contains(cell)) continue;
            if (borderOnly && cells.Contains(cell + Vector2Int.up) && cells.Contains(cell + Vector2Int.down)
                && cells.Contains(cell + Vector2Int.left) && cells.Contains(cell + Vector2Int.right)) continue;
            missing.Add(cell);
        }
        if (missing.Count == 0) return 0;
        var parent = room.transform.Find("Objects") ?? room.Grid.transform;
        int group = -1;
        if (useUndo)
        {
            Undo.IncrementCurrentGroup(); group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(borderOnly ? "Fill Room Border" : "Fill Room");
        }
        int added = 0;
        try
        {
            foreach (var cell in missing)
            {
                var wall = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                wall.transform.position = room.Grid.CellToWorld(cell);
                PrefabUtility.RecordPrefabInstancePropertyModifications(wall.transform);
                if (useUndo) Undo.RegisterCreatedObjectUndo(wall, "Create Wall");
                added++;
            }
            if (added > 0 && useUndo) EditorSceneManager.MarkSceneDirty(room.gameObject.scene);
        }
        finally
        {
            if (useUndo)
            {
                Undo.CollapseUndoOperations(group);
                Undo.IncrementCurrentGroup();
            }
        }
        return added;
    }

    static HashSet<Vector2Int> GetCells(Room room)
    {
        if (!CanFill(room)) throw new InvalidOperationException("Assign the room grid and its border markers first.");
        var grid = room.Grid;
        var bounds = new SerializedObject(grid.Bounds);
        if (bounds.FindProperty("bottomLeftMarker").objectReferenceValue == null
            || bounds.FindProperty("topRightMarker").objectReferenceValue == null
            || new SerializedObject(grid).FindProperty("grid").objectReferenceValue == null)
            throw new InvalidOperationException("Assign the Grid, Bottom Left marker, and Top Right marker first.");
        Vector2 min = grid.Bounds.MinCoords, max = grid.Bounds.MaxCoords;
        if (min.x > max.x || min.y > max.y)
            throw new InvalidOperationException("The Bottom Left marker must be below and left of the Top Right marker.");
        var first = grid.WorldToCell(min) - Vector2Int.one;
        var last = grid.WorldToCell(max) + Vector2Int.one;
        long width = (long)last.x - first.x + 1, height = (long)last.y - first.y + 1;
        if (width <= 0 || height <= 0 || width > 100000 || height > 100000 || width * height > 100000)
            throw new InvalidOperationException("The markers cover too many cells. Move them closer before filling.");
        var cells = new HashSet<Vector2Int>();
        for (int y = first.y; y <= last.y; y++)
            for (int x = first.x; x <= last.x; x++)
            {
                var cell = new Vector2Int(x, y);
                if (grid.IsWithinBounds(cell)) cells.Add(cell);
            }
        if (cells.Count == 0) throw new InvalidOperationException("No grid cell centers lie between the border markers.");
        return cells;
    }

    public static GameObject CreateRoom(string path, int width, int height, float standardTime, bool addRover, bool addBorder)
    {
        if (width < 3 || height < 3 || width > 200 || height > 200)
            throw new ArgumentOutOfRangeException(nameof(width), "Room dimensions must be between 3 and 200 cells.");
        if (standardTime <= 0 || float.IsNaN(standardTime) || float.IsInfinity(standardTime))
            throw new ArgumentOutOfRangeException(nameof(standardTime));
        if (!path.StartsWith("Assets/", StringComparison.Ordinal) || !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
            || path.Contains("..") || File.Exists(path))
            throw new InvalidOperationException("Choose a new prefab filename inside Assets.");
        var config = AssetDatabase.LoadAssetAtPath<RoomConfig>(DefaultConfigPath);
        if (config == null) throw new InvalidOperationException("Default Room Config missing: " + DefaultConfigPath);
        var root = PrefabUtility.LoadPrefabContents(TemplatePath);
        try
        {
            root.name = Path.GetFileNameWithoutExtension(path);
            root.transform.position = new Vector3(0.5f, 0.5f, 0f);
            var room = root.GetComponent<Room>();
            var settings = new SerializedObject(room);
            settings.FindProperty("config").objectReferenceValue = config;
            settings.FindProperty("<StandardTime>k__BackingField").floatValue = standardTime;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var objects = root.transform.Find("Objects");
            if (objects == null) throw new InvalidOperationException("The room template has no Objects container.");
            while (objects.childCount > 0) UnityEngine.Object.DestroyImmediate(objects.GetChild(0).gameObject);
            var swaps = root.transform.Find("Swap Markers");
            if (swaps != null)
                while (swaps.childCount > 0) UnityEngine.Object.DestroyImmediate(swaps.GetChild(0).gameObject);
            var grid = room.Grid.GetComponent<Grid>();
            var markers = new SerializedObject(room.Grid.Bounds);
            var bottomLeft = (Transform)markers.FindProperty("bottomLeftMarker").objectReferenceValue;
            var topRight = (Transform)markers.FindProperty("topRightMarker").objectReferenceValue;
            int x = -width / 2, y = -height / 2;
            bottomLeft.position = grid.CellToWorld(new Vector3Int(x, y, 0));
            topRight.position = grid.CellToWorld(new Vector3Int(x + width, y + height, 0));
            if (addRover)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RoverPath);
                if (prefab == null) throw new InvalidOperationException("Rover prefab missing: " + RoverPath);
                var rover = (GameObject)PrefabUtility.InstantiatePrefab(prefab, objects);
                rover.transform.position = room.Grid.CellToWorld(Vector2Int.zero);
                PrefabUtility.RecordPrefabInstancePropertyModifications(rover.transform);
            }
            if (addBorder) Fill(room, true, false);
            var asset = PrefabUtility.SaveAsPrefabAsset(root, path);
            if (asset == null) throw new InvalidOperationException("Could not save the room prefab.");
            return asset;
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
