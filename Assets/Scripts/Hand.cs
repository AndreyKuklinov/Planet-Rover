using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    // TODO: Hand should have the arm code. Instead of instantiation toggle visibility
    [field: SerializeField] public Mover Mover { get; private set; }
    [field: SerializeField] public Transform Target { get; private set; }
}
