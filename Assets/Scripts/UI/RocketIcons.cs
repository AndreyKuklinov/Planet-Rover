using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketIcons : MonoBehaviour
{
    [SerializeField] DeliverableCollector collector;
    [SerializeField] Image iconPrefab;

    private List<Image> icons = new();

    void Start()
    {
        collector.RequiredObjectsChanged += OnRocketObjectsChanged;
        OnRocketObjectsChanged();
    }

    private void OnRocketObjectsChanged()
    {
        foreach (var icon in icons)
            Destroy(icon.gameObject);

        icons = new List<Image>();

        foreach (var sample in collector.RequiredObjects)
        {
            var icon = Instantiate(iconPrefab, transform);
            icon.sprite = sample.Icon;
            icons.Add(icon);
        }
    }
}
