using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public bool IsTutorial { get; private set; } = false;
    [SerializeField, HideIf("IsTutorial")] private int roomCountToComplete;


    // TODO: REMOVE
    [field: SerializeField, HideInInspector] public int GameDuration { get; private set; } = 30;
    
    [field: SerializeField, HideInInspector] public int[] StarThresholds = new int[3];


    [field: SerializeField] public Room[] Levels { get; private set;  }

    public string PrefsString
        => name + "_stars";

    public bool IsOrderRandom
        => !IsTutorial;

    public bool IsTimeLimited
        => !IsTutorial;

    public bool GoToLobbyWhenWon
        => IsTutorial;

    public int RoomCountToComplete
        => IsTutorial ? Levels.Length : roomCountToComplete;
}
