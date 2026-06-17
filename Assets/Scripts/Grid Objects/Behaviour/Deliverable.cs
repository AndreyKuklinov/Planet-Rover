using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deliverable : MonoBehaviour, IDeliverable
{
    [field: SerializeField] public DeliverableData DeliverableData { get; set; }
}