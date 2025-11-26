using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject hand_prefab;
    [SerializeField] float movement_speed;
    [SerializeField] Vector2 direction;

    public bool IsHandMoving { get; private set; }
    public GameObject Hand { get; private set; }
    public bool IsHandExtended
        => Hand != null;

    [ContextMenu("Start movement")]
    public void ExtendHand()
    {
        if (IsHandMoving)
            return;

        Hand = Instantiate(hand_prefab, transform);
        IsHandMoving = true;
    }

    [ContextMenu("Stop movement")]
    public void StopExtending()
    {
        IsHandMoving = false;
    }

    [ContextMenu("Destroy hand")]
    public void DestroyHand()
    {
        if (!IsHandExtended)
            return;
        IsHandMoving = false;
        Destroy(Hand);
    }

    [ContextMenu("Unchild hand")]
    public void UnchildHand()
    {
        if (!IsHandExtended)
            return;
        Hand.transform.SetParent(null);
    }

    void Update()
    {
        if (IsHandMoving)
        {
            Hand.transform.position += (Vector3)(movement_speed * Time.deltaTime * direction);
        }

        if (IsHandExtended)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[] {
                transform.position,
                Hand.transform.position
            });
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}
