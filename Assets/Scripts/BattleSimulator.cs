using UnityEngine;
using System.Linq;

public static class BattleSimulator
{
    public static float Evaluate(EnemyGenome enemy, PlayerStats player)
    {
        // --- Simulación de combate ---
        float playerHP = player.hp;
        float enemyHP = enemy.hp;

        float time = 0f;
        float dt = 0.1f;
        float nextEnemyAtk = 0f;
        float nextPlayerAtk = 0f;

        while (playerHP > 0 && enemyHP > 0 && time < 120f)
        {
            if (time >= nextEnemyAtk)
            {
                playerHP -= enemy.damage;
                nextEnemyAtk += 1f / Mathf.Max(0.1f, enemy.attackRate);
            }

            if (time >= nextPlayerAtk)
            {
                enemyHP -= player.damage;
                nextPlayerAtk += 1f / player.attackRate;
            }

            time += dt;
        }

        // --- Fitness base ---
        float combatScore = 0f;

        combatScore += (player.hp - Mathf.Max(0, playerHP)) * 2f;   // daño hecho
        combatScore += Mathf.Max(0, enemyHP) * 1f;                 // vida sobrante
        if (playerHP <= 0 && enemyHP > 0) combatScore += 100f;     // bonus si gana
        if (enemyHP <= 0 && playerHP > player.hp * 0.9f) combatScore -= 10f; // penalización si pierde fácil

        // --- Balance de stats --- 
        float[] stats = { enemy.hp, enemy.speed, enemy.damage, enemy.range, enemy.attackRate };
        float avg = stats.Average();
        float variance = stats.Sum(s => Mathf.Pow(s - avg, 2)) / stats.Length;

        float balanceFactor = 1f / (1f + variance); 
        // cuanto más equilibrados los stats, mayor el factor

        // Fitness final :)
        return combatScore * (0.5f + balanceFactor * 0.5f);
    }
}