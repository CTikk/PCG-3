using System;
using System.Linq;
using UnityEngine;

public class GAController : MonoBehaviour
{
    [Header("Player Settings")]
    public PlayerStats playerStats = new PlayerStats();

    [Header("GA Settings")]
    public int populationSize = 20;
    public int generationsPerSecond = 5;
    public float mutationRate = 1.5f;

    private EnemyGenome[] population;
    private float[] fitness;
    private System.Random rng = new System.Random();

    private int generation;
    private EnemyGenome bestEnemy;
    private float bestFitness;

    public UIManager uiManager;

    void Start()
    {
        population = new EnemyGenome[populationSize];
        fitness = new float[populationSize];

        for (int i = 0; i < populationSize; i++)
            population[i] = EnemyGenome.RandomGenome(rng);

        EvaluatePopulation();
        UpdateBest();
        generation++;

        uiManager.UpdateUI(generation, bestFitness, bestEnemy, playerStats);
    }

    void Update()
    {
        if (Time.frameCount % (60 / generationsPerSecond) == 0)
        {
            NextGeneration();
        }
    }

    void EvaluatePopulation()
    {
        for (int i = 0; i < populationSize; i++)
            fitness[i] = BattleSimulator.Evaluate(population[i], playerStats);
    }

    void UpdateBest()
    {
        int idx = Array.IndexOf(fitness, fitness.Max());
        bestEnemy = population[idx].Clone();
        bestFitness = fitness[idx];
    }

    void NextGeneration()
    {
        EnemyGenome[] newPop = new EnemyGenome[populationSize];
        newPop[0] = bestEnemy.Clone(); // toma al mejor y lo incluyr rn la siguiente generacion

        for (int i = 1; i < populationSize; i++)
        {
            EnemyGenome parent1 = Tournament();
            EnemyGenome parent2 = Tournament();
            EnemyGenome child = Crossover(parent1, parent2);
            Mutate(child);
            child.Normalize();
            newPop[i] = child;
        }

        population = newPop;
        EvaluatePopulation();
        UpdateBest();
        generation++;

        uiManager.UpdateUI(generation, bestFitness, bestEnemy, playerStats);
    }

    EnemyGenome Tournament(int k = 3)
    {
        EnemyGenome best = null;
        float bestFit = float.MinValue;

        for (int i = 0; i < k; i++)
        {
            int idx = rng.Next(populationSize);
            if (fitness[idx] > bestFit)
            {
                bestFit = fitness[idx];
                best = population[idx];
            }
        }
        return best;
    }

    EnemyGenome Crossover(EnemyGenome a, EnemyGenome b)
    {
        EnemyGenome c = new EnemyGenome
        {
            speed = (a.speed + b.speed) / 2,
            damage = (a.damage + b.damage) / 2,
            range = (a.range + b.range) / 2,
            attackRate = (a.attackRate + b.attackRate) / 2
        };
        return c;
    }

    void Mutate(EnemyGenome g)
    {
        if (rng.NextDouble() < mutationRate) g.hp += UnityEngine.Random.Range(-2f, 2f);
        if (rng.NextDouble() < mutationRate) g.speed += UnityEngine.Random.Range(-1.5f, 1.5f);
        if (rng.NextDouble() < mutationRate) g.damage += UnityEngine.Random.Range(-2f, 2f);
        if (rng.NextDouble() < mutationRate) g.range += UnityEngine.Random.Range(-1.5f, 1.5f);
        if (rng.NextDouble() < mutationRate) g.attackRate += UnityEngine.Random.Range(-0.8f, 0.8f);

        g.Normalize(); // asegurs}ac tope :)
    }
}
