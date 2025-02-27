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

    // UI
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TMP_Text leveCompletePanelTitle;
    [SerializeField] private TMP_Text levelCompleteCoins;
    [SerializeField] private TMP_Text levelCompleteTime;

    // Player
    [SerializeField] private PlayerController playerController;
    private Vector3 playerPosition;

    internal bool isGameOver = false;
    internal bool reachedGoal = false;
    internal bool wasDeadThisRun = false;

    // Game Stats
    internal int coinCount = 0;
    internal int totalCoins = 0;
    internal float gameTimer = 0f;

    // Runtime Performance 
    private int gameSpeed = 100;
    private int frameRate = 60;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = frameRate;
    }

    private void Start()
    {
        UpdateGUI();
        UIManager.instance.fadeFromBlack = true;
        playerPosition = playerController.transform.position;

        FindTotalPickups();

        Time.timeScale = gameSpeed;
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

    public void ResetGameState()
    {
        gameTimer = 0f;
        isGameOver = false;
        reachedGoal = false;
        coinCount = 0;
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
        reachedGoal = true;
        levelCompletePanel.SetActive(true);
        leveCompletePanelTitle.text = "LEVEL COMPLETE";
        levelCompleteCoins.text = "COINS COLLECTED: "+ coinCount.ToString() +" / " + totalCoins.ToString();
        levelCompleteTime.text = "Time Taken: " + gameTimer.ToString("F2");
    }
}
