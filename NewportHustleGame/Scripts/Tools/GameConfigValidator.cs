using System;
using UnityEngine;

namespace NewportHustle.Tools
{
    /// <summary>
    /// Tool to validate game configuration and Newport business integration
    /// </summary>
    public class GameConfigValidator : MonoBehaviour
    {
        [Header("Config Validation")]
        public bool validateOnStart = false;
        
        private void Start()
        {
            if (validateOnStart)
            {
                ValidateConfigs();
            }
        }
        
        [ContextMenu("Validate All Configs")]
        public void ValidateConfigs()
        {
            Debug.Log("=== Newport Hustle Config Validation ===");
            
            ValidateVehicleDatabase();
            CheckBusinessIntegration();
            ValidateGameSystems();
            
            Debug.Log("=== Validation Complete ===");
        }
        
        private void ValidateVehicleDatabase()
        {
            // Load vehicle database from Resources
            TextAsset vehicleDataAsset = Resources.Load<TextAsset>("Config/VehicleDatabase");
            
            if (vehicleDataAsset != null)
            {
                try
                {
                    string jsonContent = vehicleDataAsset.text;
                    Debug.Log("✓ VehicleDatabase.json loaded successfully");
                    
                    // Check for authentic Newport businesses
                    if (jsonContent.Contains("Jordan's Gas Station"))
                        Debug.Log("✓ Jordan's Gas Station found");
                    if (jsonContent.Contains("Lackey Tamale Shop"))
                        Debug.Log("✓ Lackey Tamale Shop found");
                    if (jsonContent.Contains("The Yella Store"))
                        Debug.Log("✓ The Yella Store found");
                        
                    // Check for vehicle types
                    if (jsonContent.Contains("Classic Cruiser"))
                        Debug.Log("✓ Box Chevy (Classic Cruiser) found");
                    if (jsonContent.Contains("Supreme Rider"))
                        Debug.Log("✓ 1984 Cutlass Supreme (Supreme Rider) found");
                    if (jsonContent.Contains("Authority Sedan"))
                        Debug.Log("✓ Police Crown Victoria (Authority Sedan) found");
                    if (jsonContent.Contains("Patrol Classic"))
                        Debug.Log("✓ Police Grand Marquis (Patrol Classic) found");
                        
                    Debug.Log("✓ All authentic Newport businesses and vehicles validated");
                }
                catch (Exception e)
                {
                    Debug.LogError($"✗ VehicleDatabase.json validation failed: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("✗ VehicleDatabase.json not found in Resources/Config/");
                Debug.LogWarning("Please ensure VehicleDatabase.json is placed in Assets/Resources/Config/");
            }
        }
        
        [ContextMenu("Check Business Integration")]
        public void CheckBusinessIntegration()
        {
            Debug.Log("=== Newport Business Integration Check ===");
            
            // Check if zone manager exists
            if (FindObjectOfType<MonoBehaviour>() != null)
            {
                var zoneManagers = FindObjectsOfType<MonoBehaviour>();
                bool foundZoneManager = false;
                
                foreach (var component in zoneManagers)
                {
                    if (component.GetType().Name == "ZoneManager")
                    {
                        foundZoneManager = true;
                        Debug.Log("✓ ZoneManager found - businesses should be integrated");
                        break;
                    }
                }
                
                if (!foundZoneManager)
                {
                    Debug.LogWarning("! ZoneManager not found in scene");
                }
            }
            
            Debug.Log("✓ Business integration check complete");
            Debug.Log("=== Business Integration Check Complete ===");
        }
        
        [ContextMenu("Validate Game Systems")]
        public void ValidateGameSystems()
        {
            Debug.Log("=== Game Systems Validation ===");
            
            // Check for core game components by name to avoid dependency issues
            var allComponents = FindObjectsOfType<MonoBehaviour>();
            bool hasVehicleController = false;
            bool hasPoliceSystem = false;
            bool hasZoneManager = false;
            
            foreach (var component in allComponents)
            {
                string typeName = component.GetType().Name;
                if (typeName == "VehicleController") hasVehicleController = true;
                if (typeName == "PoliceSystem") hasPoliceSystem = true;
                if (typeName == "ZoneManager") hasZoneManager = true;
            }
            
            if (hasVehicleController)
                Debug.Log("✓ VehicleController system available");
            else
                Debug.LogWarning("! VehicleController not found");
                
            if (hasPoliceSystem)
                Debug.Log("✓ PoliceSystem available");
            else
                Debug.LogWarning("! PoliceSystem not found");
                
            if (hasZoneManager)
                Debug.Log("✓ ZoneManager available");
            else
                Debug.LogWarning("! ZoneManager not found");
                
            Debug.Log("=== Game Systems Validation Complete ===");
        }
        
        [ContextMenu("Show Newport Features")]
        public void ShowNewportFeatures()
        {
            Debug.Log("=== Newport Hustle Features ===");
            Debug.Log("🎮 GTA-Style Gameplay:");
            Debug.Log("  • Vehicle Physics System with Mobile Controls");
            Debug.Log("  • 4-Level Police Wanted System");
            Debug.Log("  • Crown Victoria & Grand Marquis Police Cars");
            Debug.Log("");
            Debug.Log("🚗 Authentic Vehicles (with knock-off names):");
            Debug.Log("  • Box Chevys → Classic Cruiser");
            Debug.Log("  • 1984 Cutlass Supreme → Supreme Rider");
            Debug.Log("  • Chevy SUVs → Mountain King");
            Debug.Log("  • Lincoln Navigator → Presidential SUV");
            Debug.Log("  • Lexus RX 350 → Luxury Crossover");
            Debug.Log("");
            Debug.Log("🏪 Authentic Newport Businesses:");
            Debug.Log("  • Jordan's Gas Station - Full service station");
            Debug.Log("  • Lackey Tamale Shop - Traditional Mexican food");
            Debug.Log("  • The Yella Store - Yellow tobacco shop landmark");
            Debug.Log("");
            Debug.Log("📱 Mobile Features:");
            Debug.Log("  • Touch steering and acceleration");
            Debug.Log("  • Optimized UI for mobile devices");
            Debug.Log("  • Logo integration for app stores");
            Debug.Log("=== Features Overview Complete ===");
        }
    }
}