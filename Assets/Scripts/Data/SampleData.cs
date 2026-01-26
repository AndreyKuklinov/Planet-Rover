using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Sample")]
public class SampleData : ScriptableObject
{
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
