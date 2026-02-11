using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Bounds bounds;
    [SerializeField] float extraPadding;

    const float CELL_PADDING = 0.5f;

    void Update()
    {
        var aspect = (float)Screen.width / Screen.height;
        var sizeFromHeight = (bounds.Size.y) * 0.5f;
        var sizeFromWidth = (bounds.Size.x) * 0.5f / aspect;
        var targetSize = Mathf.Max(sizeFromHeight, sizeFromWidth);

        targetSize += extraPadding + CELL_PADDING;

        cam.transform.position = new Vector3(
            bounds.Center.x,
            bounds.Center.y,
            cam.transform.position.z
        );

        cam.orthographicSize = targetSize;
    }
}
