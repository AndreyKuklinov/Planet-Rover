using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Signal : MonoBehaviour
{
    [field: SerializeField] public SignalType SignalType { get; private set; }
}
