using UnityEngine;
using System.Collections.Generic;

namespace NewportHustle.Vehicles
{
    /// <summary>
    /// Core vehicle controller for Newport Hustle - GTA-style driving mechanics
    /// Supports cars, SUVs, motorcycles with realistic physics and mobile controls
    /// </summary>
    public class VehicleController : MonoBehaviour
    {
        [Header("Vehicle Identity")]
        [SerializeField] private string vehicleName;
        [SerializeField] private VehicleType vehicleType;
        [SerializeField] private VehicleClass vehicleClass;
        [SerializeField] private float vehicleValue = 1000f;
        
        [Header("Engine & Performance")]
        [SerializeField] private float maxSpeed = 200f;           // km/h
        [SerializeField] private float acceleration = 1500f;      // Force applied
        [SerializeField] private float brakeForce = 3000f;       // Brake strength
        [SerializeField] private float reverseForce = 1000f;     // Reverse speed
        [SerializeField] private float steerAngle = 30f;         // Max steering angle
        [SerializeField] private float engineRPM = 0f;
        
        [Header("Physics & Handling")]
        [SerializeField] private float downForce = 100f;         // High-speed stability
        [SerializeField] private float centerOfMassOffset = -1f; // Lower = more stable
        [SerializeField] private AnimationCurve steerCurve;      // Speed-based steering
        [SerializeField] private float antiRoll = 5000f;         // Anti-roll bar strength
        
        [Header("Wheel Configuration")]
        [SerializeField] private WheelCollider frontLeftWheel;
        [SerializeField] private WheelCollider frontRightWheel;
        [SerializeField] private WheelCollider rearLeftWheel;
        [SerializeField] private WheelCollider rearRightWheel;
        
        [Header("Wheel Meshes")]
        [SerializeField] private Transform frontLeftWheelMesh;
        [SerializeField] private Transform frontRightWheelMesh;
        [SerializeField] private Transform rearLeftWheelMesh;
        [SerializeField] private Transform rearRightWheelMesh;
        
        [Header("Mobile Controls")]
        [SerializeField] private float touchSensitivity = 2f;
        [SerializeField] private bool autoAccelerate = false;
        [SerializeField] private float autoAccelSpeed = 0.5f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioClip[] engineSounds;
        [SerializeField] private AudioClip hornSound;
        [SerializeField] private AudioClip brakeSound;
        [SerializeField] private float enginePitchMultiplier = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem[] exhaustParticles;
        [SerializeField] private GameObject[] brakeLights;
        [SerializeField] private GameObject[] headlights;
        [SerializeField] private TrailRenderer[] tireMarks;
        
        [Header("Damage System")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private GameObject[] damageModels;       // Different damage states
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private ParticleSystem fireEffect;
        
        // Private variables
        private Rigidbody vehicleRigidbody;
        private float motorInput;
        private float steerInput;
        private float brakeInput;
        private bool isPlayerControlled = false;
        private bool isEngineRunning = true;
        private float currentSpeed;
        private bool isGrounded;
        private Vector3 lastPosition;
        
        // Mobile input
        private Vector2 touchStartPos;
        private bool isTouching = false;
        
        // GTA-style features
        private bool lightsOn = false;
        private bool sirenActive = false; // For police vehicles
        private float lastHornTime = 0f;
        
        public enum VehicleType
        {
            Sedan,      // Box Chevy, Cutlass Supreme
            SUV,        // Chevy SUV, Lincoln Navigator, Lexus RX 350
            Sports,     // Sports cars
            Motorcycle, // Motorcycles
            Police,     // Crown Victoria, Grand Marquis
            Truck,      // Pickup trucks
            Van         // Commercial vehicles
        }
        
        public enum VehicleClass
        {
            Economy,    // Basic vehicles
            Mid,        // Standard vehicles
            Luxury,     // High-end vehicles
            Super,      // Supercars
            Emergency   // Police/emergency vehicles
        }
        
        // Properties
        public float CurrentSpeed => currentSpeed;
        public float MaxSpeed => maxSpeed;
        public bool IsPlayerControlled => isPlayerControlled;
        public VehicleType Type => vehicleType;
        public VehicleClass Class => vehicleClass;
        public float Health => currentHealth;
        public bool IsEngineRunning => isEngineRunning;
        public string VehicleName => vehicleName;
        
        void Start()
        {
            InitializeVehicle();
        }
        
        void Update()
        {
            if (isPlayerControlled)
            {
                HandleInput();
            }
            
            UpdateVehicleStats();
            UpdateAudio();
            UpdateVisualEffects();
            UpdateWheelMeshes();
        }
        
        void FixedUpdate()
        {
            if (isEngineRunning)
            {
                ApplyMotorForce();
                ApplySteering();
                ApplyBraking();
                ApplyDownforce();
                ApplyAntiRoll();
            }
            
            UpdateGroundedStatus();
        }
        
        /// <summary>
        /// Initialize vehicle components and settings
        /// </summary>
        private void InitializeVehicle()
        {
            vehicleRigidbody = GetComponent<Rigidbody>();
            
            // Adjust center of mass for stability
            vehicleRigidbody.centerOfMass += Vector3.up * centerOfMassOffset;
            
            // Set wheel configurations based on vehicle type
            ConfigureWheelsForVehicleType();
            
            // Initialize audio
            if (engineAudioSource == null)
                engineAudioSource = GetComponent<AudioSource>();
            
            // Set initial health
            currentHealth = maxHealth;
            
            lastPosition = transform.position;
        }
        
        /// <summary>
        /// Configure wheel settings based on vehicle type
        /// </summary>
        private void ConfigureWheelsForVehicleType()
        {
            WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
            WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
            
            switch (vehicleType)
            {
                case VehicleType.Sports:
                    // High grip for sports cars
                    forwardFriction.extremumSlip = 0.4f;
                    forwardFriction.extremumValue = 1.0f;
                    forwardFriction.asymptoteSlip = 0.8f;
                    forwardFriction.asymptoteValue = 0.75f;
                    forwardFriction.stiffness = 1.0f;
                    break;
                    
                case VehicleType.SUV:
                    // Moderate grip, good for off-road
                    forwardFriction.extremumSlip = 0.3f;
                    forwardFriction.extremumValue = 0.8f;
                    forwardFriction.asymptoteSlip = 0.6f;
                    forwardFriction.asymptoteValue = 0.6f;
                    forwardFriction.stiffness = 0.8f;
                    break;
                    
                case VehicleType.Motorcycle:
                    // Responsive but less stable
                    forwardFriction.extremumSlip = 0.3f;
                    forwardFriction.extremumValue = 1.2f;
                    forwardFriction.asymptoteSlip = 0.7f;
                    forwardFriction.asymptoteValue = 0.8f;
                    forwardFriction.stiffness = 1.2f;
                    break;
                    
                default: // Sedan, Police, etc.
                    forwardFriction.extremumSlip = 0.3f;
                    forwardFriction.extremumValue = 0.9f;
                    forwardFriction.asymptoteSlip = 0.7f;
                    forwardFriction.asymptoteValue = 0.7f;
                    forwardFriction.stiffness = 1.0f;
                    break;
            }
            
            // Copy forward friction to sideways with adjustments
            sidewaysFriction = forwardFriction;
            sidewaysFriction.stiffness *= 0.8f; // Slightly less sideways grip
            
            // Apply to all wheels
            ApplyFrictionToWheel(frontLeftWheel, forwardFriction, sidewaysFriction);
            ApplyFrictionToWheel(frontRightWheel, forwardFriction, sidewaysFriction);
            ApplyFrictionToWheel(rearLeftWheel, forwardFriction, sidewaysFriction);
            ApplyFrictionToWheel(rearRightWheel, forwardFriction, sidewaysFriction);
        }
        
        /// <summary>
        /// Apply friction curves to a wheel
        /// </summary>
        private void ApplyFrictionToWheel(WheelCollider wheel, WheelFrictionCurve forward, WheelFrictionCurve sideways)
        {
            if (wheel != null)
            {
                wheel.forwardFriction = forward;
                wheel.sidewaysFriction = sideways;
            }
        }
        
        /// <summary>
        /// Handle player input for vehicle control
        /// </summary>
        private void HandleInput()
        {
            // Mobile touch input
            if (Application.isMobilePlatform)
            {
                HandleMobileInput();
            }
            else
            {
                HandleDesktopInput();
            }
            
            // Universal inputs
            HandleUniversalInputs();
        }
        
        /// <summary>
        /// Handle mobile touch controls
        /// </summary>
        private void HandleMobileInput()
        {
            // Get UI input from mobile controls
            motorInput = 0f;
            steerInput = 0f;
            brakeInput = 0f;
            
            // Check for touch input on screen areas
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = touch.position;
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPos = touchPos;
                        isTouching = true;
                        break;
                        
                    case TouchPhase.Moved:
                        if (isTouching)
                        {
                            Vector2 deltaTouch = (touchPos - touchStartPos) / Screen.height;
                            
                            // Vertical movement for acceleration/braking
                            if (deltaTouch.y > 0.1f)
                                motorInput = Mathf.Clamp01(deltaTouch.y * touchSensitivity);
                            else if (deltaTouch.y < -0.1f)
                                brakeInput = Mathf.Clamp01(-deltaTouch.y * touchSensitivity);
                            
                            // Horizontal movement for steering
                            steerInput = Mathf.Clamp(deltaTouch.x * touchSensitivity, -1f, 1f);
                        }
                        break;
                        
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isTouching = false;
                        if (autoAccelerate)
                            motorInput = autoAccelSpeed;
                        break;
                }
            }
            else if (autoAccelerate)
            {
                motorInput = autoAccelSpeed;
            }
        }
        
        /// <summary>
        /// Handle desktop keyboard/mouse input
        /// </summary>
        private void HandleDesktopInput()
        {
            motorInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
            brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        }
        
        /// <summary>
        /// Handle universal inputs (horn, lights, etc.)
        /// </summary>
        private void HandleUniversalInputs()
        {
            // Horn
            if (Input.GetKeyDown(KeyCode.H) || (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Began))
            {
                PlayHorn();
            }
            
            // Lights
            if (Input.GetKeyDown(KeyCode.L))
            {
                ToggleLights();
            }
            
            // Police siren (if police vehicle)
            if (vehicleType == VehicleType.Police && Input.GetKeyDown(KeyCode.R))
            {
                ToggleSiren();
            }
        }
        
        /// <summary>
        /// Apply motor force to wheels
        /// </summary>
        private void ApplyMotorForce()
        {
            float motor = acceleration * motorInput;
            
            // Apply motor force based on vehicle type
            if (vehicleType == VehicleType.Motorcycle)
            {
                // Motorcycles: rear wheel drive only
                rearLeftWheel.motorTorque = motor;
                rearRightWheel.motorTorque = motor;
            }
            else
            {
                // Cars: front or all-wheel drive based on vehicle
                switch (vehicleClass)
                {
                    case VehicleClass.Super:
                    case VehicleClass.Luxury:
                        // All-wheel drive for luxury/super cars
                        frontLeftWheel.motorTorque = motor * 0.3f;
                        frontRightWheel.motorTorque = motor * 0.3f;
                        rearLeftWheel.motorTorque = motor * 0.7f;
                        rearRightWheel.motorTorque = motor * 0.7f;
                        break;
                        
                    default:
                        // Front-wheel drive for most cars
                        frontLeftWheel.motorTorque = motor;
                        frontRightWheel.motorTorque = motor;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Apply steering to front wheels
        /// </summary>
        private void ApplySteering()
        {
            float steer = steerAngle * steerInput;
            
            // Apply speed-based steering if curve is set
            if (steerCurve != null && steerCurve.keys.Length > 0)
            {
                float speedFactor = currentSpeed / maxSpeed;
                steer *= steerCurve.Evaluate(speedFactor);
            }
            
            frontLeftWheel.steerAngle = steer;
            frontRightWheel.steerAngle = steer;
        }
        
        /// <summary>
        /// Apply braking force
        /// </summary>
        private void ApplyBraking()
        {
            float brake = brakeForce * brakeInput;
            
            // Apply to all wheels
            frontLeftWheel.brakeTorque = brake;
            frontRightWheel.brakeTorque = brake;
            rearLeftWheel.brakeTorque = brake;
            rearRightWheel.brakeTorque = brake;
            
            // Update brake lights
            UpdateBrakeLights(brakeInput > 0.1f);
        }
        
        /// <summary>
        /// Apply downforce for high-speed stability
        /// </summary>
        private void ApplyDownforce()
        {
            float speedFactor = currentSpeed / maxSpeed;
            vehicleRigidbody.AddForce(-transform.up * downForce * speedFactor * speedFactor);
        }
        
        /// <summary>
        /// Apply anti-roll force to prevent flipping
        /// </summary>
        private void ApplyAntiRoll()
        {
            ApplyAntiRollToWheelPair(frontLeftWheel, frontRightWheel);
            ApplyAntiRollToWheelPair(rearLeftWheel, rearRightWheel);
        }
        
        /// <summary>
        /// Apply anti-roll between two wheels
        /// </summary>
        private void ApplyAntiRollToWheelPair(WheelCollider leftWheel, WheelCollider rightWheel)
        {
            WheelHit leftHit, rightHit;
            bool leftGrounded = leftWheel.GetGroundHit(out leftHit);
            bool rightGrounded = rightWheel.GetGroundHit(out rightHit);
            
            if (leftGrounded)
                vehicleRigidbody.AddForceAtPosition(leftWheel.transform.up * -leftHit.force, leftWheel.transform.position);
            if (rightGrounded)
                vehicleRigidbody.AddForceAtPosition(rightWheel.transform.up * -rightHit.force, rightWheel.transform.position);
            
            if (leftGrounded && rightGrounded)
            {
                float antiRollForce = (leftHit.distance - rightHit.distance) * antiRoll;
                
                if (leftGrounded)
                    vehicleRigidbody.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
                if (rightGrounded)
                    vehicleRigidbody.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
            }
        }
        
        /// <summary>
        /// Update vehicle statistics
        /// </summary>
        private void UpdateVehicleStats()
        {
            // Calculate speed
            currentSpeed = vehicleRigidbody.velocity.magnitude * 3.6f; // Convert to km/h
            
            // Calculate engine RPM based on speed and gear simulation
            engineRPM = Mathf.Lerp(800f, 6000f, currentSpeed / maxSpeed) + (motorInput * 1000f);
            engineRPM = Mathf.Clamp(engineRPM, 600f, 7000f);
            
            // Update grounded status
            isGrounded = IsVehicleGrounded();
        }
        
        /// <summary>
        /// Check if vehicle is on the ground
        /// </summary>
        private bool IsVehicleGrounded()
        {
            return frontLeftWheel.isGrounded || frontRightWheel.isGrounded || 
                   rearLeftWheel.isGrounded || rearRightWheel.isGrounded;
        }
        
        /// <summary>
        /// Update grounded status for physics calculations
        /// </summary>
        private void UpdateGroundedStatus()
        {
            isGrounded = IsVehicleGrounded();
        }
        
        /// <summary>
        /// Update engine audio based on RPM and speed
        /// </summary>
        private void UpdateAudio()
        {
            if (engineAudioSource != null && isEngineRunning)
            {
                // Adjust pitch based on engine RPM
                float targetPitch = Mathf.Lerp(0.5f, 2.0f, (engineRPM - 600f) / 6400f);
                engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * 3f);
                
                // Adjust volume based on throttle input
                float targetVolume = Mathf.Lerp(0.3f, 1.0f, Mathf.Abs(motorInput));
                engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, targetVolume, Time.deltaTime * 2f);
            }
        }
        
        /// <summary>
        /// Update visual effects (exhaust, lights, etc.)
        /// </summary>
        private void UpdateVisualEffects()
        {
            // Update exhaust particles
            if (exhaustParticles != null)
            {
                foreach (var exhaust in exhaustParticles)
                {
                    if (exhaust != null)
                    {
                        var emission = exhaust.emission;
                        emission.rateOverTime = Mathf.Lerp(0f, 50f, Mathf.Abs(motorInput));
                    }
                }
            }
            
            // Update damage effects
            UpdateDamageEffects();
        }
        
        /// <summary>
        /// Update wheel mesh positions and rotations
        /// </summary>
        private void UpdateWheelMeshes()
        {
            UpdateWheelMesh(frontLeftWheel, frontLeftWheelMesh);
            UpdateWheelMesh(frontRightWheel, frontRightWheelMesh);
            UpdateWheelMesh(rearLeftWheel, rearLeftWheelMesh);
            UpdateWheelMesh(rearRightWheel, rearRightWheelMesh);
        }
        
        /// <summary>
        /// Update individual wheel mesh
        /// </summary>
        private void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelMesh)
        {
            if (wheelCollider != null && wheelMesh != null)
            {
                Vector3 position;
                Quaternion rotation;
                wheelCollider.GetWorldPose(out position, out rotation);
                
                wheelMesh.position = position;
                wheelMesh.rotation = rotation;
            }
        }
        
        /// <summary>
        /// Update brake lights
        /// </summary>
        private void UpdateBrakeLights(bool braking)
        {
            if (brakeLights != null)
            {
                foreach (var light in brakeLights)
                {
                    if (light != null)
                        light.SetActive(braking);
                }
            }
        }
        
        /// <summary>
        /// Update damage effects based on health
        /// </summary>
        private void UpdateDamageEffects()
        {
            float healthPercent = currentHealth / maxHealth;
            
            // Smoke effect for damaged vehicles
            if (smokeEffect != null)
            {
                var emission = smokeEffect.emission;
                if (healthPercent < 0.5f)
                {
                    emission.enabled = true;
                    emission.rateOverTime = Mathf.Lerp(0f, 30f, 1f - healthPercent);
                }
                else
                {
                    emission.enabled = false;
                }
            }
            
            // Fire effect for severely damaged vehicles
            if (fireEffect != null)
            {
                fireEffect.gameObject.SetActive(healthPercent < 0.2f);
            }
        }
        
        /// <summary>
        /// Play horn sound
        /// </summary>
        private void PlayHorn()
        {
            if (hornSound != null && Time.time - lastHornTime > 1f)
            {
                AudioSource.PlayClipAtPoint(hornSound, transform.position);
                lastHornTime = Time.time;
            }
        }
        
        /// <summary>
        /// Toggle vehicle lights
        /// </summary>
        private void ToggleLights()
        {
            lightsOn = !lightsOn;
            
            if (headlights != null)
            {
                foreach (var light in headlights)
                {
                    if (light != null)
                        light.SetActive(lightsOn);
                }
            }
        }
        
        /// <summary>
        /// Toggle police siren
        /// </summary>
        private void ToggleSiren()
        {
            if (vehicleType == VehicleType.Police)
            {
                sirenActive = !sirenActive;
                // Implementation for siren effects would go here
            }
        }
        
        /// <summary>
        /// Take damage to the vehicle
        /// </summary>
        /// <param name="damage">Amount of damage to take</param>
        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            
            // Handle vehicle destruction
            if (currentHealth <= 0f)
            {
                DestroyVehicle();
            }
        }
        
        /// <summary>
        /// Repair the vehicle
        /// </summary>
        /// <param name="repairAmount">Amount to repair</param>
        public void RepairVehicle(float repairAmount = -1f)
        {
            if (repairAmount < 0)
                currentHealth = maxHealth;
            else
                currentHealth = Mathf.Min(maxHealth, currentHealth + repairAmount);
        }
        
        /// <summary>
        /// Destroy the vehicle
        /// </summary>
        private void DestroyVehicle()
        {
            isEngineRunning = false;
            // Add explosion effect, disable controls, etc.
        }
        
        /// <summary>
        /// Set player control of this vehicle
        /// </summary>
        /// <param name="controlled">Whether player controls this vehicle</param>
        public void SetPlayerControlled(bool controlled)
        {
            isPlayerControlled = controlled;
        }
        
        /// <summary>
        /// Get vehicle info for UI display
        /// </summary>
        /// <returns>Vehicle information</returns>
        public VehicleInfo GetVehicleInfo()
        {
            return new VehicleInfo
            {
                name = vehicleName,
                type = vehicleType,
                vehicleClass = vehicleClass,
                speed = currentSpeed,
                maxSpeed = maxSpeed,
                health = currentHealth,
                maxHealth = maxHealth,
                isEngineRunning = isEngineRunning,
                lightsOn = lightsOn,
                sirenActive = sirenActive
            };
        }
        
        void OnCollisionEnter(Collision collision)
        {
            // Handle collision damage
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 10f)
            {
                float damage = (impactForce - 10f) * 2f;
                TakeDamage(damage);
            }
        }
    }
    
    /// <summary>
    /// Structure for vehicle information
    /// </summary>
    [System.Serializable]
    public struct VehicleInfo
    {
        public string name;
        public VehicleController.VehicleType type;
        public VehicleController.VehicleClass vehicleClass;
        public float speed;
        public float maxSpeed;
        public float health;
        public float maxHealth;
        public bool isEngineRunning;
        public bool lightsOn;
        public bool sirenActive;
    }
}