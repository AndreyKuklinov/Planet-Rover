using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Filled Container Data")]
public class FilledContainerData : ItemContainerData
{
    [field: SerializeField] public DeliverableData DeliverableData { get; private set; }
    [field: SerializeField] public ContainableData ContainableData { get; private set; }
    [field: SerializeField] public EmptyContainerData EmptyContainerData { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
