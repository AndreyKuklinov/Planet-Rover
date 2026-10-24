using UnityEditor;
using UnityEngine;

public class RoomTestWindow : EditorWindow
{
    [MenuItem("Window/Planet Rover/Room Test")]
    public static void ShowWindow()
    {
        var window = GetWindow<RoomTestWindow>("Room Test");
        window.minSize = new Vector2(320, 190);
        window.Show();
    }

    public static void EndSession()
    {
        foreach (var window in Resources.FindObjectsOfTypeAll<RoomTestWindow>())
        {
            window.Close();
        }
    }

    void OnInspectorUpdate() => Repaint();

    void OnGUI()
    {
        var room = RoomTestRunner.SelectedRoom;
        if (!RoomTestRunner.HasSession || !EditorApplication.isPlaying || room == null)
        {
            EditorGUILayout.HelpBox("Select a room prefab and choose Test Room to start a test session.", MessageType.Info);
            return;
        }
        EditorGUILayout.LabelField(room.name, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(room.IsTimed ? "Timed room" : "Untimed room");
        EditorGUILayout.HelpBox("The test starts automatically. Players can join at any time in the Game view. Progress is not saved.", MessageType.Info);
        int players = PartyInputManager.Instance == null ? 0 : PartyInputManager.Instance.PlayerCount;
        EditorGUILayout.LabelField("Joined players", players + " / 4");
        var manager = UnityEngine.Object.FindObjectOfType<LevelManager>();
        using (new EditorGUI.DisabledScope(manager == null))
            if (GUILayout.Button(manager != null && (manager.IsLevelRunning || manager.IsRoomTestFinished) ? "Restart Test Room" : "Start Test Room"))
                StartTest();
        if (GUILayout.Button("Stop Test and Return")) EditorApplication.isPlaying = false;
    }

    public void StartTest() => RoomTestRunner.StartTest();
}
