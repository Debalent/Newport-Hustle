using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NewportHustle.UI
{
    /// <summary>
    /// Manages app store assets and branding configuration for Newport Hustle
    /// Handles logo display, aspect ratios, and branding consistency
    /// </summary>
    public class BrandingManager : MonoBehaviour
    {
        [Header("Logo Assets")]
        [SerializeField] private Sprite primaryLogo;          // Main Newport Hustle logo
        [SerializeField] private Sprite squareLogo;           // 1:1 aspect ratio for app icons
        [SerializeField] private Sprite horizontalLogo;       // Wide format for banners
        [SerializeField] private Sprite verticalLogo;         // Tall format for portraits
        
        [Header("App Store Assets")]
        [SerializeField] private Sprite appIcon1024;          // 1024x1024 app store icon
        [SerializeField] private Sprite appIcon512;           // 512x512 app icon
        [SerializeField] private Sprite playStoreFeature;     // 1024x500 Google Play feature graphic
        [SerializeField] private Sprite appStoreScreenshot;   // Various sizes for screenshots
        
        [Header("Brand Colors")]
        [SerializeField] private Color primaryBrandColor = new Color(0.2f, 0.4f, 0.8f, 1f);     // Newport blue
        [SerializeField] private Color secondaryBrandColor = new Color(0.1f, 0.2f, 0.4f, 1f);   // Dark blue
        [SerializeField] private Color accentColor = new Color(0.8f, 0.6f, 0.2f, 1f);          // Arkansas gold
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        
        [Header("Typography")]
        [SerializeField] private TMP_FontAsset primaryFont;
        [SerializeField] private TMP_FontAsset titleFont;
        [SerializeField] private TMP_FontAsset bodyFont;
        
        [Header("Branding Text")]
        [SerializeField] private string gameTitle = "Newport Hustle";
        [SerializeField] private string tagline = "Your Story Begins in Newport";
        [SerializeField] private string description = "Experience the heart of Arkansas in this mobile adventure";
        
        // Singleton instance
        public static BrandingManager Instance { get; private set; }
        
        // App Store asset specifications
        public struct AppStoreSpecs
        {
            public const int APP_ICON_SIZE = 1024;
            public const int SMALL_APP_ICON_SIZE = 512;
            
            // Google Play Store
            public const int PLAY_FEATURE_WIDTH = 1024;
            public const int PLAY_FEATURE_HEIGHT = 500;
            public const int PLAY_ICON_SIZE = 512;
            
            // iOS App Store
            public const int IOS_SCREENSHOT_WIDTH = 1290;
            public const int IOS_SCREENSHOT_HEIGHT = 2796;
            public const int IOS_ICON_SIZE = 1024;
            
            // Universal
            public const int BANNER_WIDTH = 1920;
            public const int BANNER_HEIGHT = 1080;
        }
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeBranding();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Initialize branding system
        /// </summary>
        private void InitializeBranding()
        {
            Debug.Log("Newport Hustle Branding Manager initialized");
            
            // Validate logo assets
            ValidateLogoAssets();
            
            // Apply branding to current scene
            ApplyBrandingToScene();
        }
        
        /// <summary>
        /// Validate that all required logo assets are present
        /// </summary>
        private void ValidateLogoAssets()
        {
            if (primaryLogo == null)
                Debug.LogWarning("Primary logo not assigned in BrandingManager");
            
            if (appIcon1024 == null)
                Debug.LogWarning("1024x1024 app icon not assigned - required for app stores");
            
            if (playStoreFeature == null)
                Debug.LogWarning("Google Play feature graphic not assigned");
        }
        
        /// <summary>
        /// Apply Newport Hustle branding to the current scene
        /// </summary>
        public void ApplyBrandingToScene()
        {
            // Find and brand logo images
            Image[] logoImages = FindObjectsOfType<Image>();
            foreach (var image in logoImages)
            {
                if (image.gameObject.name.ToLower().Contains("logo"))
                {
                    ApplyLogoToImage(image);
                }
                else if (image.gameObject.name.ToLower().Contains("background"))
                {
                    ApplyBrandColorToImage(image);
                }
            }
            
            // Find and brand text elements
            TextMeshProUGUI[] textElements = FindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in textElements)
            {
                ApplyBrandingToText(text);
            }
        }
        
        /// <summary>
        /// Apply appropriate logo to an Image component
        /// </summary>
        /// <param name="image">Image component to brand</param>
        /// <param name="logoType">Type of logo to apply</param>
        public void ApplyLogoToImage(Image image, LogoType logoType = LogoType.Primary)
        {
            if (image == null) return;
            
            Sprite logoToUse = GetLogoByType(logoType);
            
            if (logoToUse != null)
            {
                image.sprite = logoToUse;
                image.preserveAspect = true;
                
                // Ensure proper color
                image.color = Color.white;
            }
        }
        
        /// <summary>
        /// Get logo sprite by type
        /// </summary>
        /// <param name="logoType">Type of logo needed</param>
        /// <returns>Appropriate logo sprite</returns>
        public Sprite GetLogoByType(LogoType logoType)
        {
            switch (logoType)
            {
                case LogoType.Primary:
                    return primaryLogo;
                case LogoType.Square:
                    return squareLogo ?? primaryLogo;
                case LogoType.Horizontal:
                    return horizontalLogo ?? primaryLogo;
                case LogoType.Vertical:
                    return verticalLogo ?? primaryLogo;
                case LogoType.AppIcon:
                    return appIcon1024 ?? primaryLogo;
                default:
                    return primaryLogo;
            }
        }
        
        /// <summary>
        /// Apply brand colors to an Image component
        /// </summary>
        /// <param name="image">Image component to color</param>
        /// <param name="colorType">Type of brand color to apply</param>
        public void ApplyBrandColorToImage(Image image, BrandColorType colorType = BrandColorType.Primary)
        {
            if (image == null) return;
            
            Color colorToUse = GetBrandColor(colorType);
            image.color = colorToUse;
        }
        
        /// <summary>
        /// Get brand color by type
        /// </summary>
        /// <param name="colorType">Type of color needed</param>
        /// <returns>Brand color</returns>
        public Color GetBrandColor(BrandColorType colorType)
        {
            switch (colorType)
            {
                case BrandColorType.Primary:
                    return primaryBrandColor;
                case BrandColorType.Secondary:
                    return secondaryBrandColor;
                case BrandColorType.Accent:
                    return accentColor;
                case BrandColorType.Text:
                    return textColor;
                case BrandColorType.Background:
                    return backgroundColor;
                default:
                    return primaryBrandColor;
            }
        }
        
        /// <summary>
        /// Apply branding to text elements
        /// </summary>
        /// <param name="textElement">Text element to brand</param>
        public void ApplyBrandingToText(TextMeshProUGUI textElement)
        {
            if (textElement == null) return;
            
            // Apply appropriate font based on text content or tag
            if (textElement.gameObject.name.ToLower().Contains("title"))
            {
                if (titleFont != null) textElement.font = titleFont;
                textElement.color = textColor;
            }
            else if (textElement.gameObject.name.ToLower().Contains("body"))
            {
                if (bodyFont != null) textElement.font = bodyFont;
                textElement.color = textColor;
            }
            else
            {
                if (primaryFont != null) textElement.font = primaryFont;
                textElement.color = textColor;
            }
        }
        
        /// <summary>
        /// Create app store icon from primary logo
        /// </summary>
        /// <param name="size">Size of the icon (usually 1024 or 512)</param>
        /// <returns>Render texture for app icon</returns>
        public RenderTexture CreateAppIcon(int size = 1024)
        {
            if (primaryLogo == null) return null;
            
            RenderTexture iconTexture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32);
            iconTexture.Create();
            
            // Create temporary camera to render the icon
            GameObject tempCameraGO = new GameObject("IconCamera");
            Camera iconCamera = tempCameraGO.AddComponent<Camera>();
            iconCamera.orthographic = true;
            iconCamera.orthographicSize = 5f;
            iconCamera.targetTexture = iconTexture;
            iconCamera.backgroundColor = backgroundColor;
            iconCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // Create logo object
            GameObject logoGO = new GameObject("LogoIcon");
            SpriteRenderer logoRenderer = logoGO.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = primaryLogo;
            logoRenderer.sortingOrder = 1;
            
            // Position for proper framing
            logoGO.transform.position = new Vector3(0, 0, 0);
            logoGO.transform.localScale = Vector3.one * 8f; // Scale to fit icon
            
            // Render the icon
            iconCamera.Render();
            
            // Clean up
            DestroyImmediate(tempCameraGO);
            DestroyImmediate(logoGO);
            
            return iconTexture;
        }
        
        /// <summary>
        /// Get game information for app store listings
        /// </summary>
        /// <returns>Game info structure</returns>
        public GameInfo GetGameInfo()
        {
            return new GameInfo
            {
                title = gameTitle,
                tagline = tagline,
                description = description,
                version = Application.version,
                platform = Application.platform.ToString(),
                buildNumber = Application.buildGUID
            };
        }
        
        // Enums for logo and color types
        public enum LogoType
        {
            Primary,
            Square,
            Horizontal,
            Vertical,
            AppIcon
        }
        
        public enum BrandColorType
        {
            Primary,
            Secondary,
            Accent,
            Text,
            Background
        }
        
        // Structure for game information
        [System.Serializable]
        public struct GameInfo
        {
            public string title;
            public string tagline;
            public string description;
            public string version;
            public string platform;
            public string buildNumber;
        }
        
        void OnValidate()
        {
            // Validate color accessibility
            if (Vector3.Distance(new Vector3(primaryBrandColor.r, primaryBrandColor.g, primaryBrandColor.b),
                                new Vector3(textColor.r, textColor.g, textColor.b)) < 0.5f)
            {
                Debug.LogWarning("Brand colors may not have sufficient contrast for accessibility");
            }
        }
    }
}