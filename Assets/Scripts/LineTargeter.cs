using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineTargeter : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] List<Transform> targets;

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPositions(
            targets
                .Select(t => t.position)
                .ToArray()
            );
    }
}
