using System;
using UnityEngine;

[Serializable]
public class EnemyGenome
{
    public float hp;
    public float speed;
    public float damage;
    public float range;
    public float attackRate;

    public const int MaxPoints = 30;      // tope total de puntos
    public const float MinValue = 0.1f;   // mínimo por stat
    public const float MaxValue = 10f;    // máximo por stat

    public float TotalPoints => hp + speed + damage + range + attackRate;

    public EnemyGenome Clone() => new EnemyGenome
    {
        hp = this.hp,
        speed = this.speed,
        damage = this.damage,
        range = this.range,
        attackRate = this.attackRate
    };

    public static EnemyGenome RandomGenome(System.Random rng)
    {
        EnemyGenome g = new EnemyGenome();

        float[] genes = new float[5];
        float remaining = MaxPoints;

        for (int i = 0; i < 5; i++)
        {
            float val = (float)(rng.NextDouble() * remaining);
            genes[i] = Mathf.Clamp(val, MinValue, MaxValue);
            remaining -= val;
        }

        g.hp = genes[0];
        g.speed = genes[1];
        g.damage = genes[2];
        g.range = genes[3];
        g.attackRate = genes[4];

        g.Normalize();
        return g;
    }

    public void Normalize()
    {
        // Ajuste si excede el total máximo
        if (TotalPoints > MaxPoints)
        {
            float factor = MaxPoints / TotalPoints;
            hp *= factor;
            speed *= factor;
            damage *= factor;
            range *= factor;
            attackRate *= factor;
        }

        // clamps individuales
        hp = Mathf.Clamp(hp, MinValue, MaxValue);
        speed = Mathf.Clamp(speed, MinValue, MaxValue);
        damage = Mathf.Clamp(damage, MinValue, MaxValue);
        range = Mathf.Clamp(range, MinValue, MaxValue);
        attackRate = Mathf.Clamp(attackRate, MinValue, MaxValue);
    }
}
