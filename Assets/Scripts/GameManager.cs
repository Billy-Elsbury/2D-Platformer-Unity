using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text timerText;


    [SerializeField] private PlayerController playerController;

    private int coinCount = 0;
    private bool isGameOver = false;
    private Vector3 playerPosition;

    //Level Complete

    [SerializeField] GameObject levelCompletePanel;
    [SerializeField] TMP_Text leveCompletePanelTitle;
    [SerializeField] TMP_Text levelCompleteCoins;
    [SerializeField] TMP_Text levelCompleteTime;


    private int totalCoins = 0;
    private float gameTimer = 0f;

    private void Awake()
    {

        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        UpdateGUI();
        UIManager.instance.fadeFromBlack = true;
        playerPosition = playerController.transform.position;

        FindTotalPickups();

        Time.timeScale = 4.0f;
    }


    private void Update()
    {
        gameTimer += Time.deltaTime;
        timerText.text = gameTimer.ToString("F2");

    }

    public void IncrementCoinCount()
    {
        coinCount++;
        UpdateGUI();
    }

    private void UpdateGUI()
    {
        coinText.text = coinCount.ToString();
        
    }

    public void Death()
    {
        if (!isGameOver)
        {
            // Disable Mobile Controls
            UIManager.instance.DisableMobileControls();
            // Initiate screen fade
            UIManager.instance.fadeToBlack = true;

            // Disable the player object
            playerController.gameObject.SetActive(false);

            // Start death coroutine to wait and then respawn the player
            StartCoroutine(DeathCoroutine());

            // Update game state
            isGameOver = true;

            // Log death message
            Debug.Log("Died");

            gameTimer = 0f;
        }
    }
 
    public void FindTotalPickups()
    {
        pickup[] pickups = GameObject.FindObjectsOfType<pickup>();

        foreach (pickup pickupObject in pickups)
        {
            if (pickupObject.pt == pickup.pickupType.coin)
            {
                totalCoins += 1;
            }  
        }
    }

    public void LevelComplete()
    {
        levelCompletePanel.SetActive(true);
        leveCompletePanelTitle.text = "LEVEL COMPLETE";
        levelCompleteCoins.text = "COINS COLLECTED: "+ coinCount.ToString() +" / " + totalCoins.ToString();
        levelCompleteTime.text = "Time Taken: " + gameTimer.ToString("F2");
    }
   
    public IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f);
        playerController.transform.position = playerPosition;

        // Wait for 2 seconds
        yield return new WaitForSeconds(1f);

        // Check if the game is still over (in case player respawns earlier)
        if (isGameOver)
        {
            SceneManager.LoadScene(1); 
        }
    }
}
