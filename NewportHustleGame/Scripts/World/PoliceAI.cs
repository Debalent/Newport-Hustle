using UnityEngine;
using UnityEngine.AI;

namespace NewportHustle.World
{
    /// <summary>
    /// AI controller for police vehicles in Newport Hustle
    /// Handles pursuit behavior, patrol routes, and response tactics
    /// </summary>
    public class PoliceAI : MonoBehaviour
    {
        [Header("AI Behavior")]
        [SerializeField] private PoliceState currentState = PoliceState.Patrol;
        [SerializeField] private Transform target;                    // Player target
        [SerializeField] private float detectionRange = 80f;         // Range to detect player
        [SerializeField] private float pursuitRange = 200f;          // Range to continue pursuit
        [SerializeField] private float attackRange = 5f;             // Range for PIT maneuvers
        
        [Header("Pursuit Behavior")]
        [SerializeField] private float pursuitSpeed = 80f;           // km/h pursuit speed
        [SerializeField] private float aggressiveness = 0.5f;        // How aggressive (0-1)
        [SerializeField] private bool canUsePITManeuver = true;      // Can perform PIT maneuvers
        [SerializeField] private float pitCooldown = 10f;            // Cooldown between PIT attempts
        [SerializeField] private float ramingForce = 1500f;          // Force for ramming
        
        [Header("Patrol Behavior")]
        [SerializeField] private Transform[] patrolPoints;           // Patrol route waypoints
        [SerializeField] private float patrolSpeed = 40f;            // km/h patrol speed
        [SerializeField] private float patrolWaitTime = 5f;          // Wait time at patrol points
        [SerializeField] private bool randomPatrol = true;           // Random patrol vs fixed route
        
        [Header("Communication")]
        [SerializeField] private float radioRange = 300f;           // Range to call for backup
        [SerializeField] private float backupCallCooldown = 30f;    // Cooldown between backup calls
        [SerializeField] private AudioSource sirenAudioSource;      // Siren audio
        [SerializeField] private GameObject[] emergencyLights;      // Police lights
        
        [Header("Vehicle Setup")]
        [SerializeField] private Vehicles.VehicleController vehicleController;
        [SerializeField] private NavMeshAgent navAgent;             // For pathfinding
        [SerializeField] private Rigidbody vehicleRigidbody;
        
        // Private variables
        private int currentPatrolIndex = 0;
        private float lastPITAttempt = 0f;
        private float lastBackupCall = 0f;
        private float patrolTimer = 0f;
        private Vector3 lastKnownTargetPosition;
        private bool sirenActive = false;
        private int wantedLevel = 1;
        private PoliceSystem policeSystem;
        
        // State machine
        public enum PoliceState
        {
            Patrol,         // Normal patrol behavior
            Investigating,  // Checking last known position
            Pursuing,       // Active pursuit of player
            Attacking,      // Attempting to stop player (PIT, ram, etc.)
            Blocked,        // Can't reach player, repositioning
            Backup          // Called for backup, waiting
        }
        
        // Properties
        public PoliceState CurrentState => currentState;
        public Transform Target => target;
        public bool SirenActive => sirenActive;
        
        void Start()
        {
            InitializePoliceAI();
        }
        
        void Update()
        {
            UpdatePoliceAI();
            UpdateAudio();
            UpdateVisualEffects();
        }
        
        void FixedUpdate()
        {
            HandleMovement();
        }
        
        /// <summary>
        /// Initialize police AI components
        /// </summary>
        private void InitializePoliceAI()
        {
            // Get components
            if (vehicleController == null)
                vehicleController = GetComponent<Vehicles.VehicleController>();
            
            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();
            
            if (vehicleRigidbody == null)
                vehicleRigidbody = GetComponent<Rigidbody>();
            
            if (sirenAudioSource == null)
                sirenAudioSource = GetComponent<AudioSource>();
            
            // Find police system
            policeSystem = FindObjectOfType<PoliceSystem>();
            
            // Setup NavMesh agent
            if (navAgent != null)
            {
                navAgent.speed = patrolSpeed / 3.6f; // Convert km/h to m/s
                navAgent.acceleration = 8f;
                navAgent.angularSpeed = 120f;
            }
            
            // Start patrol if no target
            if (target == null)
            {
                SetState(PoliceState.Patrol);
            }
            
            lastKnownTargetPosition = transform.position;
        }
        
        /// <summary>
        /// Main AI update loop
        /// </summary>
        private void UpdatePoliceAI()
        {
            // Update target detection
            UpdateTargetDetection();
            
            // Execute current state behavior
            switch (currentState)
            {
                case PoliceState.Patrol:
                    UpdatePatrolBehavior();
                    break;
                    
                case PoliceState.Investigating:
                    UpdateInvestigatingBehavior();
                    break;
                    
                case PoliceState.Pursuing:
                    UpdatePursuitBehavior();
                    break;
                    
                case PoliceState.Attacking:
                    UpdateAttackBehavior();
                    break;
                    
                case PoliceState.Blocked:
                    UpdateBlockedBehavior();
                    break;
                    
                case PoliceState.Backup:
                    UpdateBackupBehavior();
                    break;
            }
            
            // Update timers
            patrolTimer += Time.deltaTime;
        }
        
        /// <summary>
        /// Update target detection and line of sight
        /// </summary>
        private void UpdateTargetDetection()
        {
            if (target == null) return;
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // Check if target is in detection range
            if (distanceToTarget <= detectionRange)
            {
                // Check line of sight
                RaycastHit hit;
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                
                if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionRange))
                {
                    if (hit.collider.transform == target)
                    {
                        // Target spotted!
                        lastKnownTargetPosition = target.position;
                        
                        if (currentState == PoliceState.Patrol)
                        {
                            SetState(PoliceState.Pursuing);
                        }
                    }
                }
            }
            
            // Lose target if too far away
            if (distanceToTarget > pursuitRange && currentState != PoliceState.Patrol)
            {
                SetState(PoliceState.Investigating);
            }
        }
        
        /// <summary>
        /// Update patrol behavior
        /// </summary>
        private void UpdatePatrolBehavior()
        {
            if (patrolPoints.Length == 0) return;
            
            // Move to current patrol point
            Transform currentPatrolPoint = patrolPoints[currentPatrolIndex];
            float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint.position);
            
            if (distanceToPatrolPoint < 10f)
            {
                // Reached patrol point, wait or move to next
                if (patrolTimer >= patrolWaitTime)
                {
                    MoveToNextPatrolPoint();
                    patrolTimer = 0f;
                }
            }
            else
            {
                // Move toward patrol point
                if (navAgent != null)
                {
                    navAgent.SetDestination(currentPatrolPoint.position);
                    navAgent.speed = patrolSpeed / 3.6f;
                }
            }
        }
        
        /// <summary>
        /// Update investigating behavior (searching last known position)
        /// </summary>
        private void UpdateInvestigatingBehavior()
        {
            float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownTargetPosition);
            
            if (distanceToLastKnown < 15f)
            {
                // Reached last known position, search around
                patrolTimer += Time.deltaTime;
                
                if (patrolTimer >= 10f) // Search for 10 seconds
                {
                    SetState(PoliceState.Patrol);
                    patrolTimer = 0f;
                }
            }
            else
            {
                // Move to last known position
                if (navAgent != null)
                {
                    navAgent.SetDestination(lastKnownTargetPosition);
                    navAgent.speed = (patrolSpeed * 1.5f) / 3.6f; // Faster investigation speed
                }
            }
        }
        
        /// <summary>
        /// Update pursuit behavior
        /// </summary>
        private void UpdatePursuitBehavior()
        {
            if (target == null)
            {
                SetState(PoliceState.Investigating);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            lastKnownTargetPosition = target.position;
            
            // Check if close enough to attack
            if (distanceToTarget <= attackRange && canUsePITManeuver)
            {
                SetState(PoliceState.Attacking);
                return;
            }
            
            // Call for backup if needed
            CheckBackupCall();
            
            // Chase target
            if (navAgent != null)
            {
                navAgent.SetDestination(target.position);
                navAgent.speed = pursuitSpeed / 3.6f;
            }
            
            // Enable siren
            if (!sirenActive)
            {
                ActivateSiren();
            }
        }
        
        /// <summary>
        /// Update attack behavior (PIT maneuvers, ramming)
        /// </summary>
        private void UpdateAttackBehavior()
        {
            if (target == null)
            {
                SetState(PoliceState.Pursuing);
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            // If target moved away, go back to pursuit
            if (distanceToTarget > attackRange * 2f)
            {
                SetState(PoliceState.Pursuing);
                return;
            }
            
            // Attempt PIT maneuver
            if (Time.time - lastPITAttempt >= pitCooldown)
            {
                AttemptPITManeuver();
            }
            else
            {
                // Continue pursuit until PIT cooldown is ready
                SetState(PoliceState.Pursuing);
            }
        }
        
        /// <summary>
        /// Update blocked behavior (can't reach target)
        /// </summary>
        private void UpdateBlockedBehavior()
        {
            // Try to find alternate route
            patrolTimer += Time.deltaTime;
            
            if (patrolTimer >= 5f) // Try for 5 seconds
            {
                SetState(PoliceState.Pursuing);
                patrolTimer = 0f;
            }
        }
        
        /// <summary>
        /// Update backup behavior (waiting for backup)
        /// </summary>
        private void UpdateBackupBehavior()
        {
            // Maintain distance and wait for backup
            if (target != null)
            {
                Vector3 directionFromTarget = (transform.position - target.position).normalized;
                Vector3 backupPosition = target.position + directionFromTarget * 50f; // Stay 50m away
                
                if (navAgent != null)
                {
                    navAgent.SetDestination(backupPosition);
                    navAgent.speed = (patrolSpeed * 0.8f) / 3.6f; // Slower backup speed
                }
            }
            
            // Return to pursuit after backup arrives (simulated)
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= 20f)
            {
                SetState(PoliceState.Pursuing);
                patrolTimer = 0f;
            }
        }
        
        /// <summary>
        /// Handle vehicle movement based on AI decisions
        /// </summary>
        private void HandleMovement()
        {
            if (vehicleController == null || navAgent == null) return;
            
            // Get steering input from NavMesh agent
            Vector3 desiredVelocity = navAgent.desiredVelocity;
            
            if (desiredVelocity.magnitude > 0.1f)
            {
                // Calculate steering input
                Vector3 forward = transform.forward;
                float steerInput = Vector3.Dot(Vector3.Cross(forward, desiredVelocity.normalized), Vector3.up);
                steerInput = Mathf.Clamp(steerInput, -1f, 1f);
                
                // Calculate motor input based on desired speed
                float currentSpeed = vehicleController.CurrentSpeed;
                float desiredSpeed = navAgent.speed * 3.6f; // Convert to km/h
                float motorInput = (desiredSpeed > currentSpeed) ? 1f : 0f;
                
                // Apply AI-controlled inputs to vehicle
                ApplyAIInputToVehicle(motorInput, steerInput, 0f);
            }
        }
        
        /// <summary>
        /// Apply AI input to vehicle controller
        /// </summary>
        /// <param name="motor">Motor input (-1 to 1)</param>
        /// <param name="steer">Steering input (-1 to 1)</param>
        /// <param name="brake">Brake input (0 to 1)</param>
        private void ApplyAIInputToVehicle(float motor, float steer, float brake)
        {
            // This would interface with the VehicleController
            // For now, we'll directly manipulate the vehicle through Rigidbody
            
            if (vehicleRigidbody != null)
            {
                Vector3 forwardForce = transform.forward * motor * 1000f * Time.fixedDeltaTime;
                vehicleRigidbody.AddForce(forwardForce);
                
                // Apply steering torque
                Vector3 steerTorque = transform.up * steer * 500f * Time.fixedDeltaTime;
                vehicleRigidbody.AddTorque(steerTorque);
            }
        }
        
        /// <summary>
        /// Move to next patrol point
        /// </summary>
        private void MoveToNextPatrolPoint()
        {
            if (randomPatrol)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }
            else
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        
        /// <summary>
        /// Attempt PIT maneuver on target
        /// </summary>
        private void AttemptPITManeuver()
        {
            if (target == null) return;
            
            // Calculate PIT maneuver direction
            Vector3 targetVelocity = target.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
            Vector3 pitDirection = Vector3.Cross(targetVelocity.normalized, Vector3.up).normalized;
            
            // Apply ramming force
            if (vehicleRigidbody != null)
            {
                vehicleRigidbody.AddForce(pitDirection * ramingForce, ForceMode.Impulse);
            }
            
            lastPITAttempt = Time.time;
            
            Debug.Log("Police attempting PIT maneuver!");
        }
        
        /// <summary>
        /// Check if backup should be called
        /// </summary>
        private void CheckBackupCall()
        {
            if (Time.time - lastBackupCall >= backupCallCooldown && wantedLevel >= 2)
            {
                CallBackup();
            }
        }
        
        /// <summary>
        /// Call for police backup
        /// </summary>
        private void CallBackup()
        {
            lastBackupCall = Time.time;
            
            // Notify police system to spawn more units
            if (policeSystem != null)
            {
                // Police system will handle spawning backup
                Debug.Log("Police calling for backup!");
            }
        }
        
        /// <summary>
        /// Activate police siren and lights
        /// </summary>
        private void ActivateSiren()
        {
            sirenActive = true;
            
            // Enable emergency lights
            if (emergencyLights != null)
            {
                foreach (var light in emergencyLights)
                {
                    if (light != null)
                        light.SetActive(true);
                }
            }
        }
        
        /// <summary>
        /// Deactivate police siren and lights
        /// </summary>
        private void DeactivateSiren()
        {
            sirenActive = false;
            
            // Disable emergency lights
            if (emergencyLights != null)
            {
                foreach (var light in emergencyLights)
                {
                    if (light != null)
                        light.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Update audio effects
        /// </summary>
        private void UpdateAudio()
        {
            if (sirenAudioSource != null)
            {
                if (sirenActive && !sirenAudioSource.isPlaying)
                {
                    sirenAudioSource.Play();
                }
                else if (!sirenActive && sirenAudioSource.isPlaying)
                {
                    sirenAudioSource.Stop();
                }
            }
        }
        
        /// <summary>
        /// Update visual effects (lights, etc.)
        /// </summary>
        private void UpdateVisualEffects()
        {
            // Flash emergency lights if active
            if (sirenActive && emergencyLights != null)
            {
                float flashRate = 3f; // Flashes per second
                bool lightState = (Time.time * flashRate) % 1f < 0.5f;
                
                for (int i = 0; i < emergencyLights.Length; i++)
                {
                    if (emergencyLights[i] != null)
                    {
                        // Alternate lights
                        bool isOddLight = (i % 2) == 1;
                        emergencyLights[i].SetActive(sirenActive && (lightState != isOddLight));
                    }
                }
            }
        }
        
        /// <summary>
        /// Set new AI state
        /// </summary>
        /// <param name="newState">New state to set</param>
        private void SetState(PoliceState newState)
        {
            if (currentState != newState)
            {
                // Exit current state
                switch (currentState)
                {
                    case PoliceState.Pursuing:
                    case PoliceState.Attacking:
                        if (newState == PoliceState.Patrol || newState == PoliceState.Investigating)
                        {
                            DeactivateSiren();
                        }
                        break;
                }
                
                // Enter new state
                currentState = newState;
                patrolTimer = 0f;
                
                switch (newState)
                {
                    case PoliceState.Pursuing:
                    case PoliceState.Attacking:
                        ActivateSiren();
                        break;
                        
                    case PoliceState.Patrol:
                        DeactivateSiren();
                        break;
                }
                
                Debug.Log($"Police AI state changed to: {newState}");
            }
        }
        
        /// <summary>
        /// Set target for police to pursue
        /// </summary>
        /// <param name="newTarget">Target transform</param>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                SetState(PoliceState.Pursuing);
            }
        }
        
        /// <summary>
        /// Set wanted level (affects aggressiveness)
        /// </summary>
        /// <param name="level">Wanted level (1-4)</param>
        public void SetWantedLevel(int level)
        {
            wantedLevel = Mathf.Clamp(level, 1, 4);
            
            // Adjust behavior based on wanted level
            switch (wantedLevel)
            {
                case 1:
                    aggressiveness = 0.3f;
                    canUsePITManeuver = false;
                    break;
                case 2:
                    aggressiveness = 0.5f;
                    canUsePITManeuver = true;
                    break;
                case 3:
                    aggressiveness = 0.7f;
                    canUsePITManeuver = true;
                    break;
                case 4:
                    aggressiveness = 1.0f;
                    canUsePITManeuver = true;
                    break;
            }
        }
        
        /// <summary>
        /// Handle collision with player or other objects
        /// </summary>
        /// <param name="collision">Collision info</param>
        void OnCollisionEnter(Collision collision)
        {
            // Check if we hit the target
            if (target != null && collision.transform == target)
            {
                Debug.Log("Police vehicle collided with target!");
                
                // Apply damage or stopping force to target vehicle
                Vehicles.VehicleController targetVehicle = target.GetComponent<Vehicles.VehicleController>();
                if (targetVehicle != null)
                {
                    targetVehicle.TakeDamage(10f);
                }
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw pursuit range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pursuitRange);
            
            // Draw attack range
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 5f);
                    }
                }
            }
        }
    }
}