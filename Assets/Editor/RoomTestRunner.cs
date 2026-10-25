using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class RoomTestRunner
{
    const string RoomKey = "PlanetRover.RoomTest.Room";
    const string ScenesKey = "PlanetRover.RoomTest.Scenes";
    const string StageKey = "PlanetRover.RoomTest.PrefabStage";
    const string SelectionKey = "PlanetRover.RoomTest.Selection";
    const string AutoStartKey = "PlanetRover.RoomTest.AutoStart";
    const string GameScene = "Assets/Scenes/Game.unity";
    static LevelData testLevel;

    [Serializable] class SavedScenes { public SavedScene[] scenes; }
    [Serializable] class SavedScene { public string path; public bool loaded; public bool active; }

    public static bool HasSession => !string.IsNullOrEmpty(SessionState.GetString(RoomKey, ""));
    public static Room SelectedRoom => AssetDatabase.LoadAssetAtPath<GameObject>(SessionState.GetString(RoomKey, ""))?.GetComponent<Room>();

    static RoomTestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += UpdateAutoStart;
    }

    [MenuItem("Assets/Test Room", true)]
    static bool CanTestSelection() => !EditorApplication.isPlayingOrWillChangePlaymode && ResolveRoom(Selection.activeObject) != null;

    [MenuItem("Assets/Test Room", false, 2000)]
    static void TestSelection() => Launch(ResolveRoom(Selection.activeObject));

    [MenuItem("CONTEXT/Room/Test Room")]
    static void TestComponent(MenuCommand command) => Launch(ResolveRoom(command.context));

    public static Room ResolveRoom(UnityEngine.Object selected)
    {
        var go = selected as GameObject;
        if (selected is Room room) go = room.gameObject;
        if (go == null) return null;
        var stage = PrefabStageUtility.GetPrefabStage(go);
        var path = stage != null ? stage.assetPath : AssetDatabase.GetAssetPath(go);
        if (string.IsNullOrEmpty(path)) path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab != null ? prefab.GetComponent<Room>() : null;
    }

    public static void Launch(Room room)
    {
        if (room == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
        var overlaps = RoomDuplicateUtility.FindOverlaps(room);
        if (overlaps.Count > 0)
        {
            Debug.LogError($"Cannot test room '{room.name}': overlapping grid objects.\n"
                + RoomDuplicateUtility.Describe(room, overlaps) + "\nOpen the room prefab and fix the highlighted cells, or use Select Overlaps in the Room Inspector.", room);
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScene) == null)
        {
            EditorUtility.DisplayDialog("Test Room", "The Game scene could not be found at " + GameScene, "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        // GetSceneManagerSetup omits untitled scenes, so capture the open scenes directly.
        var saved = new SavedScenes { scenes = new SavedScene[SceneManager.sceneCount] };
        var activeScene = SceneManager.GetActiveScene();
        for (int i = 0; i < saved.scenes.Length; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            saved.scenes[i] = new SavedScene { path = scene.path, loaded = scene.isLoaded, active = scene == activeScene };
        }
        SessionState.SetString(ScenesKey, JsonUtility.ToJson(saved));
        SessionState.SetString(RoomKey, AssetDatabase.GetAssetPath(room.gameObject));
        SessionState.SetString(SelectionKey, AssetDatabase.GetAssetPath(Selection.activeObject));
        SessionState.SetString(StageKey, PrefabStageUtility.GetCurrentPrefabStage()?.assetPath ?? "");
        SessionState.SetBool(AutoStartKey, true);
        try
        {
            StageUtility.GoToMainStage();
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                ClearSession();
                return;
            }
            EditorSceneManager.OpenScene(GameScene);
            EditorApplication.isPlaying = true;
        }
        catch
        {
            RestoreScenes();
            throw;
        }
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!HasSession) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            var manager = UnityEngine.Object.FindObjectOfType<LevelManager>();
            if (manager != null) manager.BeginRoomTestSession();
            var lobby = UnityEngine.Object.FindObjectOfType<LobbyManager>();
            if (lobby != null) lobby.IsRoomTesting = true;
            if (!Application.isBatchMode) EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += RestoreScenes;
    }

    static void UpdateAutoStart()
    {
        if (!HasSession || !SessionState.GetBool(AutoStartKey, false)
            || !EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling) return;
        // Let the lobby finish its initial load before replacing it with the test room.
        var loader = UnityEngine.Object.FindObjectOfType<RoomLoader>();
        if (loader != null && loader.CurrentRoom == null) return;
        StartTest();
    }

    public static void StartTest()
    {
        var room = SelectedRoom;
        var manager = UnityEngine.Object.FindObjectOfType<LevelManager>();
        if (!HasSession || !EditorApplication.isPlaying || room == null || manager == null) return;
        // Start only once. Completion and quitting must still return to the lobby.
        SessionState.SetBool(AutoStartKey, false);
        if (testLevel == null) testLevel = LevelData.CreateRoomTest(room);
        manager.BeginRoomTestSession();
        manager.StartLevel(testLevel);
    }

    static void RestoreScenes()
    {
        if (!HasSession) return;
        var saved = JsonUtility.FromJson<SavedScenes>(SessionState.GetString(ScenesKey, ""));
        string stage = SessionState.GetString(StageKey, "");
        string selection = SessionState.GetString(SelectionKey, "");
        try
        {
            if (saved?.scenes != null && saved.scenes.Length > 0)
            {
                bool hasUntitledScene = Array.Exists(saved.scenes, scene => string.IsNullOrEmpty(scene.path));
                if (hasUntitledScene)
                {
                    Scene firstScene = default;
                    for (int i = 0; i < saved.scenes.Length; i++)
                    {
                        var savedScene = saved.scenes[i];
                        var scene = string.IsNullOrEmpty(savedScene.path)
                            ? EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, i == 0 ? NewSceneMode.Single : NewSceneMode.Additive)
                            : EditorSceneManager.OpenScene(savedScene.path, i == 0 ? OpenSceneMode.Single
                                : savedScene.loaded ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
                        if (i == 0) firstScene = scene;
                        if (savedScene.active) SceneManager.SetActiveScene(scene);
                    }
                    if (!saved.scenes[0].loaded) EditorSceneManager.CloseScene(firstScene, false);
                }
                else
                {
                    var setup = new SceneSetup[saved.scenes.Length];
                    for (int i = 0; i < setup.Length; i++)
                        setup[i] = new SceneSetup { path = saved.scenes[i].path, isLoaded = saved.scenes[i].loaded, isActive = saved.scenes[i].active };
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
            if (!string.IsNullOrEmpty(stage)) AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(stage));
            if (!string.IsNullOrEmpty(selection)) Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(selection);
        }
        finally
        {
            ClearSession();
        }
    }

    static void ClearSession()
    {
        if (testLevel != null) UnityEngine.Object.DestroyImmediate(testLevel);
        testLevel = null;
        RoomTestWindow.EndSession();
        SessionState.EraseString(RoomKey);
        SessionState.EraseString(ScenesKey);
        SessionState.EraseString(StageKey);
        SessionState.EraseString(SelectionKey);
        SessionState.EraseBool(AutoStartKey);
    }
}

