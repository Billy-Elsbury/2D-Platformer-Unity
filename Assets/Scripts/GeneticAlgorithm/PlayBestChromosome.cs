using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayBestChromosome : MonoBehaviour
{
    private GeneticAlgorithmRunner gar;
    private GameManager gm;
    private PlayerController player;
    private Vector3 startPosition;

    private void Start()
    {
        // Add a listener to the button click event
        GetComponent<Button>().onClick.AddListener(PlayBestSolution);
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        gar = GameObject.Find("GeneticAlgorithmRunner").GetComponent<GeneticAlgorithmRunner>();
        gm = GameManager.instance;
        startPosition = player.startPosition;
    }

    public void PlayBestSolution()
    {
        if (gar.bestSolution != null)
        {
            Debug.Log("Playing best solution...");

            // Reset the player's position and state
            player.ResetPlayer();
            GameManager.instance.ResetGameState();

            // Dynamically split the chromosome with updated ratio (Right: 60%, Jump: 30%, Left: 10%)
            int geneCount = gar.bestSolution.Chromosome.Length;
            int rightSplit = Mathf.FloorToInt(geneCount * 0.6f);
            int jumpSplit = Mathf.FloorToInt(geneCount * 0.3f);
            int leftSplit = geneCount - (rightSplit + jumpSplit); // Remaining genes for Left

            // Round values to two decimal places for consistency
            List<float> roundedChromosome = gar.bestSolution.Chromosome
                .Select(gene => Mathf.Round(gene * 100f) / 100f)
                .ToList();

            // Assign the best chromosome to the player
            player.InputChromosome = new Chromosome
            {
                RightTime = roundedChromosome.Take(rightSplit).ToList(),
                JumpTime = roundedChromosome.Skip(rightSplit).Take(jumpSplit).ToList(),
                LeftTime = roundedChromosome.Skip(rightSplit + jumpSplit).ToList()
            };

            Debug.Log($"Assigned chromosome to player: " +
                      $"RightTime={string.Join(", ", player.InputChromosome.RightTime)}\n" +
                      $"JumpTime={string.Join(", ", player.InputChromosome.JumpTime)}\n" +
                      $"LeftTime={string.Join(", ", player.InputChromosome.LeftTime)}");

            // Set the player to AI control mode
            player.controlmode = Controls.AI;

            // Start the simulation
            StartCoroutine(SimulateBestSolution());
        }
        else
        {
            Debug.LogWarning("Error: No best solution found.");
        }
    }

    private IEnumerator SimulateBestSolution()
    {
        // Track the best progress during the simulation
        float bestProgress = player.transform.position.x;

        Time.timeScale = 1f; // Normal speed for replay

        // Simulate behavior for fixed amount of time
        float simulationTime = gar.simulationTime;
        float startTime = Time.time;

        while (Time.time - startTime < simulationTime)
        {
            yield return new WaitForFixedUpdate();

            // Update best progress
            if (player.transform.position.x > bestProgress)
            {
                bestProgress = player.transform.position.x;
            }

            if (gm.reachedGoal || player.isDead)
            {
                break; // Stop early if goal reached or death
            }

            yield return null;
        }

        // Gather final stats
        int finalCoins = gm.coinCount;
        float totalTime = gm.gameTimer;
        bool reachedGoal = gm.reachedGoal;
        bool died = player.isDead;

        Debug.Log($"Final Stats: \n" +
                  $"Best Progress: {bestProgress:F2}\n" +
                  $"Coins Collected: {finalCoins}\n" +
                  $"Total Time Taken: {totalTime:F2} seconds\n" +
                  $"Reached Goal: {reachedGoal}\n" +
                  $"Died: {died}\n" +
                  $"Fitness Score: {gar.bestSolution.Cost:F2}");

        Debug.Log("Best solution simulation complete.");
    }
}
