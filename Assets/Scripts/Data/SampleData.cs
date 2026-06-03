using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sample Data")]
public class SampleData : ScriptableObject
{
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public int Value { get; private set; } = 1;
}
