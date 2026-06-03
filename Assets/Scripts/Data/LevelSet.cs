using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Set")]
public class LevelSet : ScriptableObject
{
    [field: SerializeField] public bool IsTutorial = false;
    //[field: SerializeField] public bool IsOrderRandom = true;
    //[field: SerializeField] public bool IsTimeLimited = true;
    //[field: SerializeField] public bool EndGameWhenWon = false;
    [field: SerializeField] public int GameDuration = 30;
    [field: SerializeField] public int[] StarThresholds = new int[3];
    [field: SerializeField] public Room[] Levels { get; private set;  }

    public string PrefsString
        => name + "_stars";

    public bool IsOrderRandom
        => !IsTutorial;

    public bool IsTimeLimited
        => !IsTutorial;

    public bool GoToLobbyWhenWon
        => IsTutorial;
}
