using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NewportHustle.Core;

namespace NewportHustle.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD Panels")]
        public GameObject mobileHUD;
        public GameObject desktopHUD;
        public GameObject pauseMenu;
        public GameObject inventoryPanel;
        public GameObject mapPanel;
        
        [Header("Player Stats UI")]
        public Slider healthBar;
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI respectText;
        public TextMeshProUGUI levelText;
        
        [Header("Mission UI")]
        public GameObject missionPanel;
        public TextMeshProUGUI missionTitleText;
        public TextMeshProUGUI missionObjectiveText;
        public Slider missionProgressBar;
        
        [Header("Location UI")]
        public TextMeshProUGUI locationText;
        public TextMeshProUGUI streetText;
        public GameObject locationNotificationPanel;
        
        [Header("Time and Weather")]
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI weatherText;
        public Image weatherIcon;
        
        [Header("Mobile Controls")]
        public GameObject mobileControlsPanel;
        public Joystick movementJoystick;
        public Joystick cameraJoystick;
        public Button jumpButton;
        public Button sprintButton;
        public Button interactButton;
        public Button menuButton;
        
        [Header("Interaction UI")]
        public GameObject interactionPrompt;
        public TextMeshProUGUI interactionText;
        
        [Header("Notification System")]
        public GameObject notificationPanel;
        public TextMeshProUGUI notificationText;
        public float notificationDuration = 3f;
        
        [Header("Newport-Specific UI")]
        public GameObject riverActivityPanel;
        public GameObject businessStatusPanel;
        public GameObject communityRespectPanel;
        
        // Internal state
        private bool isPaused = false;
        private bool isInventoryOpen = false;
        private bool isMapOpen = false;
        private Coroutine notificationCoroutine;
        
        void Start()
        {
            InitializeHUD();
            SetupPlatformSpecificUI();
            SubscribeToEvents();
        }
        
        void Update()
        {
            UpdateMobileInput();
            HandleHUDInput();
        }
        
        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeHUD()
        {
            // Hide all panels initially
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);
            if (locationNotificationPanel != null) locationNotificationPanel.SetActive(false);
            
            // Setup mobile controls
            SetupMobileControls();
            
            // Initialize mission panel
            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
        }
        
        private void SetupPlatformSpecificUI()
        {
            #if UNITY_ANDROID || UNITY_IOS
            // Mobile platform
            if (mobileHUD != null) mobileHUD.SetActive(true);
            if (desktopHUD != null) desktopHUD.SetActive(false);
            if (mobileControlsPanel != null) mobileControlsPanel.SetActive(true);
            #else
            // Desktop platform
            if (mobileHUD != null) mobileHUD.SetActive(false);
            if (desktopHUD != null) desktopHUD.SetActive(true);
            if (mobileControlsPanel != null) mobileControlsPanel.SetActive(false);
            #endif
        }
        
        private void SetupMobileControls()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(() => SetMobileButton(0, true));
            }
            
            if (sprintButton != null)
            {
                // Sprint button uses pointer down/up events for hold functionality
                var sprintEventTrigger = sprintButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                pointerDown.callback.AddListener((data) => SetMobileButton(1, true));
                sprintEventTrigger.triggers.Add(pointerDown);
                
                var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
                pointerUp.callback.AddListener((data) => SetMobileButton(1, false));
                sprintEventTrigger.triggers.Add(pointerUp);
            }
            
            if (interactButton != null)
            {
                interactButton.onClick.AddListener(() => SetMobileButton(2, true));
            }
            
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(TogglePauseMenu);
            }
        }
        
        private void SetMobileButton(int buttonIndex, bool pressed)
        {
            var playerController = FindObjectOfType<NewportHustle.Characters.PlayerController>();
            if (playerController != null)
            {
                playerController.SetMobileButton(buttonIndex, pressed);
            }
        }
        
        private void UpdateMobileInput()
        {
            var playerController = FindObjectOfType<NewportHustle.Characters.PlayerController>();
            if (playerController == null) return;
            
            // Update joystick inputs
            if (movementJoystick != null)
            {
                Vector2 moveInput = new Vector2(movementJoystick.Horizontal, movementJoystick.Vertical);
                playerController.SetMobileMovementInput(moveInput);
            }
            
            if (cameraJoystick != null)
            {
                Vector2 lookInput = new Vector2(cameraJoystick.Horizontal, cameraJoystick.Vertical);
                playerController.SetMobileLookInput(lookInput);
            }
        }
        
        private void HandleHUDInput()
        {
            // Handle keyboard shortcuts for desktop
            #if !UNITY_ANDROID && !UNITY_IOS
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenu();
            }
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventory();
            }
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMap();
            }
            #endif
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to game manager events
            GameManager.OnPlayerHealthChanged += UpdateHealthBar;
            GameManager.OnPlayerMoneyChanged += UpdateMoneyDisplay;
            GameManager.OnPlayerRespectChanged += UpdateRespectDisplay;
            GameManager.OnPlayerLevelChanged += UpdateLevelDisplay;
            GameManager.OnGameStateChanged += OnGameStateChanged;
            
            // Subscribe to time cycle events
            if (FindObjectOfType<TimeCycle>() != null)
            {
                TimeCycle.OnTimeChanged += UpdateTimeDisplay;
                TimeCycle.OnWeatherChanged += UpdateWeatherDisplay;
            }
            
            // Subscribe to zone manager events
            var zoneManager = FindObjectOfType<NewportHustle.World.ZoneManager>();
            if (zoneManager != null)
            {
                zoneManager.OnZoneChanged += OnZoneChanged;
                zoneManager.OnStreetEntered += OnStreetEntered;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            // Unsubscribe from events to prevent memory leaks
            GameManager.OnPlayerHealthChanged -= UpdateHealthBar;
            GameManager.OnPlayerMoneyChanged -= UpdateMoneyDisplay;
            GameManager.OnPlayerRespectChanged -= UpdateRespectDisplay;
            GameManager.OnPlayerLevelChanged -= UpdateLevelDisplay;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
            
            TimeCycle.OnTimeChanged -= UpdateTimeDisplay;
            TimeCycle.OnWeatherChanged -= UpdateWeatherDisplay;
        }
        
        // UI Update Methods
        private void UpdateHealthBar(float health)
        {
            if (healthBar != null)
            {
                healthBar.value = health / 100f;
            }
        }
        
        private void UpdateMoneyDisplay(float money)
        {
            if (moneyText != null)
            {
                moneyText.text = $"${money:F0}";
            }
        }
        
        private void UpdateRespectDisplay(float respect)
        {
            if (respectText != null)
            {
                respectText.text = $"Respect: {respect:F0}%";
            }
        }
        
        private void UpdateLevelDisplay(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level {level}";
            }
        }
        
        private void UpdateTimeDisplay(float time)
        {
            if (timeText != null)
            {
                TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
                if (timeCycle != null)
                {
                    timeText.text = timeCycle.GetFormattedTime();
                }
            }
        }
        
        private void UpdateWeatherDisplay(WeatherType weather)
        {
            if (weatherText != null)
            {
                weatherText.text = weather.ToString();
            }
            
            // Update weather icon if available
            if (weatherIcon != null)
            {
                // Load appropriate weather icon sprite
                UpdateWeatherIcon(weather);
            }
        }
        
        private void UpdateWeatherIcon(WeatherType weather)
        {
            // This would load the appropriate weather icon sprite
            // For now, just change the color to represent different weather
            switch (weather)
            {
                case WeatherType.Clear:
                    weatherIcon.color = Color.yellow;
                    break;
                case WeatherType.Cloudy:
                    weatherIcon.color = Color.gray;
                    break;
                case WeatherType.Rain:
                    weatherIcon.color = Color.blue;
                    break;
                case WeatherType.Storm:
                    weatherIcon.color = Color.red;
                    break;
                case WeatherType.Fog:
                    weatherIcon.color = Color.white;
                    break;
            }
        }
        
        // Event Handlers
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Paused:
                    if (pauseMenu != null) pauseMenu.SetActive(true);
                    break;
                case GameState.Playing:
                    if (pauseMenu != null) pauseMenu.SetActive(false);
                    break;
            }
        }
        
        private void OnZoneChanged(NewportHustle.World.NewportZone previousZone, NewportHustle.World.NewportZone newZone)
        {
            var zoneManager = FindObjectOfType<NewportHustle.World.ZoneManager>();
            if (zoneManager != null)
            {
                var zoneData = zoneManager.GetZoneData(newZone);
                if (zoneData != null && locationText != null)
                {
                    locationText.text = zoneData.zoneName;
                    ShowLocationNotification(zoneData.zoneName, zoneData.description);
                }
            }
        }
        
        private void OnStreetEntered(string streetName)
        {
            if (streetText != null)
            {
                streetText.text = streetName;
            }
        }
        
        // UI Panel Management
        public void TogglePauseMenu()
        {
            isPaused = !isPaused;
            
            if (isPaused)
            {
                GameManager.Instance.SetGameState(GameState.Paused);
            }
            else
            {
                GameManager.Instance.SetGameState(GameState.Playing);
            }
        }
        
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
            }
        }
        
        public void ToggleMap()
        {
            isMapOpen = !isMapOpen;
            if (mapPanel != null)
            {
                mapPanel.SetActive(isMapOpen);
            }
        }
        
        // Mission UI
        public void ShowMission(string title, string objective, float progress)
        {
            if (missionPanel != null)
            {
                missionPanel.SetActive(true);
                
                if (missionTitleText != null)
                    missionTitleText.text = title;
                
                if (missionObjectiveText != null)
                    missionObjectiveText.text = objective;
                
                if (missionProgressBar != null)
                    missionProgressBar.value = progress;
            }
        }
        
        public void HideMission()
        {
            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
        }
        
        // Interaction UI
        public void ShowInteractionPrompt(string text)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                if (interactionText != null)
                {
                    interactionText.text = text;
                }
            }
        }
        
        public void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        
        // Notification System
        public void ShowNotification(string message)
        {
            if (notificationPanel != null && notificationText != null)
            {
                notificationText.text = message;
                notificationPanel.SetActive(true);
                
                // Stop previous notification coroutine if running
                if (notificationCoroutine != null)
                {
                    StopCoroutine(notificationCoroutine);
                }
                
                notificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
            }
        }
        
        private System.Collections.IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSeconds(notificationDuration);
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
        
        // Location Notification
        private void ShowLocationNotification(string locationName, string description)
        {
            if (locationNotificationPanel != null)
            {
                locationNotificationPanel.SetActive(true);
                
                // Find text components in notification panel
                TextMeshProUGUI[] texts = locationNotificationPanel.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = locationName;
                    texts[1].text = description;
                }
                
                StartCoroutine(HideLocationNotificationAfterDelay());
            }
        }
        
        private System.Collections.IEnumerator HideLocationNotificationAfterDelay()
        {
            yield return new WaitForSeconds(4f);
            if (locationNotificationPanel != null)
            {
                locationNotificationPanel.SetActive(false);
            }
        }
        
        // Newport-Specific UI Methods
        public void UpdateBusinessStatus(string businessName, float income, string status)
        {
            if (businessStatusPanel != null)
            {
                // Update business status display
                TextMeshProUGUI[] texts = businessStatusPanel.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = businessName;
                    texts[1].text = $"${income:F0}/day";
                    texts[2].text = status;
                }
            }
        }
        
        public void UpdateCommunityRespect(float overallRespect, string standing)
        {
            if (communityRespectPanel != null)
            {
                TextMeshProUGUI[] texts = communityRespectPanel.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = $"Community Standing: {standing}";
                    texts[1].text = $"Overall Respect: {overallRespect:F0}%";
                }
            }
        }
        
        // Button handlers for UI
        public void OnResumeButtonClicked()
        {
            TogglePauseMenu();
        }
        
        public void OnSaveButtonClicked()
        {
            GameManager.Instance.SaveGame();
            ShowNotification("Game Saved");
        }
        
        public void OnLoadButtonClicked()
        {
            GameManager.Instance.LoadGame();
            ShowNotification("Game Loaded");
        }
        
        public void OnQuitButtonClicked()
        {
            GameManager.Instance.QuitGame();
        }
    }
}