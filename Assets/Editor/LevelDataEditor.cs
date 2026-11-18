using UnityEditor;

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
    }
}
