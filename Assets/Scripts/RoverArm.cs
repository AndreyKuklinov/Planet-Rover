using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject hand_prefab;
    [SerializeField] float movement_speed;
    [SerializeField] Vector2 direction;

    [SerializeField] bool is_moving = false;

    GameObject hand;

    [ContextMenu("Start movement")]
    public void StartMovement()
    {
        if (is_moving)
            return;

        hand = Instantiate(hand_prefab, transform);
        is_moving = true;
    }

    [ContextMenu("Stop movement")]
    public void StopMovement()
    {
        is_moving = false;
    }

    [ContextMenu("Destroy hand")]
    public void DestroyHand()
    {
        Destroy(hand);
    }

    void Update()
    {
        if (is_moving)
        {
            hand.transform.position += (Vector3)(movement_speed * Time.deltaTime * direction);
        }

        if (hand != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[] {
                transform.position,
                hand.transform.position
            });
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}
