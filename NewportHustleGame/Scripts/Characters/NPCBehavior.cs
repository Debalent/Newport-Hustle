using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace NewportHustle.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCBehavior : MonoBehaviour
    {
        [Header("NPC Identity")]
        public string npcName = "Newport Citizen";
        public NPCType npcType = NPCType.Civilian;
        public string dialogueID = "";
        
        [Header("Behavior Settings")]
        public NPCBehaviorState currentState = NPCBehaviorState.Idle;
        public float detectionRange = 10f;
        public float interactionRange = 3f;
        public float wanderRadius = 20f;
        public float stateChangeInterval = 30f;
        
        [Header("Newport-Specific")]
        public NewportDistrict homeDistrict = NewportDistrict.Downtown;
        public BusinessType associatedBusiness = BusinessType.None;
        public float relationshipWithPlayer = 0f; // -100 to 100
        
        [Header("Daily Schedule")]
        public List<NPCScheduleEntry> dailySchedule = new List<NPCScheduleEntry>();
        
        // Components
        private NavMeshAgent agent;
        private Animator animator;
        private PlayerController player;
        private DialogueSystem dialogueSystem;
        
        // Behavior variables
        private Vector3 startPosition;
        private Vector3 wanderTarget;
        private float stateTimer;
        private bool playerInRange;
        private bool isInteracting;
        
        // Newport behaviors
        private bool isWorkingHours;
        private Vector3 workLocation;
        private Vector3 homeLocation;

        // Cached component references (avoid FindObjectOfType every frame)
        private TimeCycle cachedTimeCycle;
        
        void Start()
        {
            InitializeNPC();
        }
        
        void Update()
        {
            CheckPlayerProximity();
            HandleCurrentState();
            UpdateAnimations();
            CheckSchedule();
        }
        
        private void InitializeNPC()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            player = FindObjectOfType<PlayerController>();
            dialogueSystem = FindObjectOfType<DialogueSystem>();
            
            startPosition = transform.position;
            homeLocation = startPosition;

            // Cache TimeCycle once — FindObjectOfType is expensive per-frame
            cachedTimeCycle = FindObjectOfType<TimeCycle>();

            // Set work location based on business type
            SetWorkLocation();
            
            // Initialize based on NPC type
            InitializeNPCType();
            
            // Start with appropriate behavior
            ChangeState(NPCBehaviorState.Idle);
        }
        
        private void SetWorkLocation()
        {
            switch (associatedBusiness)
            {
                case BusinessType.Barbershop:
                    // Find barbershop in Barbers Row
                    workLocation = FindBusinessLocation("BarbershopLocation");
                    break;
                case BusinessType.Spa:
                    // Find spa in Spa District
                    workLocation = FindBusinessLocation("SpaLocation");
                    break;
                case BusinessType.Restaurant:
                    workLocation = FindBusinessLocation("RestaurantLocation");
                    break;
                case BusinessType.Shop:
                    workLocation = FindBusinessLocation("ShopLocation");
                    break;
                default:
                    workLocation = startPosition;
                    break;
            }
        }
        
        private Vector3 FindBusinessLocation(string locationTag)
        {
            GameObject location = GameObject.FindGameObjectWithTag(locationTag);
            return location != null ? location.transform.position : startPosition;
        }
        
        private void InitializeNPCType()
        {
            switch (npcType)
            {
                case NPCType.Mentor:
                    detectionRange = 15f;
                    relationshipWithPlayer = 50f;
                    break;
                case NPCType.Rival:
                    relationshipWithPlayer = -30f;
                    break;
                case NPCType.BarbershopOwner:
                    associatedBusiness = BusinessType.Barbershop;
                    relationshipWithPlayer = 20f;
                    break;
                case NPCType.SpaOwner:
                    associatedBusiness = BusinessType.Spa;
                    relationshipWithPlayer = 10f;
                    break;
                case NPCType.PoliceContact:
                    detectionRange = 20f;
                    break;
            }
        }
        
        private void CheckPlayerProximity()
        {
            if (player != null)
            {
                // Use sqrMagnitude instead of Distance — avoids a sqrt per NPC per frame
                float sqrDist = (transform.position - player.transform.position).sqrMagnitude;
                playerInRange = sqrDist <= detectionRange * detectionRange;

                // Smooth look-at when player is in interaction range
                if (sqrDist <= interactionRange * interactionRange && !isInteracting)
                {
                    Vector3 lookDirection = player.transform.position - transform.position;
                    lookDirection.y = 0;
                    if (lookDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        transform.rotation = Quaternion.RotateTowards(
                            transform.rotation, targetRotation, 180f * Time.deltaTime);
                    }
                }
            }
        }
        
        private void HandleCurrentState()
        {
            stateTimer += Time.deltaTime;
            
            switch (currentState)
            {
                case NPCBehaviorState.Idle:
                    HandleIdleState();
                    break;
                case NPCBehaviorState.Wandering:
                    HandleWanderingState();
                    break;
                case NPCBehaviorState.Working:
                    HandleWorkingState();
                    break;
                case NPCBehaviorState.GoingHome:
                    HandleGoingHomeState();
                    break;
                case NPCBehaviorState.Talking:
                    HandleTalkingState();
                    break;
                case NPCBehaviorState.Fleeing:
                    HandleFleeingState();
                    break;
            }
            
            // Random state changes
            if (stateTimer >= stateChangeInterval && !isInteracting)
            {
                ChooseNewState();
            }
        }
        
        private void HandleIdleState()
        {
            agent.SetDestination(transform.position);
            
            if (playerInRange && !isInteracting)
            {
                // React to player based on relationship
                if (relationshipWithPlayer < -50f)
                {
                    ChangeState(NPCBehaviorState.Fleeing);
                }
                else if (relationshipWithPlayer > 0f)
                {
                    // Friendly greeting animation
                    if (animator != null)
                    {
                        animator.SetTrigger("Wave");
                    }
                }
            }
        }
        
        private void HandleWanderingState()
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetRandomWanderTarget();
            }
        }
        
        private void HandleWorkingState()
        {
            agent.SetDestination(workLocation);

            if ((transform.position - workLocation).sqrMagnitude < 4f) // 2f squared
            {
                if (animator != null)
                {
                    animator.SetBool("IsWorking", true);
                }
            }
        }

        private void HandleGoingHomeState()
        {
            agent.SetDestination(homeLocation);

            if ((transform.position - homeLocation).sqrMagnitude < 4f) // 2f squared
            {
                ChangeState(NPCBehaviorState.Idle);
            }
        }
        
        private void HandleTalkingState()
        {
            agent.SetDestination(transform.position);
            // Talking state is managed by dialogue system
        }
        
        private void HandleFleeingState()
        {
            if (player == null)
            {
                ChangeState(NPCBehaviorState.Idle);
                return;
            }

            Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * 10f;

            agent.SetDestination(fleeTarget);

            if (!playerInRange)
            {
                ChangeState(NPCBehaviorState.Idle);
            }
        }
        
        private void CheckSchedule()
        {
            if (dailySchedule.Count == 0) return;

            if (cachedTimeCycle == null) return;

            float currentTime = cachedTimeCycle.GetCurrentTime();
            
            foreach (var entry in dailySchedule)
            {
                if (currentTime >= entry.startTime && currentTime < entry.endTime)
                {
                    if (currentState != entry.activity)
                    {
                        ChangeState(entry.activity);
                    }
                    break;
                }
            }
        }
        
        private void ChooseNewState()
        {
            isWorkingHours = cachedTimeCycle != null && cachedTimeCycle.IsBusinessHours();
            
            // Choose state based on time and NPC type
            if (isWorkingHours && associatedBusiness != BusinessType.None)
            {
                ChangeState(NPCBehaviorState.Working);
            }
            else if (!isWorkingHours && Random.value < 0.3f)
            {
                ChangeState(NPCBehaviorState.GoingHome);
            }
            else
            {
                ChangeState(NPCBehaviorState.Wandering);
            }
        }
        
        private void ChangeState(NPCBehaviorState newState)
        {
            currentState = newState;
            stateTimer = 0f;
            
            if (animator != null)
            {
                animator.SetBool("IsWorking", newState == NPCBehaviorState.Working);
                animator.SetBool("IsWalking", newState == NPCBehaviorState.Wandering || newState == NPCBehaviorState.GoingHome);
            }
        }
        
        private void SetRandomWanderTarget()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += startPosition;
            randomDirection.y = startPosition.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
            {
                agent.SetDestination(hit.position);
            }
        }
        
        private void UpdateAnimations()
        {
            if (animator != null)
            {
                float speed = agent.velocity.magnitude;
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsMoving", speed > 0.1f);
            }
        }
        
        // Interaction methods
        public void StartDialogue()
        {
            if (dialogueSystem != null && !string.IsNullOrEmpty(dialogueID))
            {
                isInteracting = true;
                ChangeState(NPCBehaviorState.Talking);
                dialogueSystem.StartDialogue(dialogueID, this);
            }
        }
        
        public void EndDialogue()
        {
            isInteracting = false;
            ChangeState(NPCBehaviorState.Idle);
        }
        
        public void ChangeRelationship(float amount)
        {
            relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + amount, -100f, 100f);
        }
        
        // Newport-specific methods
        public void ReactToWeather(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Rain:
                    // NPCs seek shelter
                    if (currentState == NPCBehaviorState.Wandering)
                    {
                        ChangeState(NPCBehaviorState.GoingHome);
                    }
                    break;
                case WeatherType.Clear:
                    // More likely to wander outside
                    if (Random.value < 0.5f && currentState == NPCBehaviorState.Idle)
                    {
                        ChangeState(NPCBehaviorState.Wandering);
                    }
                    break;
            }
        }
        
        public void ReactToPlayerActions(string action)
        {
            switch (action)
            {
                case "help":
                    ChangeRelationship(5f);
                    break;
                case "crime":
                    ChangeRelationship(-10f);
                    if (npcType == NPCType.PoliceContact)
                    {
                        // Call police
                    }
                    break;
                case "business":
                    if (associatedBusiness != BusinessType.None)
                    {
                        ChangeRelationship(2f);
                    }
                    break;
            }
        }
        
        // Getters
        public bool CanInteract => Vector3.Distance(transform.position, player.transform.position) <= interactionRange;
        public string GetInteractionPrompt() => $"Talk to {npcName}";
    }
    
    [System.Serializable]
    public class NPCScheduleEntry
    {
        public float startTime;
        public float endTime;
        public NPCBehaviorState activity;
        public string location;
    }
    
    public enum NPCType
    {
        Civilian,
        Mentor,
        Rival,
        BarbershopOwner,
        SpaOwner,
        SideHustler,
        PoliceContact,
        SpaClient,
        Wildcard
    }
    
    public enum NPCBehaviorState
    {
        Idle,
        Wandering,
        Working,
        GoingHome,
        Talking,
        Fleeing,
        Shopping,
        Socializing
    }
    
    public enum NewportDistrict
    {
        Downtown,
        Riverfront,
        Residential,
        SpaDistrict,
        BarbersRow,
        DiazDistrict,
        JacksonportArea
    }
    
    public enum BusinessType
    {
        None,
        Barbershop,
        Spa,
        Restaurant,
        Shop,
        Service
    }
}