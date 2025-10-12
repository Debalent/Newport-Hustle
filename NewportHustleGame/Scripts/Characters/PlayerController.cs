using UnityEngine;
using System.Collections;

namespace NewportHustle.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 3f;
        public float runSpeed = 6f;
        public float sprintSpeed = 9f;
        public float jumpHeight = 1.5f;
        public float gravity = -9.81f;
        
        [Header("Mobile Touch Controls")]
        public bool useMobileControls = true;
        public float touchSensitivity = 2f;
        public float joystickDeadZone = 0.1f;
        
        [Header("Camera Settings")]
        public Transform cameraTransform;
        public float mouseSensitivity = 2f;
        public float verticalLookLimit = 90f;
        
        [Header("Vehicle System")]
        public float vehicleEntryRange = 2f;
        public LayerMask vehicleLayerMask = 1 << 10; // Vehicle layer
        
        // Vehicle-related variables
        private Vehicles.VehicleController currentVehicle;
        private bool isInVehicle = false;
        private Transform originalParent;
        private Vector3 exitOffset = new Vector3(2f, 0f, 0f); // Offset when exiting vehicle
        
        // Properties
        public bool IsInVehicle => isInVehicle;
        public Vehicles.VehicleController CurrentVehicle => currentVehicle;
        
        [Header("Interaction")]
        public float interactionRange = 3f;
        public LayerMask interactionLayerMask = 1;
        
        // Components
        private CharacterController characterController;
        private Animator animator;
        
        // Movement variables
        private Vector3 velocity;
        private Vector3 moveDirection;
        private bool isGrounded;
        private bool isRunning;
        private bool isSprinting;
        private bool isJumping;
        
        // Camera rotation
        private float xRotation = 0f;
        private float yRotation = 0f;
        
        // Mobile input
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool[] touchButtons = new bool[4]; // Jump, Sprint, Interact, Menu
        
        // Input states
        private bool jumpPressed;
        private bool sprintPressed;
        private bool interactPressed;
        
        void Start()
        {
            InitializePlayer();
        }
        
        void Update()
        {
            HandleInput();
            HandleMovement();
            HandleCamera();
            HandleInteraction();
            UpdateAnimations();
        }
        
        private void InitializePlayer()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            
            // Lock cursor for PC controls
            #if !UNITY_ANDROID && !UNITY_IOS
            Cursor.lockState = CursorLockMode.Locked;
            useMobileControls = false;
            #endif
            
            // Find camera if not assigned
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        
        private void HandleInput()
        {
            if (useMobileControls)
            {
                HandleMobileInput();
            }
            else
            {
                HandleKeyboardInput();
            }
        }
        
        private void HandleKeyboardInput()
        {
            // Movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            moveInput = new Vector2(horizontal, vertical);
            
            // Look input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            lookInput = new Vector2(mouseX, -mouseY);
            
            // Action inputs
            jumpPressed = Input.GetButtonDown("Jump");
            sprintPressed = Input.GetKey(KeyCode.LeftShift);
            interactPressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F);
            isRunning = Input.GetKey(KeyCode.LeftControl);
            
            // Vehicle controls
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (isInVehicle)
                {
                    ExitVehicle();
                }
                else
                {
                    TryEnterVehicle();
                }
            }
        }
        
        private void HandleMobileInput()
        {
            // Mobile input is handled by UI elements that set these values
            // This would be connected to on-screen joysticks and buttons
            
            // Example touch controls (would be set by UI components)
            jumpPressed = touchButtons[0];
            sprintPressed = touchButtons[1];
            interactPressed = touchButtons[2];
            
            // Reset touch buttons after reading
            for (int i = 0; i < touchButtons.Length; i++)
            {
                touchButtons[i] = false;
            }
        }
        
        private void HandleMovement()
        {
            // Check if grounded
            isGrounded = characterController.isGrounded;
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            
            // Calculate move direction
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            
            // Remove y component to keep movement horizontal
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            
            moveDirection = forward * moveInput.y + right * moveInput.x;
            
            // Apply speed based on movement type
            float currentSpeed = walkSpeed;
            isSprinting = false;
            
            if (sprintPressed && moveDirection.magnitude > 0.1f)
            {
                currentSpeed = sprintSpeed;
                isSprinting = true;
            }
            else if (isRunning && moveDirection.magnitude > 0.1f)
            {
                currentSpeed = runSpeed;
            }
            
            // Apply movement
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
            
            // Jumping
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;
            }
            else
            {
                isJumping = false;
            }
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
            
            // Rotate player to face movement direction
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        
        private void HandleCamera()
        {
            if (cameraTransform != null)
            {
                // Apply look input
                yRotation += lookInput.x;
                xRotation += lookInput.y;
                
                // Clamp vertical rotation
                xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
                
                // Apply rotation to camera
                cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
                
                // Position camera behind player
                Vector3 targetPosition = transform.position + Vector3.up * 1.8f - cameraTransform.forward * 3f;
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, 10f * Time.deltaTime);
            }
        }
        
        private void HandleInteraction()
        {
            if (interactPressed)
            {
                // Raycast for interactable objects
                Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, interactionRange, interactionLayerMask))
                {
                    // Check for interactable component
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact(this);
                    }
                }
            }
        }
        
        private void UpdateAnimations()
        {
            if (animator != null)
            {
                // Movement animations
                float speed = moveDirection.magnitude;
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetBool("IsRunning", isRunning);
                animator.SetBool("IsSprinting", isSprinting);
                animator.SetBool("IsJumping", isJumping);
                
                // Direction for blend trees
                Vector3 localMovement = transform.InverseTransformDirection(moveDirection);
                animator.SetFloat("Horizontal", localMovement.x);
                animator.SetFloat("Vertical", localMovement.z);
            }
        }
        
        // Public methods for mobile input
        public void SetMobileMovementInput(Vector2 input)
        {
            if (input.magnitude < joystickDeadZone)
                input = Vector2.zero;
            moveInput = input;
        }
        
        public void SetMobileLookInput(Vector2 input)
        {
            lookInput = input * touchSensitivity;
        }
        
        public void SetMobileButton(int buttonIndex, bool pressed)
        {
            if (buttonIndex >= 0 && buttonIndex < touchButtons.Length)
            {
                touchButtons[buttonIndex] = pressed;
            }
        }
        
        // Newport-specific methods (updated with vehicle system)
        public void EnterVehicle(GameObject vehicle)
        {
            // Handle entering vehicles (cars, boats for Newport river)
            Vehicles.VehicleController vehicleController = vehicle.GetComponent<Vehicles.VehicleController>();
            if (vehicleController != null)
            {
                EnterVehicle(vehicleController);
            }
        }
        
        public void ExitVehicle()
        {
            // Handle exiting vehicles
            if (isInVehicle && currentVehicle != null)
            {
                // Calculate exit position
                Vector3 exitPosition = currentVehicle.transform.position + currentVehicle.transform.right * exitOffset.x;
                exitPosition.y += exitOffset.y;
                
                // Check if exit position is safe
                if (!Physics.CheckSphere(exitPosition, 1f))
                {
                    transform.position = exitPosition;
                }
                else
                {
                    // Find alternative exit position
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f * Mathf.Deg2Rad;
                        Vector3 altExit = currentVehicle.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 3f;
                        
                        if (!Physics.CheckSphere(altExit, 1f))
                        {
                            transform.position = altExit;
                            break;
                        }
                    }
                }
                
                // Restore player control
                transform.parent = originalParent;
                characterController.enabled = true;
                currentVehicle.SetPlayerControlled(false);
                currentVehicle = null;
                isInVehicle = false;
                
                // Re-enable player controller
                enabled = true;
                
                // Update camera to follow player again
                if (cameraTransform != null)
                {
                    // Reset camera parent or position as needed
                }
            }
        }
        
        /// <summary>
        /// Try to enter a nearby vehicle
        /// </summary>
        private void TryEnterVehicle()
        {
            Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, vehicleEntryRange, vehicleLayerMask);
            
            if (nearbyVehicles.Length > 0)
            {
                // Find closest vehicle
                Collider closestVehicle = null;
                float closestDistance = float.MaxValue;
                
                foreach (Collider vehicleCollider in nearbyVehicles)
                {
                    float distance = Vector3.Distance(transform.position, vehicleCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestVehicle = vehicleCollider;
                    }
                }
                
                if (closestVehicle != null)
                {
                    Vehicles.VehicleController vehicleController = closestVehicle.GetComponent<Vehicles.VehicleController>();
                    if (vehicleController != null)
                    {
                        EnterVehicle(vehicleController);
                    }
                }
            }
        }
        
        /// <summary>
        /// Enter a specific vehicle
        /// </summary>
        /// <param name="vehicle">Vehicle controller to enter</param>
        private void EnterVehicle(Vehicles.VehicleController vehicle)
        {
            if (vehicle != null && !isInVehicle)
            {
                // Store original parent
                originalParent = transform.parent;
                
                // Disable character controller
                characterController.enabled = false;
                
                // Set vehicle as current
                currentVehicle = vehicle;
                isInVehicle = true;
                
                // Position player in vehicle
                transform.position = vehicle.transform.position + Vector3.up * 1f; // Adjust height as needed
                transform.parent = vehicle.transform;
                
                // Give vehicle control to player
                vehicle.SetPlayerControlled(true);
                
                // Notify police system if this is vehicle theft
                if (!vehicle.IsPlayerControlled)
                {
                    var policeSystem = FindObjectOfType<World.PoliceSystem>();
                    if (policeSystem != null)
                    {
                        policeSystem.OnVehicleTheft();
                    }
                }
                
                // Disable player movement controller while in vehicle
                enabled = false;
                
                Debug.Log($"Entered vehicle: {vehicle.VehicleName}");
            }
        }
        
        /// <summary>
        /// Get the nearest interactive vehicle
        /// </summary>
        /// <returns>Nearest vehicle controller or null</returns>
        public Vehicles.VehicleController GetNearestVehicle()
        {
            Collider[] nearbyVehicles = Physics.OverlapSphere(transform.position, vehicleEntryRange, vehicleLayerMask);
            
            if (nearbyVehicles.Length > 0)
            {
                Collider closestVehicle = null;
                float closestDistance = float.MaxValue;
                
                foreach (Collider vehicleCollider in nearbyVehicles)
                {
                    float distance = Vector3.Distance(transform.position, vehicleCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestVehicle = vehicleCollider;
                    }
                }
                
                if (closestVehicle != null)
                {
                    return closestVehicle.GetComponent<Vehicles.VehicleController>();
                }
            }
            
            return null;
        }
        
        public void StartSwimming()
        {
            // Handle swimming in Newport's White River
            gravity = -2f; // Reduced gravity for swimming
        }
        
        public void StopSwimming()
        {
            // Stop swimming
            gravity = -9.81f; // Normal gravity
        }
        
        // Getters
        public bool IsMoving => moveDirection.magnitude > 0.1f;
        public bool IsRunning => isRunning;
        public bool IsSprinting => isSprinting;
        public bool IsGrounded => isGrounded;
        public Vector3 Velocity => characterController.velocity;
    }
    
    // Interface for interactable objects
    public interface IInteractable
    {
        void Interact(PlayerController player);
        string GetInteractionPrompt();
    }
}