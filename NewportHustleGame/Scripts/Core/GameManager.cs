using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

namespace NewportHustle.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game State")]
        public GameState currentGameState = GameState.MainMenu;
        public bool isPaused = false;
        
        [Header("Player Stats")]
        public float playerHealth = 100f;
        public float playerMoney = 500f;
        public int playerLevel = 1;
        public float playerRespect = 0f;
        
        [Header("Game Settings")]
        public bool enableMobileControls = true;
        public float gameSpeed = 1f;
        
        // Singleton pattern
        public static GameManager Instance { get; private set; }
        
        // Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<float> OnPlayerHealthChanged;
        public static event Action<float> OnPlayerMoneyChanged;
        public static event Action<int> OnPlayerLevelChanged;
        public static event Action<float> OnPlayerRespectChanged;
        
        // Game systems
        private SaveSystem saveSystem;
        private TimeCycle timeCycle;
        
        void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            SetGameState(GameState.MainMenu);
        }
        
        void Update()
        {
            HandleInput();
        }
        
        private void InitializeGame()
        {
            saveSystem = GetComponent<SaveSystem>();
            timeCycle = GetComponent<TimeCycle>();
            
            // Initialize mobile controls if on mobile platform
            #if UNITY_ANDROID || UNITY_IOS
            enableMobileControls = true;
            #else
            enableMobileControls = false;
            #endif
            
            Application.targetFrameRate = 60;
        }
        
        private void HandleInput()
        {
            // Pause/Resume game
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
        
        public void SetGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                currentGameState = newState;
                OnGameStateChanged?.Invoke(newState);
                
                switch (newState)
                {
                    case GameState.MainMenu:
                        Time.timeScale = 1f;
                        break;
                    case GameState.Playing:
                        Time.timeScale = gameSpeed;
                        break;
                    case GameState.Paused:
                        Time.timeScale = 0f;
                        break;
                    case GameState.GameOver:
                        Time.timeScale = 0f;
                        break;
                }
            }
        }
        
        public void TogglePause()
        {
            if (currentGameState == GameState.Playing)
            {
                isPaused = true;
                SetGameState(GameState.Paused);
            }
            else if (currentGameState == GameState.Paused)
            {
                isPaused = false;
                SetGameState(GameState.Playing);
            }
        }
        
        // Player stat management
        public void UpdatePlayerHealth(float amount)
        {
            playerHealth = Mathf.Clamp(playerHealth + amount, 0f, 100f);
            OnPlayerHealthChanged?.Invoke(playerHealth);
            
            if (playerHealth <= 0f)
            {
                GameOver();
            }
        }
        
        public void UpdatePlayerMoney(float amount)
        {
            playerMoney += amount;
            OnPlayerMoneyChanged?.Invoke(playerMoney);
        }
        
        public void UpdatePlayerRespect(float amount)
        {
            playerRespect = Mathf.Clamp(playerRespect + amount, 0f, 100f);
            OnPlayerRespectChanged?.Invoke(playerRespect);
        }
        
        public void LevelUp()
        {
            playerLevel++;
            OnPlayerLevelChanged?.Invoke(playerLevel);
            UpdatePlayerRespect(10f); // Gain respect on level up
        }
        
        public void StartNewGame()
        {
            // Reset player stats
            playerHealth = 100f;
            playerMoney = 500f;
            playerLevel = 1;
            playerRespect = 0f;
            
            // Load main game scene
            SceneManager.LoadScene("NewportCity");
            SetGameState(GameState.Playing);
        }
        
        public void LoadGame()
        {
            if (saveSystem != null)
            {
                saveSystem.LoadGame();
            }
        }
        
        public void SaveGame()
        {
            if (saveSystem != null)
            {
                saveSystem.SaveGame();
            }
        }
        
        public void GameOver()
        {
            SetGameState(GameState.GameOver);
            // Show game over screen
        }
        
        public void QuitGame()
        {
            SaveGame();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Newport-specific methods
        public void EnterNewportArea(string areaName)
        {
            Debug.Log($"Entering {areaName} area in Newport");
            // Handle area-specific logic
        }
        
        public bool CanAfford(float cost)
        {
            return playerMoney >= cost;
        }
        
        public bool SpendMoney(float amount)
        {
            if (CanAfford(amount))
            {
                UpdatePlayerMoney(-amount);
                return true;
            }
            return false;
        }
    }
    
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Loading
    }
}