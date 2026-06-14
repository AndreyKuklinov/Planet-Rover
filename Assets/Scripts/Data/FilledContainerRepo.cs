using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[CreateAssetMenu(menuName = "Repos/Filled Container Repo")]
public class FilledContainerRepo : ScriptableObject
{
    [SerializeField] FilledContainerData[] containers;

    public FilledContainerData GetContainerData(ContainableData containable, EmptyContainerData container)
    {
        foreach (var filledContainer in containers)
        {
            if (filledContainer.ContainableData == containable && filledContainer.EmptyContainerData == container)
                return filledContainer;
        }

        return null;
    }
}
