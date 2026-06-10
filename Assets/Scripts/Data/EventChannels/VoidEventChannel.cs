using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
	public event Action Raised;

	public void Raise()
	{
		Raised?.Invoke();
	}
}