using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Deliverable Data")]
public class DeliverableData : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
}
