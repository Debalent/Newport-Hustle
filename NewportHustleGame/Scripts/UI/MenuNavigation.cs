using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NewportHustle.Core;

namespace NewportHustle.UI
{
    public class MenuNavigation : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainMenuPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public GameObject saveLoadPanel;
        public GameObject characterCreatorPanel;
        
        [Header("Main Menu Buttons")]
        public Button newGameButton;
        public Button continueButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        
        [Header("Settings Panels")]
        public GameObject graphicsSettingsPanel;
        public GameObject audioSettingsPanel;
        public GameObject controlsSettingsPanel;
        
        [Header("Graphics Settings")]
        public Dropdown qualityDropdown;
        public Toggle fullscreenToggle;
        public Slider brightnessSlider;
        public Toggle vsyncToggle;
        
        [Header("Audio Settings")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider voiceVolumeSlider;
        public Toggle muteToggle;
        
        [Header("Controls Settings")]
        public Slider mouseSensitivitySlider;
        public Toggle invertYToggle;
        public Slider touchSensitivitySlider;
        public Toggle hapticFeedbackToggle;
        
        [Header("Newport Branding")]
        public TextMeshProUGUI gameTitle;
        public Image logoImage;
        public Image backgroundImage;
        public TextMeshProUGUI gameSubtitle;
        public Image newportLogo;
        public TextMeshProUGUI versionText;
        
        [Header("Background Elements")]
        public Image backgroundImage;
        public GameObject newportScenery;
        public ParticleSystem atmosphereParticles;
        
        private bool isInitialized = false;
        
        void Start()
        {
            InitializeMenu();
        }
        
        private void InitializeMenu()
        {
            if (isInitialized) return;
            
            SetupNewportBranding();
            SetupMenuButtons();
            SetupSettingsControls();
            LoadPlayerPreferences();
            CheckSaveFileAvailability();
            
            // Show main menu initially
            ShowMainMenu();
            
            isInitialized = true;
        }
        
        private void SetupNewportBranding()
        {
            if (gameTitle != null)
            {
                gameTitle.text = "NEWPORT HUSTLE";
            }
            
            if (gameSubtitle != null)
            {
                gameSubtitle.text = "A Mobile Adventure in Small-Town Arkansas";
            }
            
            if (versionText != null)
            {
                versionText.text = $"Version {Application.version}";
            }
            
            // Apply Newport Hustle logo through BrandingManager
            if (BrandingManager.Instance != null)
            {
                if (logoImage != null)
                {
                    BrandingManager.Instance.ApplyLogoToImage(logoImage, BrandingManager.LogoType.Primary);
                }
                
                if (backgroundImage != null)
                {
                    BrandingManager.Instance.ApplyBrandColorToImage(backgroundImage, BrandingManager.BrandColorType.Background);
                }
            }
            
            // Setup Newport-themed background
            SetupNewportBackground();
        }
        
        private void SetupNewportBackground()
        {
            // This would set up a beautiful Newport, AR background
            // For now, just ensure the background elements are active
            if (newportScenery != null)
            {
                newportScenery.SetActive(true);
            }
            
            if (atmosphereParticles != null)
            {
                atmosphereParticles.Play();
            }
        }
        
        private void SetupMenuButtons()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(StartNewGame);
            }
            
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueGame);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ShowSettings);
            }
            
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(ShowCredits);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }
        
        private void SetupSettingsControls()
        {
            // Graphics settings
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
            
            if (brightnessSlider != null)
            {
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            }
            
            if (vsyncToggle != null)
            {
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }
            
            // Audio settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }
            
            if (muteToggle != null)
            {
                muteToggle.onValueChanged.AddListener(OnMuteChanged);
            }
            
            // Controls settings
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }
            
            if (invertYToggle != null)
            {
                invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
            }
            
            if (touchSensitivitySlider != null)
            {
                touchSensitivitySlider.onValueChanged.AddListener(OnTouchSensitivityChanged);
            }
            
            if (hapticFeedbackToggle != null)
            {
                hapticFeedbackToggle.onValueChanged.AddListener(OnHapticFeedbackChanged);
            }
        }
        
        private void LoadPlayerPreferences()
        {
            // Load graphics settings
            if (qualityDropdown != null)
            {
                qualityDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
            }
            
            if (brightnessSlider != null)
            {
                brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1.0f);
            }
            
            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
            }
            
            // Load audio settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.9f);
            }
            
            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 1.0f);
            }
            
            if (muteToggle != null)
            {
                muteToggle.isOn = PlayerPrefs.GetInt("Mute", 0) == 1;
            }
            
            // Load controls settings
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
            }
            
            if (invertYToggle != null)
            {
                invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
            }
            
            if (touchSensitivitySlider != null)
            {
                touchSensitivitySlider.value = PlayerPrefs.GetFloat("TouchSensitivity", 1.0f);
            }
            
            if (hapticFeedbackToggle != null)
            {
                hapticFeedbackToggle.isOn = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
            }
        }
        
        private void CheckSaveFileAvailability()
        {
            if (continueButton != null)
            {
                SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
                if (saveSystem != null)
                {
                    continueButton.interactable = saveSystem.HasSaveFile();
                }
                else
                {
                    // Check for save file manually
                    bool hasSave = PlayerPrefs.HasKey("PlayerCharacterData");
                    continueButton.interactable = hasSave;
                }
            }
        }
        
        // Navigation Methods
        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }
        
        public void ShowSettings()
        {
            HideAllPanels();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
            ShowGraphicsSettings();
        }
        
        public void ShowGraphicsSettings()
        {
            HideSettingsPanels();
            if (graphicsSettingsPanel != null)
            {
                graphicsSettingsPanel.SetActive(true);
            }
        }
        
        public void ShowAudioSettings()
        {
            HideSettingsPanels();
            if (audioSettingsPanel != null)
            {
                audioSettingsPanel.SetActive(true);
            }
        }
        
        public void ShowControlsSettings()
        {
            HideSettingsPanels();
            if (controlsSettingsPanel != null)
            {
                controlsSettingsPanel.SetActive(true);
            }
        }
        
        public void ShowCredits()
        {
            HideAllPanels();
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
        }
        
        public void ShowSaveLoad()
        {
            HideAllPanels();
            if (saveLoadPanel != null)
            {
                saveLoadPanel.SetActive(true);
            }
        }
        
        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
            if (characterCreatorPanel != null) characterCreatorPanel.SetActive(false);
        }
        
        private void HideSettingsPanels()
        {
            if (graphicsSettingsPanel != null) graphicsSettingsPanel.SetActive(false);
            if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
            if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
        }
        
        // Game Control Methods
        public void StartNewGame()
        {
            // Show character creator first
            if (characterCreatorPanel != null)
            {
                HideAllPanels();
                characterCreatorPanel.SetActive(true);
            }
            else
            {
                // Directly start game if no character creator
                LoadGameScene();
            }
        }
        
        public void ContinueGame()
        {
            LoadGameScene();
            
            // Load save after scene loads
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame();
            }
        }
        
        private void LoadGameScene()
        {
            // Load the main game scene
            SceneManager.LoadScene("NewportCity");
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Settings Event Handlers
        private void OnQualityChanged(int value)
        {
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt("GraphicsQuality", value);
        }
        
        private void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
        }
        
        private void OnBrightnessChanged(float value)
        {
            // Apply brightness setting
            PlayerPrefs.SetFloat("Brightness", value);
            // This would typically adjust screen brightness or gamma
        }
        
        private void OnVSyncChanged(bool value)
        {
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            // Apply to music audio sources
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // Apply to SFX audio sources
        }
        
        private void OnVoiceVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("VoiceVolume", value);
            // Apply to voice audio sources
        }
        
        private void OnMuteChanged(bool value)
        {
            AudioListener.pause = value;
            PlayerPrefs.SetInt("Mute", value ? 1 : 0);
        }
        
        private void OnMouseSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
        }
        
        private void OnInvertYChanged(bool value)
        {
            PlayerPrefs.SetInt("InvertY", value ? 1 : 0);
        }
        
        private void OnTouchSensitivityChanged(float value)
        {
            PlayerPrefs.SetFloat("TouchSensitivity", value);
        }
        
        private void OnHapticFeedbackChanged(bool value)
        {
            PlayerPrefs.SetInt("HapticFeedback", value ? 1 : 0);
        }
        
        // Newport-specific menu features
        public void ShowNewportInfo()
        {
            // Show information about Newport, Arkansas
            string newportInfo = @"
            Newport, Arkansas
            
            Founded in 1875, Newport sits on the banks of the White River in Jackson County, Arkansas. 
            Known for its rich history, friendly community, and beautiful river activities, Newport offers 
            the perfect setting for both relaxation and adventure.
            
            The game features real street names and locations from Newport, while all characters and 
            specific businesses are fictional to protect privacy and allow for creative storytelling.
            
            Experience the charm of small-town Arkansas in Newport Hustle!
            ";
            
            Debug.Log(newportInfo);
            // This would show in an actual info panel
        }
        
        public void ApplyNewportTheme()
        {
            // Apply Newport-specific visual theme
            if (backgroundImage != null)
            {
                // Load Newport-themed background
            }
        }
    }
}