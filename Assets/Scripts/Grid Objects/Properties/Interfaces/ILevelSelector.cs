using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILevelSelector : MonoBehaviour
{
    public static event Action<LevelData> LevelSelected;
}
