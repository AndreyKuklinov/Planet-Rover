using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomBuildValidator : IPreprocessBuildWithReport, IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        ValidateRoomAssets(AssetDatabase.GetDependencies(scenes, true));
    }

    public static void ValidateRoomAssets(IEnumerable<string> paths)
    {
        var errors = new List<string>();
        foreach (string path in paths.Distinct().OrderBy(path => path))
        {
            if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null || asset.GetComponentsInChildren<Room>(true).Length == 0) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var room in root.GetComponentsInChildren<Room>(true)) AddErrors(errors, room, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        ThrowIfInvalid(errors);
    }

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null) return;
        ValidateScene(scene);
    }

    public static void ValidateScene(Scene scene)
    {
        var errors = new List<string>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var room in root.GetComponentsInChildren<Room>(true)) AddErrors(errors, room, scene.path);
        ThrowIfInvalid(errors);
    }

    static void AddErrors(List<string> errors, Room room, string path)
    {
        var overlaps = RoomDuplicateUtility.FindOverlaps(room);
        if (overlaps.Count > 0)
            errors.Add($"Room '{room.name}' ({path}):\n" + RoomDuplicateUtility.Describe(room, overlaps));
    }

    static void ThrowIfInvalid(List<string> errors)
    {
        if (errors.Count > 0)
            throw new BuildFailedException("Build blocked: multiple room objects occupy the same grid cell.\n"
                + string.Join("\n\n", errors) + "\nOpen the room prefab and remove or move objects in the highlighted cells. Select Overlaps in the Room Inspector can help locate them.");
    }
}
