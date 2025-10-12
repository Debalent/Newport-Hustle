using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace NewportHustle.Characters
{
    public class CharacterCreator : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject characterCreationPanel;
        public GameObject previewPanel;
        
        [Header("Character Preview")]
        public GameObject characterPreview;
        public Camera previewCamera;
        
        [Header("Customization UI")]
        public Slider skinColorSlider;
        public Slider heightSlider;
        public Slider buildSlider;
        public Dropdown hairStyleDropdown;
        public Slider hairColorSlider;
        public Dropdown clothingStyleDropdown;
        public Slider clothingColorSlider;
        public TMP_InputField nameInputField;
        
        [Header("Character Assets")]
        public List<GameObject> hairStyles = new List<GameObject>();
        public List<Material> skinMaterials = new List<Material>();
        public List<Material> hairMaterials = new List<Material>();
        public List<GameObject> clothingOptions = new List<GameObject>();
        public List<Material> clothingMaterials = new List<Material>();
        
        [Header("Newport Style Options")]
        public List<CharacterStyle> newportStyles = new List<CharacterStyle>();
        
        // Character components
        private SkinnedMeshRenderer characterRenderer;
        private GameObject currentHair;
        private GameObject currentClothing;
        private CharacterData currentCharacterData;
        
        // Character limits
        private const int MAX_NAME_LENGTH = 20;
        private const int MIN_NAME_LENGTH = 2;
        
        void Start()
        {
            InitializeCharacterCreator();
        }
        
        private void InitializeCharacterCreator()
        {
            // Setup UI listeners
            SetupUIListeners();
            
            // Initialize character preview
            SetupCharacterPreview();
            
            // Setup Newport-specific styles
            SetupNewportStyles();
            
            // Create default character
            CreateDefaultCharacter();
        }
        
        private void SetupUIListeners()
        {
            if (skinColorSlider != null)
            {
                skinColorSlider.onValueChanged.AddListener(OnSkinColorChanged);
            }
            
            if (heightSlider != null)
            {
                heightSlider.onValueChanged.AddListener(OnHeightChanged);
            }
            
            if (buildSlider != null)
            {
                buildSlider.onValueChanged.AddListener(OnBuildChanged);
            }
            
            if (hairStyleDropdown != null)
            {
                hairStyleDropdown.onValueChanged.AddListener(OnHairStyleChanged);
            }
            
            if (hairColorSlider != null)
            {
                hairColorSlider.onValueChanged.AddListener(OnHairColorChanged);
            }
            
            if (clothingStyleDropdown != null)
            {
                clothingStyleDropdown.onValueChanged.AddListener(OnClothingStyleChanged);
            }
            
            if (clothingColorSlider != null)
            {
                clothingColorSlider.onValueChanged.AddListener(OnClothingColorChanged);
            }
            
            if (nameInputField != null)
            {
                nameInputField.onValueChanged.AddListener(OnNameChanged);
            }
        }
        
        private void SetupCharacterPreview()
        {
            if (characterPreview != null)
            {
                characterRenderer = characterPreview.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            
            // Setup preview camera
            if (previewCamera != null)
            {
                previewCamera.targetTexture = RenderTexture.GetTemporary(512, 512);
            }
        }
        
        private void SetupNewportStyles()
        {
            // Newport-specific character styles
            newportStyles.Add(new CharacterStyle
            {
                styleName = "River Worker",
                description = "Practical clothing for working by the White River",
                hairStyleIndex = 0,
                clothingStyleIndex = 0,
                recommendedColors = new Color[] { Color.blue, Color.gray, Color.brown }
            });
            
            newportStyles.Add(new CharacterStyle
            {
                styleName = "Downtown Professional",
                description = "Business attire suitable for Newport's downtown area",
                hairStyleIndex = 1,
                clothingStyleIndex = 1,
                recommendedColors = new Color[] { Color.black, Color.white, Color.gray }
            });
            
            newportStyles.Add(new CharacterStyle
            {
                styleName = "Spa District Trendy",
                description = "Fashionable look popular in the spa district",
                hairStyleIndex = 2,
                clothingStyleIndex = 2,
                recommendedColors = new Color[] { Color.magenta, Color.cyan, Color.yellow }
            });
            
            newportStyles.Add(new CharacterStyle
            {
                styleName = "Barbers Row Classic",
                description = "Traditional style from Newport's barber district",
                hairStyleIndex = 0,
                clothingStyleIndex = 0,
                recommendedColors = new Color[] { Color.red, Color.blue, Color.green }
            });
        }
        
        private void CreateDefaultCharacter()
        {
            currentCharacterData = new CharacterData
            {
                characterName = "Newport Newcomer",
                skinColorIndex = 0,
                height = 1.0f,
                build = 0.5f,
                hairStyleIndex = 0,
                hairColorIndex = 0,
                clothingStyleIndex = 0,
                clothingColorIndex = 0
            };
            
            ApplyCharacterData(currentCharacterData);
        }
        
        // UI Event Handlers
        private void OnSkinColorChanged(float value)
        {
            int skinIndex = Mathf.FloorToInt(value * (skinMaterials.Count - 1));
            currentCharacterData.skinColorIndex = skinIndex;
            ApplySkinColor();
        }
        
        private void OnHeightChanged(float value)
        {
            currentCharacterData.height = Mathf.Lerp(0.8f, 1.2f, value);
            ApplyHeight();
        }
        
        private void OnBuildChanged(float value)
        {
            currentCharacterData.build = value;
            ApplyBuild();
        }
        
        private void OnHairStyleChanged(int value)
        {
            currentCharacterData.hairStyleIndex = value;
            ApplyHairStyle();
        }
        
        private void OnHairColorChanged(float value)
        {
            int colorIndex = Mathf.FloorToInt(value * (hairMaterials.Count - 1));
            currentCharacterData.hairColorIndex = colorIndex;
            ApplyHairColor();
        }
        
        private void OnClothingStyleChanged(int value)
        {
            currentCharacterData.clothingStyleIndex = value;
            ApplyClothing();
        }
        
        private void OnClothingColorChanged(float value)
        {
            int colorIndex = Mathf.FloorToInt(value * (clothingMaterials.Count - 1));
            currentCharacterData.clothingColorIndex = colorIndex;
            ApplyClothingColor();
        }
        
        private void OnNameChanged(string value)
        {
            // Validate name
            if (value.Length <= MAX_NAME_LENGTH)
            {
                currentCharacterData.characterName = value;
            }
            else
            {
                nameInputField.text = currentCharacterData.characterName;
            }
        }
        
        // Character Application Methods
        private void ApplyCharacterData(CharacterData data)
        {
            ApplySkinColor();
            ApplyHeight();
            ApplyBuild();
            ApplyHairStyle();
            ApplyHairColor();
            ApplyClothing();
            ApplyClothingColor();
            
            // Update UI to match data
            UpdateUIFromData(data);
        }
        
        private void ApplySkinColor()
        {
            if (characterRenderer != null && skinMaterials.Count > 0)
            {
                int index = Mathf.Clamp(currentCharacterData.skinColorIndex, 0, skinMaterials.Count - 1);
                Material[] materials = characterRenderer.materials;
                materials[0] = skinMaterials[index]; // Assuming skin is the first material
                characterRenderer.materials = materials;
            }
        }
        
        private void ApplyHeight()
        {
            if (characterPreview != null)
            {
                Vector3 scale = characterPreview.transform.localScale;
                scale.y = currentCharacterData.height;
                characterPreview.transform.localScale = scale;
            }
        }
        
        private void ApplyBuild()
        {
            if (characterPreview != null)
            {
                Vector3 scale = characterPreview.transform.localScale;
                float buildScale = Mathf.Lerp(0.8f, 1.2f, currentCharacterData.build);
                scale.x = buildScale;
                scale.z = buildScale;
                characterPreview.transform.localScale = scale;
            }
        }
        
        private void ApplyHairStyle()
        {
            // Remove current hair
            if (currentHair != null)
            {
                DestroyImmediate(currentHair);
            }
            
            // Apply new hair style
            if (hairStyles.Count > 0 && characterPreview != null)
            {
                int index = Mathf.Clamp(currentCharacterData.hairStyleIndex, 0, hairStyles.Count - 1);
                GameObject hairPrefab = hairStyles[index];
                
                if (hairPrefab != null)
                {
                    currentHair = Instantiate(hairPrefab, characterPreview.transform);
                    // Position hair on head
                    Transform headBone = FindBone(characterPreview.transform, "Head");
                    if (headBone != null)
                    {
                        currentHair.transform.SetParent(headBone);
                        currentHair.transform.localPosition = Vector3.zero;
                        currentHair.transform.localRotation = Quaternion.identity;
                    }
                }
            }
        }
        
        private void ApplyHairColor()
        {
            if (currentHair != null && hairMaterials.Count > 0)
            {
                int index = Mathf.Clamp(currentCharacterData.hairColorIndex, 0, hairMaterials.Count - 1);
                Renderer hairRenderer = currentHair.GetComponent<Renderer>();
                if (hairRenderer != null)
                {
                    hairRenderer.material = hairMaterials[index];
                }
            }
        }
        
        private void ApplyClothing()
        {
            // Remove current clothing
            if (currentClothing != null)
            {
                DestroyImmediate(currentClothing);
            }
            
            // Apply new clothing
            if (clothingOptions.Count > 0 && characterPreview != null)
            {
                int index = Mathf.Clamp(currentCharacterData.clothingStyleIndex, 0, clothingOptions.Count - 1);
                GameObject clothingPrefab = clothingOptions[index];
                
                if (clothingPrefab != null)
                {
                    currentClothing = Instantiate(clothingPrefab, characterPreview.transform);
                }
            }
        }
        
        private void ApplyClothingColor()
        {
            if (currentClothing != null && clothingMaterials.Count > 0)
            {
                int index = Mathf.Clamp(currentCharacterData.clothingColorIndex, 0, clothingMaterials.Count - 1);
                Renderer clothingRenderer = currentClothing.GetComponent<Renderer>();
                if (clothingRenderer != null)
                {
                    clothingRenderer.material = clothingMaterials[index];
                }
            }
        }
        
        private void UpdateUIFromData(CharacterData data)
        {
            if (nameInputField != null)
                nameInputField.text = data.characterName;
            
            if (skinColorSlider != null)
                skinColorSlider.value = (float)data.skinColorIndex / (skinMaterials.Count - 1);
            
            if (heightSlider != null)
                heightSlider.value = (data.height - 0.8f) / 0.4f;
            
            if (buildSlider != null)
                buildSlider.value = data.build;
            
            if (hairStyleDropdown != null)
                hairStyleDropdown.value = data.hairStyleIndex;
            
            if (hairColorSlider != null)
                hairColorSlider.value = (float)data.hairColorIndex / (hairMaterials.Count - 1);
            
            if (clothingStyleDropdown != null)
                clothingStyleDropdown.value = data.clothingStyleIndex;
            
            if (clothingColorSlider != null)
                clothingColorSlider.value = (float)data.clothingColorIndex / (clothingMaterials.Count - 1);
        }
        
        private Transform FindBone(Transform parent, string boneName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(boneName))
                    return child;
                
                Transform found = FindBone(child, boneName);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        // Public Methods
        public void ApplyNewportStyle(int styleIndex)
        {
            if (styleIndex >= 0 && styleIndex < newportStyles.Count)
            {
                CharacterStyle style = newportStyles[styleIndex];
                currentCharacterData.hairStyleIndex = style.hairStyleIndex;
                currentCharacterData.clothingStyleIndex = style.clothingStyleIndex;
                
                // Apply recommended color
                if (style.recommendedColors.Length > 0)
                {
                    Color recommendedColor = style.recommendedColors[0];
                    // Convert color to material index (simplified)
                    currentCharacterData.clothingColorIndex = 0;
                }
                
                ApplyCharacterData(currentCharacterData);
            }
        }
        
        public void RandomizeCharacter()
        {
            currentCharacterData.skinColorIndex = Random.Range(0, skinMaterials.Count);
            currentCharacterData.height = Random.Range(0.8f, 1.2f);
            currentCharacterData.build = Random.Range(0f, 1f);
            currentCharacterData.hairStyleIndex = Random.Range(0, hairStyles.Count);
            currentCharacterData.hairColorIndex = Random.Range(0, hairMaterials.Count);
            currentCharacterData.clothingStyleIndex = Random.Range(0, clothingOptions.Count);
            currentCharacterData.clothingColorIndex = Random.Range(0, clothingMaterials.Count);
            
            ApplyCharacterData(currentCharacterData);
        }
        
        public bool ValidateCharacter()
        {
            // Check name length
            if (string.IsNullOrEmpty(currentCharacterData.characterName) || 
                currentCharacterData.characterName.Length < MIN_NAME_LENGTH)
            {
                Debug.LogWarning("Character name is too short");
                return false;
            }
            
            return true;
        }
        
        public CharacterData GetCharacterData()
        {
            return currentCharacterData;
        }
        
        public void SaveCharacter()
        {
            if (ValidateCharacter())
            {
                // Save character data to persistent storage
                string characterJson = JsonUtility.ToJson(currentCharacterData);
                PlayerPrefs.SetString("PlayerCharacterData", characterJson);
                PlayerPrefs.Save();
                
                Debug.Log($"Character '{currentCharacterData.characterName}' saved successfully!");
            }
        }
        
        public void LoadCharacter()
        {
            if (PlayerPrefs.HasKey("PlayerCharacterData"))
            {
                string characterJson = PlayerPrefs.GetString("PlayerCharacterData");
                currentCharacterData = JsonUtility.FromJson<CharacterData>(characterJson);
                ApplyCharacterData(currentCharacterData);
            }
        }
        
        public void StartGame()
        {
            if (ValidateCharacter())
            {
                SaveCharacter();
                
                // Hide character creation UI
                if (characterCreationPanel != null)
                    characterCreationPanel.SetActive(false);
                
                // Start the game
                GameManager.Instance.StartNewGame();
            }
        }
    }
    
    [System.Serializable]
    public class CharacterData
    {
        public string characterName = "Newport Newcomer";
        public int skinColorIndex = 0;
        public float height = 1.0f;
        public float build = 0.5f;
        public int hairStyleIndex = 0;
        public int hairColorIndex = 0;
        public int clothingStyleIndex = 0;
        public int clothingColorIndex = 0;
    }
    
    [System.Serializable]
    public class CharacterStyle
    {
        public string styleName;
        public string description;
        public int hairStyleIndex;
        public int clothingStyleIndex;
        public Color[] recommendedColors;
    }
}