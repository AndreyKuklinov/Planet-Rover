using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawRotator : MonoBehaviour
{
    [SerializeField] SignalItemConverter saw;
    [SerializeField] float rotationSpeed = 360f; 

    void Start()
    {
        
    }

    void Update()
    {
        if (!saw.IsPowered)
            return;

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
