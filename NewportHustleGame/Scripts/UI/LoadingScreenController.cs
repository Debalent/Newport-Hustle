using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace NewportHustle.UI
{
    /// <summary>
    /// Controls loading screens with Newport Hustle branding and progress display
    /// Features the Newport Hustle logo prominently with smooth animations
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("Logo and Branding")]
        [SerializeField] private Image logoImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite newportHustleLogo;
        [SerializeField] private Color brandColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Newport blue
        
        [Header("Loading Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI loadingStatusText;
        [SerializeField] private TextMeshProUGUI tipsText;
        
        [Header("Animation Settings")]
        [SerializeField] private float logoFadeInTime = 2f;
        [SerializeField] private float progressUpdateSpeed = 2f;
        [SerializeField] private float minimumLoadTime = 3f;
        [SerializeField] private bool enableLogoAnimation = true;
        
        [Header("Newport Themed Tips")]
        [SerializeField] private string[] newportTips = {
            "Newport, Arkansas sits along the beautiful White River",
            "Explore the historic downtown area for hidden opportunities",
            "Build relationships with locals to unlock new missions",
            "The fishing spots around Newport offer peaceful moments",
            "Every character in Newport has their own story to tell",
            "Respect the community and it will respect you back",
            "Small town values lead to big opportunities",
            "The White River has been Newport's lifeline for generations"
        };
        
        // Private variables
        private float currentProgress = 0f;
        private float targetProgress = 0f;
        private string targetSceneName;
        private AsyncOperation sceneLoadOperation;
        private bool isLoading = false;
        private float loadStartTime;
        
        // Static instance for easy access
        public static LoadingScreenController Instance { get; private set; }
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeLoadingScreen();
        }
        
        void Start()
        {
            if (enableLogoAnimation)
            {
                StartCoroutine(AnimateLogo());
            }
        }
        
        void Update()
        {
            if (isLoading)
            {
                UpdateLoadingProgress();
            }
        }
        
        /// <summary>
        /// Initialize the loading screen with Newport Hustle branding
        /// </summary>
        private void InitializeLoadingScreen()
        {
            // Set up logo
            if (logoImage != null && newportHustleLogo != null)
            {
                logoImage.sprite = newportHustleLogo;
                logoImage.color = enableLogoAnimation ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
            
            // Set up background with brand color
            if (backgroundImage != null)
            {
                backgroundImage.color = brandColor;
            }
            
            // Initialize progress elements
            if (progressBar != null)
            {
                progressBar.value = 0f;
            }
            
            if (progressText != null)
            {
                progressText.text = "0%";
            }
            
            if (loadingStatusText != null)
            {
                loadingStatusText.text = "Initializing Newport Hustle...";
            }
            
            // Show random Newport tip
            ShowRandomTip();
        }
        
        /// <summary>
        /// Start loading a new scene with the loading screen
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning("Already loading a scene!");
                return;
            }
            
            targetSceneName = sceneName;
            gameObject.SetActive(true);
            StartCoroutine(LoadSceneAsync());
        }
        
        /// <summary>
        /// Coroutine to handle async scene loading
        /// </summary>
        private IEnumerator LoadSceneAsync()
        {
            isLoading = true;
            loadStartTime = Time.time;
            currentProgress = 0f;
            targetProgress = 0f;
            
            // Update status
            if (loadingStatusText != null)
            {
                loadingStatusText.text = "Loading Newport...";
            }
            
            // Start loading the scene
            sceneLoadOperation = SceneManager.LoadSceneAsync(targetSceneName);
            sceneLoadOperation.allowSceneActivation = false;
            
            // Update progress while loading
            while (!sceneLoadOperation.isDone)
            {
                // Unity's progress goes from 0 to 0.9, then jumps to 1
                float realProgress = Mathf.Clamp01(sceneLoadOperation.progress / 0.9f);
                targetProgress = realProgress * 100f;
                
                // Ensure minimum load time for branding visibility
                float timeElapsed = Time.time - loadStartTime;
                if (realProgress >= 1f && timeElapsed >= minimumLoadTime)
                {
                    if (loadingStatusText != null)
                    {
                        loadingStatusText.text = "Entering Newport...";
                    }
                    
                    sceneLoadOperation.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // Final progress update
            targetProgress = 100f;
            yield return new WaitForSeconds(0.5f); // Brief pause to show completion
            
            isLoading = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Update the loading progress display
        /// </summary>
        private void UpdateLoadingProgress()
        {
            // Smooth progress bar animation
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, progressUpdateSpeed * Time.deltaTime);
            
            if (progressBar != null)
            {
                progressBar.value = currentProgress / 100f;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(currentProgress)}%";
            }
        }
        
        /// <summary>
        /// Animate the Newport Hustle logo on screen
        /// </summary>
        private IEnumerator AnimateLogo()
        {
            if (logoImage == null) yield break;
            
            float elapsedTime = 0f;
            Color startColor = new Color(1f, 1f, 1f, 0f);
            Color endColor = Color.white;
            
            while (elapsedTime < logoFadeInTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / logoFadeInTime);
                logoImage.color = Color.Lerp(startColor, endColor, alpha);
                
                // Add slight scale animation
                float scale = Mathf.Lerp(0.8f, 1f, alpha);
                logoImage.transform.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            logoImage.color = endColor;
            logoImage.transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Display a random Newport-themed tip
        /// </summary>
        private void ShowRandomTip()
        {
            if (tipsText != null && newportTips.Length > 0)
            {
                int randomIndex = Random.Range(0, newportTips.Length);
                tipsText.text = $"Tip: {newportTips[randomIndex]}";
            }
        }
        
        /// <summary>
        /// Set a custom loading status message
        /// </summary>
        /// <param name="status">Status message to display</param>
        public void SetLoadingStatus(string status)
        {
            if (loadingStatusText != null)
            {
                loadingStatusText.text = status;
            }
        }
        
        /// <summary>
        /// Set custom progress (for non-scene loading operations)
        /// </summary>
        /// <param name="progress">Progress value (0-100)</param>
        public void SetProgress(float progress)
        {
            targetProgress = Mathf.Clamp(progress, 0f, 100f);
        }
        
        /// <summary>
        /// Show the loading screen without loading a scene
        /// </summary>
        public void ShowLoadingScreen()
        {
            gameObject.SetActive(true);
            ShowRandomTip();
        }
        
        /// <summary>
        /// Hide the loading screen
        /// </summary>
        public void HideLoadingScreen()
        {
            gameObject.SetActive(false);
            isLoading = false;
        }
        
        /// <summary>
        /// Check if currently loading
        /// </summary>
        public bool IsLoading()
        {
            return isLoading;
        }
        
        void OnValidate()
        {
            // Auto-assign components if not set
            if (logoImage == null)
                logoImage = transform.Find("Logo")?.GetComponent<Image>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (progressBar == null)
                progressBar = GetComponentInChildren<Slider>();
            
            if (progressText == null)
                progressText = transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
        }
    }
}