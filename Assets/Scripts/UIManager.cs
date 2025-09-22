using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text genText;
    public Text fitnessText;
    public Text enemyStatsText;
    public Text playerStatsText;

    public void UpdateUI(int generation, float fitness, EnemyGenome enemy, PlayerStats player)
    {
        genText.text = $"Generación: {generation}";
        fitnessText.text = $"Mejor fitness: {fitness:F2}";

        enemyStatsText.text = $"Enemy Stats\n" +
                              $"HP: {enemy.hp:F1}\n" +
                              $"Speed: {enemy.speed:F1}\n" +
                              $"Damage: {enemy.damage:F1}\n" +
                              $"Range: {enemy.range:F1}\n" +
                              $"AtkRate: {enemy.attackRate:F1}\n" +
                              $"Total: {enemy.TotalPoints:F1}/{EnemyGenome.MaxPoints}";

        playerStatsText.text = player.ToString();
    }
}