using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    [SerializeField] BetterHand hand;

    void Start()
    {
        transform.rotation = Quaternion.LookRotation(
            Vector3.forward, 
            DirectionVector.GetVector3(hand.Direction)
        );
    }

    void Update()
    {
        transform.position = hand.HandPosition;
    }
}
