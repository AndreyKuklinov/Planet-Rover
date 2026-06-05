using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SignalColorRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Signal signal;

    private void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += UpdateColor;
#endif
    }

    void UpdateColor()
    {
        if (spriteRenderer == null || signal == null)
            return;

        spriteRenderer.color = SignalColor.GetColor(signal.SignalType);
    }
}
