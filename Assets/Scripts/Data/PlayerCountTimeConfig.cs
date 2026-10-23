using UnityEngine;

[CreateAssetMenu(fileName = "Player Count Time Config", menuName = "Levels/Player Count Time Config")]
public class PlayerCountTimeConfig : ScriptableObject
{
    [SerializeField, Min(0.01f)] float onePlayerMultiplier = 1f;
    [SerializeField, Min(0.01f)] float twoPlayerMultiplier = 1.5f;
    [SerializeField, Min(0.01f)] float threePlayerMultiplier = 1.5f;
    [SerializeField, Min(0.01f)] float fourPlayerMultiplier = 1.5f;

    public float GetMultiplier(int playerCount)
    {
        switch (Mathf.Clamp(playerCount, 1, 4))
        {
            case 1: return onePlayerMultiplier;
            case 2: return twoPlayerMultiplier;
            case 3: return threePlayerMultiplier;
            default: return fourPlayerMultiplier;
        }
    }
}
