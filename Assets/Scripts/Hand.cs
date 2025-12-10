using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [field: SerializeField] public Mover Mover { get; private set; }
    [field: SerializeField] public Transform Target { get; private set; }
}
