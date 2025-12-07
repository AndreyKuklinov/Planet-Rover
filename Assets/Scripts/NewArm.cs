using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewArm : MonoBehaviour
{
    [SerializeField] bool canVoluntarilyStop = true;

    public void Extend()
    {

    }

    public void StopExtending()
    {
        if (!canVoluntarilyStop)
            return;
    }

    public void Deactivate()
    {

    }
}
