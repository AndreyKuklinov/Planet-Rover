using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLineRenderer : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Rover rover;
    [SerializeField] RoverArm arm;

    void Update()
    {
        if (!arm.IsHandOut)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.SetPositions(new[] {
            rover.transform.position,
            arm.Hand.SpriteRenderer.transform.position,
        });
        lineRenderer.enabled = true;
    }
}
