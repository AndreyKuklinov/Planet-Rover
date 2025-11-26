using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverHand : MonoBehaviour
{
    [SerializeField] float _movement_speed;
    [SerializeField] bool _is_moving;

    Vector2 _direction = Vector2.left;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_is_moving)
        {
            transform.position += (Vector3)(_movement_speed * Time.deltaTime * _direction);
        }
    }
}
