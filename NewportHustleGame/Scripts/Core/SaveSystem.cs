using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace NewportHustle.Core
{
    public class SaveSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        public string saveFileName = "newport_hustle_save.json";
        public bool autoSave = true;
        public float autoSaveInterval = 300f; // 5 minutes
        
        private string savePath;
        private float autoSaveTimer;
        
        // Events
        public static event Action OnGameSaved;
        public static event Action OnGameLoaded;
        
        void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            autoSaveTimer = autoSaveInterval;
        }
        
        void Update()
        {
            if (autoSave && GameManager.Instance.currentGameState == GameState.Playing)
            {
                autoSaveTimer -= Time.deltaTime;
                if (autoSaveTimer <= 0f)
                {
                    SaveGame();
                    autoSaveTimer = autoSaveInterval;
                }
            }
        }
        
        public void SaveGame()
        {
            try
            {
                SaveData saveData = CreateSaveData();
                string jsonData = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(savePath, jsonData);
                
                OnGameSaved?.Invoke();
                Debug.Log("Game saved successfully to: " + savePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save game: " + e.Message);
            }
        }
        
        public void LoadGame()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    string jsonData = File.ReadAllText(savePath);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                    ApplySaveData(saveData);
                    
                    OnGameLoaded?.Invoke();
                    Debug.Log("Game loaded successfully from: " + savePath);
                }
                else
                {
                    Debug.LogWarning("No save file found at: " + savePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load game: " + e.Message);
            }
        }
        
        public bool HasSaveFile()
        {
            return File.Exists(savePath);
        }
        
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log("Save file deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete save file: " + e.Message);
            }
        }
        
        private SaveData CreateSaveData()
        {
            GameManager gm = GameManager.Instance;
            SaveData saveData = new SaveData
            {
                // Player data
                playerHealth = gm.playerHealth,
                playerMoney = gm.playerMoney,
                playerLevel = gm.playerLevel,
                playerRespect = gm.playerRespect,
                
                // World data
                currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                playerPosition = GetPlayerPosition(),
                
                // Time data
                gameTime = GetGameTime(),
                daysPassed = GetDaysPassed(),
                
                // Mission progress
                completedMissions = GetCompletedMissions(),
                activeMissions = GetActiveMissions(),
                
                // Newport-specific data
                unlockedAreas = GetUnlockedAreas(),
                relationships = GetRelationships(),
                businessOwned = GetOwnedBusinesses(),
                
                // Timestamp
                saveTimestamp = DateTime.Now.ToBinary()
            };
            
            return saveData;
        }
        
        private void ApplySaveData(SaveData saveData)
        {
            GameManager gm = GameManager.Instance;
            
            // Apply player data
            gm.playerHealth = saveData.playerHealth;
            gm.playerMoney = saveData.playerMoney;
            gm.playerLevel = saveData.playerLevel;
            gm.playerRespect = saveData.playerRespect;
            
            // Apply world data
            SetPlayerPosition(saveData.playerPosition);
            
            // Apply time data
            SetGameTime(saveData.gameTime);
            SetDaysPassed(saveData.daysPassed);
            
            // Apply mission progress
            SetCompletedMissions(saveData.completedMissions);
            SetActiveMissions(saveData.activeMissions);
            
            // Apply Newport-specific data
            SetUnlockedAreas(saveData.unlockedAreas);
            SetRelationships(saveData.relationships);
            SetOwnedBusinesses(saveData.businessOwned);
            
            // Load the correct scene if different
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != saveData.currentScene)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.currentScene);
            }
        }
        
        // Helper methods for getting/setting game data
        private Vector3 GetPlayerPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform.position : Vector3.zero;
        }
        
        private void SetPlayerPosition(Vector3 position)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = position;
            }
        }
        
        private float GetGameTime()
        {
            TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
            return timeCycle != null ? timeCycle.GetCurrentTime() : 0f;
        }
        
        private void SetGameTime(float time)
        {
            TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
            if (timeCycle != null)
            {
                timeCycle.SetTime(time);
            }
        }
        
        private int GetDaysPassed()
        {
            TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
            return timeCycle != null ? timeCycle.GetDaysPassed() : 0;
        }
        
        private void SetDaysPassed(int days)
        {
            TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
            if (timeCycle != null)
            {
                timeCycle.SetDaysPassed(days);
            }
        }
        
        private List<string> GetCompletedMissions()
        {
            // TODO: Implement mission system integration
            return new List<string>();
        }
        
        private void SetCompletedMissions(List<string> missions)
        {
            // TODO: Implement mission system integration
        }
        
        private List<string> GetActiveMissions()
        {
            // TODO: Implement mission system integration
            return new List<string>();
        }
        
        private void SetActiveMissions(List<string> missions)
        {
            // TODO: Implement mission system integration
        }
        
        private List<string> GetUnlockedAreas()
        {
            // Newport areas that are unlocked
            return new List<string> { "Downtown", "Riverfront" }; // Default unlocked areas
        }
        
        private void SetUnlockedAreas(List<string> areas)
        {
            // TODO: Implement area unlock system
        }
        
        private Dictionary<string, float> GetRelationships()
        {
            // Character relationships/reputation
            return new Dictionary<string, float>();
        }
        
        private void SetRelationships(Dictionary<string, float> relationships)
        {
            // TODO: Implement relationship system
        }
        
        private List<string> GetOwnedBusinesses()
        {
            // Businesses owned by player in Newport
            return new List<string>();
        }
        
        private void SetOwnedBusinesses(List<string> businesses)
        {
            // TODO: Implement business ownership system
        }
    }
    
    [System.Serializable]
    public class SaveData
    {
        // Player data
        public float playerHealth;
        public float playerMoney;
        public int playerLevel;
        public float playerRespect;
        
        // World data
        public string currentScene;
        public Vector3 playerPosition;
        
        // Time data
        public float gameTime;
        public int daysPassed;
        
        // Mission progress
        public List<string> completedMissions = new List<string>();
        public List<string> activeMissions = new List<string>();
        
        // Newport-specific data
        public List<string> unlockedAreas = new List<string>();
        public Dictionary<string, float> relationships = new Dictionary<string, float>();
        public List<string> businessOwned = new List<string>();
        
        // Metadata
        public long saveTimestamp;
    }
}