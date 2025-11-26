using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float movement_speed;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            movement_speed * Time.deltaTime
        );
    }
}
