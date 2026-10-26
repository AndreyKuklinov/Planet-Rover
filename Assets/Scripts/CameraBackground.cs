using UnityEngine;

// Keep the camera's backdrop covering the viewport as room framing changes.
[RequireComponent(typeof(SpriteRenderer))]
public class CameraBackground : MonoBehaviour
{
    Camera targetCamera;
    SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        targetCamera = GetComponentInParent<Camera>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (targetCamera == null || !targetCamera.orthographic || spriteRenderer.sprite == null)
            return;

        var spriteSize = spriteRenderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        var height = targetCamera.orthographicSize * 2f;
        var scale = Mathf.Max(height * targetCamera.aspect / spriteSize.x, height / spriteSize.y);
        var parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(scale / Mathf.Abs(parentScale.x),
            scale / Mathf.Abs(parentScale.y), transform.localScale.z);

        // Retain the authored depth, centering the image even if its pivot is off-center.
        var depth = Vector3.Dot(transform.position - targetCamera.transform.position,
            targetCamera.transform.forward);
        transform.rotation = targetCamera.transform.rotation;
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * depth
            - transform.TransformVector(spriteRenderer.sprite.bounds.center);
    }
}
