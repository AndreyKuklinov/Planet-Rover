using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    [field: SerializeField] public bool IsTutorial { get; private set; } = false;
    [SerializeField, HideIf("IsTutorial")] private int roomCountToComplete;

    [field: SerializeField] public RoomData[] Rooms { get; private set;  }

    public string PrefsString
        => name + "_stars";

    public bool IsOrderRandom
        => !IsTutorial;

    public bool IsTimeLimited
        => !IsTutorial;

    public bool GoToLobbyWhenWon
        => IsTutorial;

    public int RoomCountToComplete
        => IsTutorial ? Rooms.Length : roomCountToComplete;
}
