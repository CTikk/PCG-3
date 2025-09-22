using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    [Header("Stats del Jugador")]
    public float hp = 50f;
    public float speed = 3f;
    public float damage = 3f;
    public float range = 4f;
    public float attackRate = 1f;

    public override string ToString()
    {
        return $"Player Stats\n" +
               $"HP: {hp:F1}\n" +
               $"Speed: {speed:F1}\n" +
               $"Damage: {damage:F1}\n" +
               $"Range: {range:F1}\n" +
               $"AtkRate: {attackRate:F1}";
    }
}
