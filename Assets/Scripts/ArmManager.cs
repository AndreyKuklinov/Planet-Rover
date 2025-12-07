using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    [SerializeField] NewArm[] arms = new NewArm[4];

    public void OnButtonPressed(Direction direction)
    {
        
    }

    public void OnButtonReleased(Direction direction)
    {
        
    }

    NewArm GetArm(Direction direction)
        => arms[(int)direction];
}
