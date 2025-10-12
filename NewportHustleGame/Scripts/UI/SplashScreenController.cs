using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace NewportHustle.UI
{
    /// <summary>
    /// Controls the main splash screen and cover screen with Newport Hustle branding
    /// Features logo animation, version info, and smooth transitions
    /// </summary>
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Logo and Branding")]
        [SerializeField] private Image logoImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite newportHustleLogo;
        [SerializeField] private Color backgroundStartColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        [SerializeField] private Color backgroundEndColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI gameTitle;
        [SerializeField] private TextMeshProUGUI subtitle;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private TextMeshProUGUI pressAnyKeyText;
        [SerializeField] private TextMeshProUGUI copyrightText;
        
        [Header("Animation Settings")]
        [SerializeField] private float logoAnimationDuration = 3f;
        [SerializeField] private float textFadeDelay = 1f;
        [SerializeField] private float textFadeDuration = 1.5f;
        [SerializeField] private float backgroundFadeDuration = 4f;
        [SerializeField] private bool autoAdvanceAfterTime = true;
        [SerializeField] private float autoAdvanceDelay = 8f;
        
        [Header("Scene Transition")]
        [SerializeField] private string nextSceneName = "MainMenu";
        [SerializeField] private float transitionFadeTime = 1f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip splashAudio;
        [SerializeField] private float audioVolume = 0.7f;
        
        // Private variables
        private bool animationComplete = false;
        private bool canAdvance = false;
        private bool isTransitioning = false;
        private Coroutine autoAdvanceCoroutine;
        
        // Newport-themed content
        private readonly string[] subtitleOptions = {
            "Welcome to Newport, Arkansas",
            "Where Small Town Dreams Come Alive",
            "Your Story Begins on the White River",
            "Discover the Heart of Arkansas"
        };
        
        void Start()
        {
            InitializeSplashScreen();
            StartCoroutine(PlaySplashSequence());
        }
        
        void Update()
        {
            // Check for any input to advance
            if (canAdvance && !isTransitioning && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                AdvanceToNextScene();
            }
        }
        
        /// <summary>
        /// Initialize the splash screen elements
        /// </summary>
        private void InitializeSplashScreen()
        {
            // Set up logo
            if (logoImage != null && newportHustleLogo != null)
            {
                logoImage.sprite = newportHustleLogo;
                logoImage.color = new Color(1f, 1f, 1f, 0f);
                logoImage.transform.localScale = Vector3.one * 0.5f;
            }
            
            // Set up background
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundStartColor;
            }
            
            // Set up text elements
            SetupTextElement(gameTitle, "NEWPORT HUSTLE", 0f);
            SetupTextElement(subtitle, GetRandomSubtitle(), 0f);
            SetupTextElement(versionText, $"Version {Application.version}", 0f);
            SetupTextElement(pressAnyKeyText, "Press any key to continue", 0f);
            SetupTextElement(copyrightText, $"© {System.DateTime.Now.Year} Newport Hustle Team", 0f);
            
            // Set up audio
            if (audioSource != null && splashAudio != null)
            {
                audioSource.clip = splashAudio;
                audioSource.volume = audioVolume;
                audioSource.loop = false;
            }
        }
        
        /// <summary>
        /// Set up individual text elements
        /// </summary>
        private void SetupTextElement(TextMeshProUGUI textElement, string content, float alpha)
        {
            if (textElement != null)
            {
                textElement.text = content;
                Color color = textElement.color;
                color.a = alpha;
                textElement.color = color;
            }
        }
        
        /// <summary>
        /// Get a random subtitle for variety
        /// </summary>
        private string GetRandomSubtitle()
        {
            return subtitleOptions[Random.Range(0, subtitleOptions.Length)];
        }
        
        /// <summary>
        /// Main splash screen animation sequence
        /// </summary>
        private IEnumerator PlaySplashSequence()
        {
            // Play splash audio
            if (audioSource != null && splashAudio != null)
            {
                audioSource.Play();
            }
            
            // Start background color transition
            StartCoroutine(AnimateBackgroundColor());
            
            // Animate logo entrance
            yield return StartCoroutine(AnimateLogo());
            
            // Wait a moment
            yield return new WaitForSeconds(textFadeDelay);
            
            // Fade in text elements
            yield return StartCoroutine(AnimateTextElements());
            
            // Animation complete - allow user input
            animationComplete = true;
            canAdvance = true;
            
            // Start press any key animation
            StartCoroutine(AnimatePressAnyKey());
            
            // Auto-advance after delay if enabled
            if (autoAdvanceAfterTime)
            {
                autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
            }
        }
        
        /// <summary>
        /// Animate the Newport Hustle logo
        /// </summary>
        private IEnumerator AnimateLogo()
        {
            if (logoImage == null) yield break;
            
            float elapsedTime = 0f;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one;
            Color startColor = new Color(1f, 1f, 1f, 0f);
            Color endColor = Color.white;
            
            while (elapsedTime < logoAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / logoAnimationDuration;
                
                // Use easing for smooth animation
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                
                logoImage.transform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
                logoImage.color = Color.Lerp(startColor, endColor, easedProgress);
                
                yield return null;
            }
            
            logoImage.transform.localScale = endScale;
            logoImage.color = endColor;
        }
        
        /// <summary>
        /// Animate background color transition
        /// </summary>
        private IEnumerator AnimateBackgroundColor()
        {
            if (backgroundImage == null) yield break;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < backgroundFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / backgroundFadeDuration;
                
                backgroundImage.color = Color.Lerp(backgroundStartColor, backgroundEndColor, progress);
                
                yield return null;
            }
            
            backgroundImage.color = backgroundEndColor;
        }
        
        /// <summary>
        /// Animate text elements fading in
        /// </summary>
        private IEnumerator AnimateTextElements()
        {
            TextMeshProUGUI[] textElements = { gameTitle, subtitle, versionText, copyrightText };
            
            foreach (var textElement in textElements)
            {
                if (textElement != null)
                {
                    StartCoroutine(FadeInText(textElement, textFadeDuration * 0.7f));
                    yield return new WaitForSeconds(0.2f); // Stagger the animations
                }
            }
            
            // Wait for all text to finish fading
            yield return new WaitForSeconds(textFadeDuration);
        }
        
        /// <summary>
        /// Fade in a text element
        /// </summary>
        private IEnumerator FadeInText(TextMeshProUGUI textElement, float duration)
        {
            if (textElement == null) yield break;
            
            float elapsedTime = 0f;
            Color startColor = textElement.color;
            Color endColor = startColor;
            startColor.a = 0f;
            endColor.a = 1f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                textElement.color = Color.Lerp(startColor, endColor, progress);
                
                yield return null;
            }
            
            textElement.color = endColor;
        }
        
        /// <summary>
        /// Animate the "Press any key" text with pulsing effect
        /// </summary>
        private IEnumerator AnimatePressAnyKey()
        {
            if (pressAnyKeyText == null) yield break;
            
            // First fade it in
            yield return StartCoroutine(FadeInText(pressAnyKeyText, 1f));
            
            // Then pulse it
            while (canAdvance && !isTransitioning)
            {
                yield return StartCoroutine(PulseText(pressAnyKeyText, 1.5f));
            }
        }
        
        /// <summary>
        /// Create a pulsing effect for text
        /// </summary>
        private IEnumerator PulseText(TextMeshProUGUI textElement, float pulseDuration)
        {
            if (textElement == null) yield break;
            
            float elapsedTime = 0f;
            Color originalColor = textElement.color;
            
            while (elapsedTime < pulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / pulseDuration;
                
                float alpha = 0.5f + 0.5f * Mathf.Sin(progress * Mathf.PI * 2f);
                Color newColor = originalColor;
                newColor.a = alpha;
                textElement.color = newColor;
                
                yield return null;
            }
            
            textElement.color = originalColor;
        }
        
        /// <summary>
        /// Auto-advance coroutine
        /// </summary>
        private IEnumerator AutoAdvanceCoroutine()
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            
            if (canAdvance && !isTransitioning)
            {
                AdvanceToNextScene();
            }
        }
        
        /// <summary>
        /// Advance to the next scene
        /// </summary>
        private void AdvanceToNextScene()
        {
            if (isTransitioning) return;
            
            isTransitioning = true;
            canAdvance = false;
            
            // Stop auto-advance coroutine
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
            }
            
            StartCoroutine(TransitionToNextScene());
        }
        
        /// <summary>
        /// Handle scene transition with fade effect
        /// </summary>
        private IEnumerator TransitionToNextScene()
        {
            // You could add a fade-out effect here
            yield return new WaitForSeconds(transitionFadeTime);
            
            // Load next scene
            if (LoadingScreenController.Instance != null)
            {
                LoadingScreenController.Instance.LoadScene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        
        /// <summary>
        /// Skip the splash screen immediately
        /// </summary>
        public void SkipSplash()
        {
            if (!isTransitioning)
            {
                StopAllCoroutines();
                AdvanceToNextScene();
            }
        }
        
        /// <summary>
        /// Set the next scene to load
        /// </summary>
        public void SetNextScene(string sceneName)
        {
            nextSceneName = sceneName;
        }
        
        void OnValidate()
        {
            // Auto-assign components if not set
            if (logoImage == null)
                logoImage = transform.Find("Logo")?.GetComponent<Image>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
    }
}