using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Mover handPrefab;
    [SerializeField] float movementSpeed;
    [SerializeField] Direction direction;

    public bool IsHandMoving
        => Hand.IsMoving;
    public Mover Hand { get; private set; }
    public bool IsHandExtended
        => Hand != null;

    public void OnButtonPressed()
    {
        ExtendHand();
    }

    [ContextMenu("Start movement")]
    public void ExtendHand()
    {
        if (IsHandExtended)
            return;

        Hand = Instantiate(handPrefab, transform);
        Hand.MoveInDirection(direction, movementSpeed);
    }

    [ContextMenu("Stop movement")]
    public void StopExtending()
    {
        Hand.StopMoving();
    }

    [ContextMenu("Destroy hand")]
    public void DestroyHand()
    {
        if (!IsHandExtended)
            return;

        Hand.StopMoving();
        Destroy(Hand.gameObject);
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
        UpdateArmLine();
    }

    void UpdateArmLine()
    {
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
