using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalColorRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] IHasSignal obj;

    private void OnValidate()
    {
        if (spriteRenderer == null || obj == null)
            return;

        spriteRenderer.color = SignalColor.GetColor(obj.Signal);
    }
}
