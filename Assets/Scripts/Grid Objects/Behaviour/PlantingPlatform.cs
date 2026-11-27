using UnityEngine;

public class PlantingPlatform : MonoBehaviour
{
    [SerializeField] ItemCollector collector;
    [SerializeField] Objective objective;
    [SerializeField] GameObject sign;
    [SerializeField] SpriteRenderer seedIcon;
    [SerializeField] GameObject grownPlant;

    void OnEnable()
    {
        collector.RequiredObjectsChanged += RefreshSeedIcon;
        collector.AllObjectsCollected += Grow;
    }

    void OnDisable()
    {
        collector.RequiredObjectsChanged -= RefreshSeedIcon;
        collector.AllObjectsCollected -= Grow;
    }

    void RefreshSeedIcon()
    {
        if (collector.RequiredItems.Count > 0)
            seedIcon.sprite = collector.RequiredItems[0].MainSprite;
    }

    void Grow()
    {
        sign.SetActive(false);
        grownPlant.SetActive(true);
        objective.Complete();
    }
}
