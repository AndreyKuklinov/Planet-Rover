using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketIcons : MonoBehaviour
{
    [SerializeField] DeliveryIconRepo deliveryIconRepo;
    [SerializeField] ItemCollector collector; 
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

        foreach (var item in collector.RequiredItems)
        {
            var icon = Instantiate(iconPrefab, transform);
            icon.sprite = deliveryIconRepo.GetIcon(item);
            icons.Add(icon);
        }
    }
}
