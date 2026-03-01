using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwapMarker : MonoBehaviour
{
    readonly Color[] colors = new Color[]
    {
        ColorExtensions.FromHex("#9656a2"),
        ColorExtensions.FromHex("#369acc"),
        ColorExtensions.FromHex("#95cf92"),
        ColorExtensions.FromHex("#f8e16f"),
        ColorExtensions.FromHex("#f4895f"),
        ColorExtensions.FromHex("#de324c"),
        ColorExtensions.FromHex("#6c584c")
    };

    [field: SerializeField] public int Index {  get; private set; }

    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] Image image;

    void OnValidate()
    {
        if(textMesh != null)
            textMesh.text = Index.ToString();

        if(image != null && Index < colors.Length)
            image.color = colors[Index];
    }
}
