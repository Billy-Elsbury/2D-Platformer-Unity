using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System; 

public class PlayBestChromosome : MonoBehaviour
{
    public TMP_InputField manualChromosomeInput;
    private GeneticAlgorithmRunner gar;
    private GameManager gm;
    private PlayerController player;
    private Vector3 startPosition;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        if (player == null) Debug.LogError("PlayBestChromosome: Player not found!");

        gar = GameObject.Find("GeneticAlgorithmRunner").GetComponent<GeneticAlgorithmRunner>();
        if (gar == null) Debug.LogError("PlayBestChromosome: GeneticAlgorithmRunner not found!");

        gm = GameManager.instance;
        if (gm == null) Debug.LogError("PlayBestChromosome: GameManager instance not found!");

        startPosition = player.startPosition;

        // listener to button click event to call new method
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayManualChromosome);
        }
        else
        {
            Debug.LogError("PlayBestChromosome: Button component not found on this GameObject!");
        }

        if (manualChromosomeInput == null)
        {
            Debug.LogError("PlayBestChromosome: Manual Chromosome TMP_InputField is not assigned in the Inspector!");
        }
    }

    // Method to play the chromosome entered manually
    public void PlayManualChromosome()
    {
        if (manualChromosomeInput == null)
        {
            Debug.LogError("Cannot play: TMP_InputField is not assigned.");
            return;
        }

        string inputText = manualChromosomeInput.text;
        if (string.IsNullOrWhiteSpace(inputText))
        {
            Debug.LogWarning("Manual chromosome input is empty.");
            return;
        }

        //Parse the chromosome data from the input string
        List<float> parsedChromosomeData = new List<float>();
        string[] geneStrings = inputText.Split(','); // Split by comma

        foreach (string geneStr in geneStrings)
        {
            if (float.TryParse(geneStr.Trim(), out float geneValue)) // Trim whitespace and parse
            {
                parsedChromosomeData.Add(geneValue);
            }
            else
            {
                Debug.LogError($"Error parsing value: '{geneStr.Trim()}'. Please enter comma-separated numbers.");
                return; // Stop if parsing fails
            }
        }

        if (parsedChromosomeData.Count == 0)
        {
            Debug.LogError("Parsed chromosome data is empty after attempting to parse input.");
            return;
        }

        Debug.Log($"Playing manually entered chromosome with {parsedChromosomeData.Count} genes...");

        // Reset player position and state
        if (player != null) player.ResetPlayer();
        if (gm != null) gm.ResetGameState();
        else { Debug.LogError("GameManager is null, cannot reset state."); return; }


        //Split the parsed chromosome data
        int geneCount = parsedChromosomeData.Count;
        int rightSplit = Mathf.FloorToInt(geneCount * 0.6f);
        int jumpSplit = Mathf.FloorToInt(geneCount * 0.3f);

        List<float> roundedChromosome = parsedChromosomeData
            .Select(gene => Mathf.Round(gene * 100f) / 100f)
            .ToList();

        // Create and assign the chromosome based on the parsed data
        Chromosome manualChromosome = new Chromosome
        {
            RightTime = roundedChromosome.Take(rightSplit).ToList(),
            JumpTime = roundedChromosome.Skip(rightSplit).Take(jumpSplit).ToList(),
            LeftTime = roundedChromosome.Skip(rightSplit + jumpSplit).ToList()
        };

        if (player == null) { Debug.LogError("Player is null, cannot assign chromosome."); return; }
        player.InputChromosome = manualChromosome;

        Debug.Log($"Assigned manual chromosome to player: \n" +
                  $"RightTime ({manualChromosome.RightTime.Count} genes): {string.Join(", ", manualChromosome.RightTime)}\n" +
                  $"JumpTime ({manualChromosome.JumpTime.Count} genes): {string.Join(", ", manualChromosome.JumpTime)}\n" +
                  $"LeftTime ({manualChromosome.LeftTime.Count} genes): {string.Join(", ", manualChromosome.LeftTime)}");

        player.controlmode = Controls.AI;
        StartCoroutine(SimulateChromosome(manualChromosome));
    }

    // Coroutine to simulate the assigned chromosome
    private IEnumerator SimulateChromosome(Chromosome playedChromosome)
    {
        if (player == null || gm == null || gar == null)
        {
            Debug.LogError("Cannot simulate: Required component (Player, GM, or GAR) is null.");
            yield break;
        }

        float bestProgress = player.transform.position.x;
        Time.timeScale = 1f;
        float simulationTime = gar.simulationTime > 0 ? gar.simulationTime : 40f;
        float startTime = Time.time;

        Debug.Log($"Starting simulation for manual chromosome. Duration: {simulationTime}s");

        while (Time.time - startTime < simulationTime)
        {
            yield return new WaitForFixedUpdate();

            if (player == null || gm == null)
            {
                Debug.LogError("Player or GameManager became null during simulation.");
                yield break;
            }

            if (player.transform.position.x > bestProgress)
            {
                bestProgress = player.transform.position.x;
            }

            if (gm.reachedGoal)
            {
                Debug.Log("Simulation ended: Goal Reached.");
                break;
            }
            if (player.isDead)
            {
                Debug.Log("Simulation ended: Player Died.");
                break;
            }
        }

        if (player == null || gm == null)
        {
            Debug.LogError("Player or GameManager is null after simulation.");
        }
        else
        {
            int finalCoins = gm.coinCount;
            float totalTime = gm.gameTimer;
            bool reachedGoal = gm.reachedGoal;
            bool died = player.isDead;

            Debug.Log($"--- Manual Simulation Complete ---\n" +
                    $"Best Progress Reached: {bestProgress:F2}\n" +
                    $"Coins Collected: {finalCoins}\n" +
                    $"Total Time Taken: {totalTime:F2} seconds\n" +
                    $"Reached Goal: {reachedGoal}\n" +
                    $"Died: {died}");
        }
    }
}