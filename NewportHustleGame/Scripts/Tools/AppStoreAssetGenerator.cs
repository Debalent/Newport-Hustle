using UnityEngine;
using UnityEditor;
using System.IO;

namespace NewportHustle.Tools
{
    /// <summary>
    /// Editor tool to generate app store assets from the Newport Hustle logo
    /// Creates properly sized and formatted assets for iOS and Android stores
    /// </summary>
    public class AppStoreAssetGenerator : EditorWindow
    {
        [Header("Source Logo")]
        [SerializeField] private Texture2D sourceLogo;
        
        [Header("Asset Generation Settings")]
        [SerializeField] private bool generateAppIcons = true;
        [SerializeField] private bool generateScreenshots = true;
        [SerializeField] private bool generatePromotional = true;
        [SerializeField] private bool preserveTransparency = true;
        
        [Header("Background Options")]
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        [SerializeField] private bool useGradientBackground = true;
        [SerializeField] private Color gradientTopColor = new Color(0.1f, 0.3f, 0.7f, 1f);
        [SerializeField] private Color gradientBottomColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        
        [Header("Output Settings")]
        [SerializeField] private string outputPath = "Assets/UI/Branding/Generated/";
        [SerializeField] private TextureFormat outputFormat = TextureFormat.RGBA32;
        [SerializeField] private int compressionQuality = 100;
        
        // Asset specifications
        private readonly AssetSpec[] appIconSpecs = {
            new AssetSpec("iOS_AppIcon_1024", 1024, 1024, false),
            new AssetSpec("Android_AppIcon_512", 512, 512, true),
            new AssetSpec("iOS_AppIcon_180", 180, 180, false), // iPhone
            new AssetSpec("iOS_AppIcon_120", 120, 120, false), // iPhone
            new AssetSpec("iOS_AppIcon_167", 167, 167, false), // iPad
            new AssetSpec("iOS_AppIcon_152", 152, 152, false), // iPad
        };
        
        private readonly AssetSpec[] promotionalSpecs = {
            new AssetSpec("GooglePlay_Feature", 1024, 500, true),
            new AssetSpec("Banner_1920x1080", 1920, 1080, true),
            new AssetSpec("Social_Square", 1080, 1080, true),
            new AssetSpec("Social_Vertical", 1080, 1920, true),
            new AssetSpec("Splash_Screen", 2048, 2048, true),
            new AssetSpec("Loading_Screen", 1024, 1024, true),
        };
        
        [MenuItem("Newport Hustle/Generate App Store Assets")]
        public static void ShowWindow()
        {
            AppStoreAssetGenerator window = GetWindow<AppStoreAssetGenerator>();
            window.titleContent = new GUIContent("App Store Asset Generator");
            window.Show();
        }
        
        void OnGUI()
        {
            GUILayout.Label("Newport Hustle App Store Asset Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Source logo selection
            GUILayout.Label("Source Logo", EditorStyles.boldLabel);
            sourceLogo = (Texture2D)EditorGUILayout.ObjectField("Logo Texture", sourceLogo, typeof(Texture2D), false);
            
            if (sourceLogo == null)
            {
                EditorGUILayout.HelpBox("Please assign the Newport Hustle logo texture to generate assets.", MessageType.Warning);
                return;
            }
            
            GUILayout.Space(10);
            
            // Generation options
            GUILayout.Label("Generation Options", EditorStyles.boldLabel);
            generateAppIcons = EditorGUILayout.Toggle("Generate App Icons", generateAppIcons);
            generateScreenshots = EditorGUILayout.Toggle("Generate Screenshot Templates", generateScreenshots);
            generatePromotional = EditorGUILayout.Toggle("Generate Promotional Assets", generatePromotional);
            preserveTransparency = EditorGUILayout.Toggle("Preserve Transparency", preserveTransparency);
            
            GUILayout.Space(10);
            
            // Background options
            GUILayout.Label("Background Options", EditorStyles.boldLabel);
            backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
            useGradientBackground = EditorGUILayout.Toggle("Use Gradient Background", useGradientBackground);
            
            if (useGradientBackground)
            {
                gradientTopColor = EditorGUILayout.ColorField("Gradient Top", gradientTopColor);
                gradientBottomColor = EditorGUILayout.ColorField("Gradient Bottom", gradientBottomColor);
            }
            
            GUILayout.Space(10);
            
            // Output settings
            GUILayout.Label("Output Settings", EditorStyles.boldLabel);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            outputFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", outputFormat);
            compressionQuality = EditorGUILayout.IntSlider("Quality", compressionQuality, 1, 100);
            
            GUILayout.Space(20);
            
            // Generate buttons
            if (GUILayout.Button("Generate All Assets", GUILayout.Height(30)))
            {
                GenerateAllAssets();
            }
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate App Icons Only"))
            {
                GenerateAppIcons();
            }
            if (GUILayout.Button("Generate Promotional Only"))
            {
                GeneratePromotionalAssets();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Asset preview
            if (sourceLogo != null)
            {
                GUILayout.Label("Source Logo Preview", EditorStyles.boldLabel);
                GUILayout.Label($"Size: {sourceLogo.width}x{sourceLogo.height}");
                GUILayout.Label($"Format: {sourceLogo.format}");
                
                float previewSize = 100f;
                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(previewRect, sourceLogo);
            }
        }
        
        private void GenerateAllAssets()
        {
            if (!ValidateInputs()) return;
            
            EnsureOutputDirectory();
            
            if (generateAppIcons) GenerateAppIcons();
            if (generatePromotional) GeneratePromotionalAssets();
            if (generateScreenshots) GenerateScreenshotTemplates();
            
            AssetDatabase.Refresh();
            Debug.Log("All Newport Hustle app store assets generated successfully!");
        }
        
        private void GenerateAppIcons()
        {
            foreach (var spec in appIconSpecs)
            {
                GenerateAsset(spec);
            }
            Debug.Log("App icons generated successfully!");
        }
        
        private void GeneratePromotionalAssets()
        {
            foreach (var spec in promotionalSpecs)
            {
                GenerateAsset(spec);
            }
            Debug.Log("Promotional assets generated successfully!");
        }
        
        private void GenerateScreenshotTemplates()
        {
            // Generate screenshot templates with logo placement
            var screenshotSpecs = new AssetSpec[] {
                new AssetSpec("iPhone_Screenshot_1290x2796", 1290, 2796, true),
                new AssetSpec("iPhone_Screenshot_1179x2556", 1179, 2556, true),
                new AssetSpec("iPad_Screenshot_2048x2732", 2048, 2732, true),
                new AssetSpec("Android_Phone_1080x1920", 1080, 1920, true),
                new AssetSpec("Android_Tablet_1920x1080", 1920, 1080, true),
            };
            
            foreach (var spec in screenshotSpecs)
            {
                GenerateScreenshotTemplate(spec);
            }
            Debug.Log("Screenshot templates generated successfully!");
        }
        
        private void GenerateAsset(AssetSpec spec)
        {
            // Create render texture
            RenderTexture renderTexture = new RenderTexture(spec.width, spec.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            
            // Create temporary camera
            GameObject cameraGO = new GameObject("TempCamera");
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.targetTexture = renderTexture;
            camera.clearFlags = CameraClearFlags.SolidColor;
            
            // Set background
            if (spec.useTransparency && preserveTransparency)
            {
                camera.backgroundColor = Color.clear;
                camera.clearFlags = CameraClearFlags.Nothing;
            }
            else
            {
                camera.backgroundColor = useGradientBackground ? gradientBottomColor : backgroundColor;
            }
            
            // Create logo object
            GameObject logoGO = new GameObject("Logo");
            SpriteRenderer logoRenderer = logoGO.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = Sprite.Create(sourceLogo, new Rect(0, 0, sourceLogo.width, sourceLogo.height), Vector2.one * 0.5f);
            
            // Scale logo appropriately
            float logoScale = CalculateLogoScale(spec, sourceLogo);
            logoGO.transform.localScale = Vector3.one * logoScale;
            
            // Add gradient background if enabled
            if (useGradientBackground && !spec.useTransparency)
            {
                CreateGradientBackground(camera);
            }
            
            // Render
            camera.Render();
            
            // Save texture
            SaveRenderTexture(renderTexture, spec.name);
            
            // Cleanup
            DestroyImmediate(cameraGO);
            DestroyImmediate(logoGO);
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
        
        private void GenerateScreenshotTemplate(AssetSpec spec)
        {
            // Create a screenshot template with logo and UI elements positioned
            RenderTexture renderTexture = new RenderTexture(spec.width, spec.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            
            // Create camera
            GameObject cameraGO = new GameObject("ScreenshotCamera");
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.targetTexture = renderTexture;
            camera.backgroundColor = backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;
            
            // Create Newport-themed background
            CreateNewportBackground(camera);
            
            // Add logo in corner
            GameObject logoGO = new GameObject("Logo");
            SpriteRenderer logoRenderer = logoGO.AddComponent<SpriteRenderer>();
            logoRenderer.sprite = Sprite.Create(sourceLogo, new Rect(0, 0, sourceLogo.width, sourceLogo.height), Vector2.one * 0.5f);
            logoRenderer.sortingOrder = 10;
            
            // Position logo in top corner
            logoGO.transform.position = new Vector3(-8f, 8f, 0f);
            logoGO.transform.localScale = Vector3.one * 2f;
            
            // Render
            camera.Render();
            
            // Save
            SaveRenderTexture(renderTexture, spec.name + "_Template");
            
            // Cleanup
            DestroyImmediate(cameraGO);
            DestroyImmediate(logoGO);
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
        
        private float CalculateLogoScale(AssetSpec spec, Texture2D logo)
        {
            // Calculate appropriate scale to fit logo in the asset
            float targetSize = Mathf.Min(spec.width, spec.height) * 0.7f; // Use 70% of smaller dimension
            float logoSize = Mathf.Max(logo.width, logo.height);
            return (targetSize / logoSize) * 0.1f; // Unity units scaling
        }
        
        private void CreateGradientBackground(Camera camera)
        {
            // Create a simple gradient background
            GameObject gradientGO = new GameObject("GradientBackground");
            SpriteRenderer gradientRenderer = gradientGO.AddComponent<SpriteRenderer>();
            
            // Create gradient texture
            Texture2D gradientTexture = new Texture2D(2, 256);
            for (int y = 0; y < 256; y++)
            {
                Color color = Color.Lerp(gradientBottomColor, gradientTopColor, y / 255f);
                gradientTexture.SetPixel(0, y, color);
                gradientTexture.SetPixel(1, y, color);
            }
            gradientTexture.Apply();
            
            gradientRenderer.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, 2, 256), Vector2.one * 0.5f);
            gradientRenderer.sortingOrder = -10;
            gradientGO.transform.localScale = new Vector3(20f, 20f, 1f);
        }
        
        private void CreateNewportBackground(Camera camera)
        {
            // Create a simple Newport-themed background for screenshots
            GameObject backgroundGO = new GameObject("NewportBackground");
            SpriteRenderer backgroundRenderer = backgroundGO.AddComponent<SpriteRenderer>();
            
            // Create simple background texture
            Texture2D backgroundTexture = new Texture2D(100, 100);
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    Color color = Color.Lerp(backgroundColor, gradientTopColor, noise * 0.3f);
                    backgroundTexture.SetPixel(x, y, color);
                }
            }
            backgroundTexture.Apply();
            
            backgroundRenderer.sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, 100, 100), Vector2.one * 0.5f);
            backgroundRenderer.sortingOrder = -20;
            backgroundGO.transform.localScale = new Vector3(30f, 30f, 1f);
        }
        
        private void SaveRenderTexture(RenderTexture renderTexture, string fileName)
        {
            // Read render texture
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            
            // Encode to PNG
            byte[] pngData = texture.EncodeToPNG();
            
            // Save file
            string filePath = Path.Combine(outputPath, fileName + ".png");
            File.WriteAllBytes(filePath, pngData);
            
            // Cleanup
            DestroyImmediate(texture);
            
            Debug.Log($"Generated: {filePath}");
        }
        
        private bool ValidateInputs()
        {
            if (sourceLogo == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a source logo texture.", "OK");
                return false;
            }
            
            if (string.IsNullOrEmpty(outputPath))
            {
                EditorUtility.DisplayDialog("Error", "Please specify an output path.", "OK");
                return false;
            }
            
            return true;
        }
        
        private void EnsureOutputDirectory()
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }
        
        private struct AssetSpec
        {
            public string name;
            public int width;
            public int height;
            public bool useTransparency;
            
            public AssetSpec(string name, int width, int height, bool useTransparency)
            {
                this.name = name;
                this.width = width;
                this.height = height;
                this.useTransparency = useTransparency;
            }
        }
    }
}