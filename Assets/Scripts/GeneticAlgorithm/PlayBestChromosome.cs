using System.Collections;
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

            // Dynamically split the chromosome into LeftTime, RightTime, and JumpTime
            int geneCount = gar.bestSolution.Chromosome.Length;
            int splitPoint = geneCount / 3;

            // Assign the best chromosome to the player
            player.InputChromosome = new Chromosome
            {
                LeftTime = gar.bestSolution.Chromosome.Take(splitPoint).ToList(),
                RightTime = gar.bestSolution.Chromosome.Skip(splitPoint).Take(splitPoint).ToList(),
                JumpTime = gar.bestSolution.Chromosome.Skip(2 * splitPoint).Take(splitPoint).ToList()
            };

            Debug.Log($"Assigned chromosome to player: LeftTime={string.Join(", ", player.InputChromosome.LeftTime)}, RightTime={string.Join(", ", player.InputChromosome.RightTime)}, JumpTime={string.Join(", ", player.InputChromosome.JumpTime)}");

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
        // Gather final stats
        Vector3 finalPosition = player.transform.position;
        int finalCoins = gm.coinCount;
        float totalTime = gm.gameTimer;
        bool reachedGoal = gm.reachedGoal;
        bool died = player.isDead;

        Debug.Log($"Final Stats: \n" +
                  $"Position: (X: {finalPosition.x:F2}\n" +
                  $"Coins Collected: {finalCoins}\n" +
                  $"Total Time Taken: {totalTime:F2} seconds\n" +
                  $"Reached Goal: {reachedGoal}\n" +
                  $"Died: {died}\n" +
                  $"Fitness Score: {gar.bestSolution.Cost:F2}");

        Time.timeScale = 1f; // Normal speed for replay

        // Simulate behavior for fixed amount of time
        float simulationTime = gar.simulationTime;
        float startTime = Time.time;


        while (Time.time - startTime < simulationTime)
        {
            yield return new WaitForFixedUpdate();

            if (gm.reachedGoal || player.isDead)
            {
                break; // Stop early if goal reached or death
            }

            yield return null;
        }

        Debug.Log("Best solution simulation complete.");
    }
}
