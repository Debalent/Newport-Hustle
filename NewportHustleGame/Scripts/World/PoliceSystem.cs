using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using NewportHustle.Core;

namespace NewportHustle.World
{
    /// <summary>
    /// Manages police AI and wanted system for Newport Hustle
    /// Handles pursuit mechanics, wanted levels, and law enforcement response
    /// </summary>
    public class PoliceSystem : MonoBehaviour
    {
        [Header("Wanted System")]
        [SerializeField] private int currentWantedLevel = 0;
        [SerializeField] private float wantedDecayRate = 1f;        // How fast wanted level decays
        [SerializeField] private float wantedTimer = 0f;            // Time since last crime
        [SerializeField] private float[] wantedLevelDurations = { 60f, 120f, 180f, 300f }; // Duration for each level
        
        [Header("Police Response")]
        [SerializeField] private GameObject[] policeVehiclePrefabs;
        [SerializeField] private Transform[] policeSpawnPoints;
        [SerializeField] private int maxActivePolice = 6;
        [SerializeField] private float spawnDistance = 200f;        // Distance from player to spawn police
        [SerializeField] private float despawnDistance = 400f;     // Distance to despawn police
        
        [Header("Detection System")]
        [SerializeField] private float witnessRange = 50f;         // Range for NPCs to witness crimes
        [SerializeField] private float policeDetectionRange = 100f; // Police detection range
        [SerializeField] private LayerMask policeLayer = 1 << 8;   // Police vehicle layer
        [SerializeField] private LayerMask witnessLayer = 1 << 9;   // NPC layer
        
        [Header("Crime Response")]
        [SerializeField] private float speedingThreshold = 80f;    // km/h over limit triggers police
        [SerializeField] private float recklessDrivingThreshold = 50f; // Speed for reckless driving
        [SerializeField] private float damageThreshold = 20f;      // Damage to trigger response
        
        [Header("Audio")]
        [SerializeField] private AudioSource policeRadioSource;
        [SerializeField] private AudioClip[] policeRadioClips;
        [SerializeField] private AudioClip sirenSound;
        
        // Private variables
        private List<GameObject> activePoliceVehicles = new List<GameObject>();
        private Transform playerTransform;
        private Vehicles.VehicleController playerVehicle;
        private float lastCrimeTime;
        private Vector3 lastKnownPlayerPosition;
        private bool playerInVehicle = false;
        
        // Crime tracking
        private Dictionary<CrimeType, float> crimeValues = new Dictionary<CrimeType, float>();
        
        // Events
        public System.Action<int> OnWantedLevelChanged;
        public System.Action<CrimeType, float> OnCrimeCommitted;
        
        public enum CrimeType
        {
            Speeding,           // Minor - 0.1 wanted
            RunningRedLight,    // Minor - 0.2 wanted
            RecklessDriving,    // Medium - 0.3 wanted
            PropertyDamage,     // Medium - 0.4 wanted
            VehicleTheft,       // Major - 0.8 wanted
            HitAndRun,          // Major - 1.0 wanted
            AssaultingOfficer,  // Severe - 1.5 wanted
            FleeingPolice       // Escalating - 0.5 wanted per 10 seconds
        }
        
        // Properties
        public int CurrentWantedLevel => currentWantedLevel;
        public float WantedTimer => wantedTimer;
        public bool IsWanted => currentWantedLevel > 0;
        public Vector3 LastKnownPosition => lastKnownPlayerPosition;
        
        void Start()
        {
            InitializePoliceSystem();
            SetupCrimeValues();
        }
        
        void Update()
        {
            UpdateWantedSystem();
            UpdatePoliceResponse();
            CheckForCrimes();
            UpdateLastKnownPosition();
        }
        
        /// <summary>
        /// Initialize the police system
        /// </summary>
        private void InitializePoliceSystem()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerVehicle = player.GetComponentInChildren<Vehicles.VehicleController>();
            }
            
            // Setup audio
            if (policeRadioSource == null)
                policeRadioSource = GetComponent<AudioSource>();
            
            lastKnownPlayerPosition = playerTransform.position;
        }
        
        /// <summary>
        /// Setup crime values for different offenses
        /// </summary>
        private void SetupCrimeValues()
        {
            crimeValues[CrimeType.Speeding] = 0.1f;
            crimeValues[CrimeType.RunningRedLight] = 0.2f;
            crimeValues[CrimeType.RecklessDriving] = 0.3f;
            crimeValues[CrimeType.PropertyDamage] = 0.4f;
            crimeValues[CrimeType.VehicleTheft] = 0.8f;
            crimeValues[CrimeType.HitAndRun] = 1.0f;
            crimeValues[CrimeType.AssaultingOfficer] = 1.5f;
            crimeValues[CrimeType.FleeingPolice] = 0.5f;
        }
        
        /// <summary>
        /// Update the wanted system timer and level
        /// </summary>
        private void UpdateWantedSystem()
        {
            if (currentWantedLevel > 0)
            {
                wantedTimer += Time.deltaTime;
                
                // Check if wanted level should decay
                float requiredTime = wantedLevelDurations[Mathf.Min(currentWantedLevel - 1, wantedLevelDurations.Length - 1)];
                
                if (wantedTimer >= requiredTime)
                {
                    ReduceWantedLevel();
                }
            }
            else
            {
                wantedTimer = 0f;
            }
        }
        
        /// <summary>
        /// Update police response based on wanted level
        /// </summary>
        private void UpdatePoliceResponse()
        {
            if (currentWantedLevel > 0)
            {
                // Ensure we have enough police vehicles
                int requiredPolice = GetRequiredPoliceCount();
                
                while (activePoliceVehicles.Count < requiredPolice && activePoliceVehicles.Count < maxActivePolice)
                {
                    SpawnPoliceVehicle();
                }
                
                // Remove distant police vehicles
                CleanupDistantPolice();
            }
            else
            {
                // Clear all police when not wanted
                if (activePoliceVehicles.Count > 0)
                {
                    ClearAllPolice();
                }
            }
        }
        
        /// <summary>
        /// Check for various crimes the player might be committing
        /// </summary>
        private void CheckForCrimes()
        {
            if (playerVehicle != null && playerVehicle.IsPlayerControlled)
            {
                playerInVehicle = true;
                
                // Check for speeding
                CheckSpeeding();
                
                // Check for reckless driving
                CheckRecklessDriving();
                
                // Check for fleeing police
                if (currentWantedLevel > 0)
                {
                    CheckFleeingPolice();
                }
            }
            else
            {
                playerInVehicle = false;
            }
        }
        
        /// <summary>
        /// Check if player is speeding
        /// </summary>
        private void CheckSpeeding()
        {
            float currentSpeed = playerVehicle.CurrentSpeed;
            float speedLimit = GetCurrentSpeedLimit();
            
            if (currentSpeed > speedLimit + speedingThreshold)
            {
                // Check if police are nearby to witness
                if (IsPoliceNearby() || Random.Range(0f, 1f) < 0.1f) // 10% chance per frame when speeding
                {
                    CommitCrime(CrimeType.Speeding);
                }
            }
        }
        
        /// <summary>
        /// Check for reckless driving behavior
        /// </summary>
        private void CheckRecklessDriving()
        {
            float currentSpeed = playerVehicle.CurrentSpeed;
            
            // High speed + sharp turns = reckless driving
            if (currentSpeed > recklessDrivingThreshold && Input.GetAxis("Horizontal") > 0.8f)
            {
                if (Random.Range(0f, 1f) < 0.05f) // 5% chance per frame
                {
                    CommitCrime(CrimeType.RecklessDriving);
                }
            }
        }
        
        /// <summary>
        /// Check if player is fleeing from police
        /// </summary>
        private void CheckFleeingPolice()
        {
            if (IsPoliceNearby() && playerVehicle.CurrentSpeed > 30f)
            {
                // Add fleeing charges over time
                if (Time.time - lastCrimeTime > 10f) // Every 10 seconds of fleeing
                {
                    CommitCrime(CrimeType.FleeingPolice);
                }
            }
        }
        
        /// <summary>
        /// Check if police are nearby
        /// </summary>
        /// <returns>True if police are within detection range</returns>
        private bool IsPoliceNearby()
        {
            Collider[] policeColliders = Physics.OverlapSphere(playerTransform.position, policeDetectionRange, policeLayer);
            return policeColliders.Length > 0;
        }
        
        /// <summary>
        /// Get current speed limit based on zone
        /// </summary>
        /// <returns>Speed limit in km/h</returns>
        private float GetCurrentSpeedLimit()
        {
            // Get zone from ZoneManager
            var zoneManager = FindObjectOfType<ZoneManager>();
            if (zoneManager != null)
            {
                string currentZone = zoneManager.GetCurrentZone();
                
                switch (currentZone.ToLower())
                {
                    case "downtown":
                        return 50f; // 50 km/h in downtown
                    case "residential":
                        return 40f; // 40 km/h in residential
                    case "highway":
                        return 100f; // 100 km/h on highways
                    default:
                        return 60f; // Default speed limit
                }
            }
            
            return 60f; // Default fallback
        }
        
        /// <summary>
        /// Get required number of police based on wanted level
        /// </summary>
        /// <returns>Number of police vehicles needed</returns>
        private int GetRequiredPoliceCount()
        {
            switch (currentWantedLevel)
            {
                case 1: return 1;
                case 2: return 2;
                case 3: return 4;
                case 4: return 6;
                default: return 0;
            }
        }
        
        /// <summary>
        /// Spawn a police vehicle
        /// </summary>
        private void SpawnPoliceVehicle()
        {
            if (policeVehiclePrefabs.Length == 0 || playerTransform == null) return;
            
            // Find spawn point away from player but not too far
            Vector3 spawnPosition = GetPoliceSpawnPosition();
            
            // Choose appropriate police vehicle based on wanted level
            GameObject policePrefab = GetPoliceVehiclePrefab();
            
            if (policePrefab != null)
            {
                GameObject policeVehicle = Instantiate(policePrefab, spawnPosition, Quaternion.identity);
                
                // Setup police AI
                PoliceAI policeAI = policeVehicle.GetComponent<PoliceAI>();
                if (policeAI != null)
                {
                    policeAI.SetTarget(playerTransform);
                    policeAI.SetWantedLevel(currentWantedLevel);
                }
                
                activePoliceVehicles.Add(policeVehicle);
                
                // Play radio chatter
                PlayPoliceRadio();
            }
        }
        
        /// <summary>
        /// Get appropriate police vehicle based on wanted level
        /// </summary>
        /// <returns>Police vehicle prefab</returns>
        private GameObject GetPoliceVehiclePrefab()
        {
            if (policeVehiclePrefabs.Length == 0) return null;
            
            // For now, return random police vehicle
            // Later, could be based on wanted level (regular patrol vs SWAT)
            return policeVehiclePrefabs[Random.Range(0, policeVehiclePrefabs.Length)];
        }
        
        /// <summary>
        /// Get position to spawn police vehicle
        /// </summary>
        /// <returns>Spawn position</returns>
        private Vector3 GetPoliceSpawnPosition()
        {
            // Try predefined spawn points first
            if (policeSpawnPoints.Length > 0)
            {
                Transform nearestSpawn = null;
                float nearestDistance = float.MaxValue;
                
                foreach (Transform spawnPoint in policeSpawnPoints)
                {
                    float distance = Vector3.Distance(spawnPoint.position, playerTransform.position);
                    if (distance > spawnDistance && distance < despawnDistance && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestSpawn = spawnPoint;
                    }
                }
                
                if (nearestSpawn != null)
                    return nearestSpawn.position;
            }
            
            // Generate random spawn position
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 spawnDirection = new Vector3(randomDirection.x, 0, randomDirection.y);
            Vector3 spawnPosition = playerTransform.position + spawnDirection * spawnDistance;
            
            // Ensure spawn position is on ground
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 100f, Vector3.down, out hit, 200f))
            {
                spawnPosition.y = hit.point.y;
            }
            
            return spawnPosition;
        }
        
        /// <summary>
        /// Remove police vehicles that are too far away
        /// </summary>
        private void CleanupDistantPolice()
        {
            for (int i = activePoliceVehicles.Count - 1; i >= 0; i--)
            {
                if (activePoliceVehicles[i] == null)
                {
                    activePoliceVehicles.RemoveAt(i);
                    continue;
                }
                
                float distance = Vector3.Distance(activePoliceVehicles[i].transform.position, playerTransform.position);
                if (distance > despawnDistance)
                {
                    Destroy(activePoliceVehicles[i]);
                    activePoliceVehicles.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Clear all active police vehicles
        /// </summary>
        private void ClearAllPolice()
        {
            foreach (GameObject policeVehicle in activePoliceVehicles)
            {
                if (policeVehicle != null)
                {
                    Destroy(policeVehicle);
                }
            }
            activePoliceVehicles.Clear();
        }
        
        /// <summary>
        /// Update player's last known position
        /// </summary>
        private void UpdateLastKnownPosition()
        {
            if (currentWantedLevel > 0 && playerTransform != null)
            {
                // Update last known position if player is visible to police
                if (IsPoliceNearby())
                {
                    lastKnownPlayerPosition = playerTransform.position;
                }
            }
        }
        
        /// <summary>
        /// Play police radio chatter
        /// </summary>
        private void PlayPoliceRadio()
        {
            if (policeRadioSource != null && policeRadioClips.Length > 0)
            {
                AudioClip randomClip = policeRadioClips[Random.Range(0, policeRadioClips.Length)];
                policeRadioSource.PlayOneShot(randomClip);
            }
        }
        
        /// <summary>
        /// Commit a crime and increase wanted level
        /// </summary>
        /// <param name="crimeType">Type of crime committed</param>
        public void CommitCrime(CrimeType crimeType)
        {
            if (crimeValues.ContainsKey(crimeType))
            {
                float crimeValue = crimeValues[crimeType];
                IncreaseWantedLevel(crimeValue);
                
                lastCrimeTime = Time.time;
                wantedTimer = 0f; // Reset timer
                
                // Trigger events
                OnCrimeCommitted?.Invoke(crimeType, crimeValue);
                
                Debug.Log($"Crime committed: {crimeType} (+{crimeValue} wanted level)");
            }
        }
        
        /// <summary>
        /// Increase wanted level by specified amount
        /// </summary>
        /// <param name="amount">Amount to increase</param>
        private void IncreaseWantedLevel(float amount)
        {
            float totalWanted = (currentWantedLevel * 1.0f) + amount;
            int newWantedLevel = Mathf.Clamp(Mathf.FloorToInt(totalWanted), 0, 4);
            
            if (newWantedLevel != currentWantedLevel)
            {
                currentWantedLevel = newWantedLevel;
                OnWantedLevelChanged?.Invoke(currentWantedLevel);
                
                Debug.Log($"Wanted level increased to: {currentWantedLevel}");
            }
        }
        
        /// <summary>
        /// Reduce wanted level by one
        /// </summary>
        private void ReduceWantedLevel()
        {
            currentWantedLevel = Mathf.Max(0, currentWantedLevel - 1);
            wantedTimer = 0f;
            
            OnWantedLevelChanged?.Invoke(currentWantedLevel);
            
            Debug.Log($"Wanted level decreased to: {currentWantedLevel}");
        }
        
        /// <summary>
        /// Clear all wanted levels (for testing or story events)
        /// </summary>
        public void ClearWantedLevel()
        {
            currentWantedLevel = 0;
            wantedTimer = 0f;
            OnWantedLevelChanged?.Invoke(currentWantedLevel);
            ClearAllPolice();
        }
        
        /// <summary>
        /// Force set wanted level (for missions)
        /// </summary>
        /// <param name="level">Wanted level to set</param>
        public void SetWantedLevel(int level)
        {
            currentWantedLevel = Mathf.Clamp(level, 0, 4);
            wantedTimer = 0f;
            OnWantedLevelChanged?.Invoke(currentWantedLevel);
        }
        
        /// <summary>
        /// Handle vehicle collision for crime detection
        /// </summary>
        /// <param name="collision">Collision info</param>
        public void OnVehicleCollision(Collision collision)
        {
            if (playerInVehicle)
            {
                float impactForce = collision.relativeVelocity.magnitude;
                
                // Property damage
                if (impactForce > damageThreshold)
                {
                    CommitCrime(CrimeType.PropertyDamage);
                }
                
                // Hit and run if hitting another vehicle and fleeing
                Vehicles.VehicleController otherVehicle = collision.gameObject.GetComponent<Vehicles.VehicleController>();
                if (otherVehicle != null && impactForce > damageThreshold * 1.5f)
                {
                    StartCoroutine(CheckForHitAndRun());
                }
            }
        }
        
        /// <summary>
        /// Check if player flees after an accident
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator CheckForHitAndRun()
        {
            yield return new WaitForSeconds(5f); // Wait 5 seconds
            
            // If player is still moving fast, it's hit and run
            if (playerVehicle != null && playerVehicle.CurrentSpeed > 20f)
            {
                CommitCrime(CrimeType.HitAndRun);
            }
        }
        
        /// <summary>
        /// Handle red light violations
        /// </summary>
        public void OnRedLightViolation()
        {
            if (playerInVehicle)
            {
                CommitCrime(CrimeType.RunningRedLight);
            }
        }
        
        /// <summary>
        /// Handle vehicle theft
        /// </summary>
        public void OnVehicleTheft()
        {
            CommitCrime(CrimeType.VehicleTheft);
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw police detection range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, policeDetectionRange);
            
            // Draw witness range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, witnessRange);
            
            // Draw spawn distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, spawnDistance);
        }
    }
}