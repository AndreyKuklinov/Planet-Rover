using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLineRenderer : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] RoverArm arm;

    void Update()
    {
        if (!arm.IsHandExtended)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.SetPositions(new[] {
            arm.transform.position,
            arm.HandPosition
        });
        lineRenderer.enabled = true;
    }
}
