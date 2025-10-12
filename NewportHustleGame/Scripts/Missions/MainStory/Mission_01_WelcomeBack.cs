using UnityEngine;
using NewportHustle.Characters;

namespace NewportHustle.Missions.MainStory
{
    public class Mission_01_WelcomeBack : MonoBehaviour
    {
        [Header("Mission Info")]
        public string missionTitle = "Welcome Back to Newport";
        public string missionDescription = "You've returned to Newport, Arkansas after years away. Time to reacquaint yourself with the town and its people.";
        
        [Header("Objectives")]
        public bool visitDowntown = false;
        public bool talkToMentor = false;
        public bool exploreRiverfront = false;
        public bool findAccommodation = false;
        
        [Header("Mission Rewards")]
        public float moneyReward = 100f;
        public float respectReward = 10f;
        
        [Header("Newport Locations")]
        public Transform downtownArea;
        public Transform riverfrontArea;
        public Transform mentorLocation;
        public Transform[] accommodationOptions;
        
        private bool missionStarted = false;
        private bool missionCompleted = false;
        private int completedObjectives = 0;
        private const int totalObjectives = 4;
        
        void Start()
        {
            // This mission starts automatically when the game begins
            StartMission();
        }
        
        void Update()
        {
            if (missionStarted && !missionCompleted)
            {
                CheckObjectives();
            }
        }
        
        public void StartMission()
        {
            missionStarted = true;
            Debug.Log($"Mission Started: {missionTitle}");
            
            // Show mission introduction
            ShowMissionIntro();
            
            // Set up objective markers
            SetupObjectiveMarkers();
        }
        
        private void ShowMissionIntro()
        {
            // This would trigger a cutscene or dialogue
            // For now, just log the intro text
            string introText = @"
            Welcome back to Newport, Arkansas! 
            
            The White River still flows through town, and the familiar streets hold both memories and new opportunities. 
            You've been away for years, but Newport never forgot you. 
            
            Time to see what's changed and what's stayed the same in this small Arkansas town.
            
            Your old mentor has been asking about you - maybe you should pay them a visit.
            ";
            
            Debug.Log(introText);
            
            // Update HUD with mission info
            UpdateMissionHUD();
        }
        
        private void SetupObjectiveMarkers()
        {
            // Place objective markers on the map
            if (downtownArea != null)
            {
                CreateObjectiveMarker(downtownArea.position, "Visit Downtown Newport");
            }
            
            if (riverfrontArea != null)
            {
                CreateObjectiveMarker(riverfrontArea.position, "Explore the White River Waterfront");
            }
            
            if (mentorLocation != null)
            {
                CreateObjectiveMarker(mentorLocation.position, "Talk to Your Old Mentor");
            }
            
            foreach (Transform accommodation in accommodationOptions)
            {
                if (accommodation != null)
                {
                    CreateObjectiveMarker(accommodation.position, "Find a Place to Stay");
                }
            }
        }
        
        private void CreateObjectiveMarker(Vector3 position, string description)
        {
            // This would create a visual marker in the game world
            // For now, just create a simple marker object
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.position = position + Vector3.up * 2f;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.name = $"Objective: {description}";
            
            // Make it yellow and semi-transparent
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(1f, 1f, 0f, 0.7f);
                material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material = material;
            }
            
            // Add a simple up-down animation
            ObjectiveMarker markerScript = marker.AddComponent<ObjectiveMarker>();
            markerScript.description = description;
        }
        
        private void CheckObjectives()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            
            // Check downtown visit
            if (!visitDowntown && downtownArea != null)
            {
                if (Vector3.Distance(playerPos, downtownArea.position) < 10f)
                {
                    CompleteObjective("visitDowntown", "Downtown Newport Visited!");
                    visitDowntown = true;
                }
            }
            
            // Check riverfront exploration
            if (!exploreRiverfront && riverfrontArea != null)
            {
                if (Vector3.Distance(playerPos, riverfrontArea.position) < 15f)
                {
                    CompleteObjective("exploreRiverfront", "White River Waterfront Explored!");
                    exploreRiverfront = true;
                    
                    // Special Newport flavor text
                    Debug.Log("The White River flows peacefully here. You remember fishing these waters as a kid...");
                }
            }
            
            // Check mentor meeting (this would be triggered by dialogue system)
            // For now, check proximity
            if (!talkToMentor && mentorLocation != null)
            {
                if (Vector3.Distance(playerPos, mentorLocation.position) < 5f)
                {
                    TriggerMentorMeeting();
                }
            }
            
            // Check accommodation (simplified - just visiting one of the locations)
            if (!findAccommodation)
            {
                foreach (Transform accommodation in accommodationOptions)
                {
                    if (accommodation != null && Vector3.Distance(playerPos, accommodation.position) < 5f)
                    {
                        CompleteObjective("findAccommodation", "Accommodation Found!");
                        findAccommodation = true;
                        break;
                    }
                }
            }
            
            // Check mission completion
            if (visitDowntown && talkToMentor && exploreRiverfront && findAccommodation && !missionCompleted)
            {
                CompleteMission();
            }
        }
        
        private void TriggerMentorMeeting()
        {
            // This would normally trigger through the dialogue system
            // For now, simulate the interaction
            CompleteObjective("talkToMentor", "Reunited with Your Mentor!");
            talkToMentor = true;
            
            // Newport-specific mentor dialogue preview
            string mentorText = @"
            Well, well... look who's back in Newport! 
            
            I heard through the grapevine you were coming home. This town's changed some since you left, 
            but the heart of it's still the same. The barbershops on Main Street are busier than ever, 
            and that new spa district is bringing in folks from Little Rock and Memphis.
            
            You picked a good time to come back. There's opportunity here for someone with ambition. 
            Just remember - in Newport, your reputation travels fast. The folks here have long memories, 
            but they're also quick to give someone a second chance if they prove themselves.
            
            What do you say we get you settled in and then I'll show you around? 
            Things have been happening down by the river you'll want to see...
            ";
            
            Debug.Log(mentorText);
        }
        
        private void CompleteObjective(string objectiveName, string completionMessage)
        {
            completedObjectives++;
            Debug.Log($"Objective Complete: {completionMessage}");
            
            // Update HUD
            UpdateMissionHUD();
            
            // Give small reward for each objective
            GameManager.Instance.UpdatePlayerMoney(25f);
            GameManager.Instance.UpdatePlayerRespect(2f);
        }
        
        private void CompleteMission()
        {
            missionCompleted = true;
            Debug.Log($"Mission Complete: {missionTitle}");
            
            // Give mission rewards
            GameManager.Instance.UpdatePlayerMoney(moneyReward);
            GameManager.Instance.UpdatePlayerRespect(respectReward);
            
            // Show completion message
            string completionText = @"
            Mission Complete: Welcome Back to Newport!
            
            You've successfully reacquainted yourself with Newport. The town feels familiar yet different, 
            full of new possibilities. Your mentor's words echo in your mind - reputation travels fast here, 
            but so does opportunity.
            
            The White River continues to flow, carrying with it the promise of new adventures and the whispers 
            of old secrets. Newport is ready for you, and you're ready for Newport.
            
            What will you make of your homecoming?
            ";
            
            Debug.Log(completionText);
            
            // Unlock next mission
            UnlockNextMission();
            
            // Clean up objective markers
            CleanupObjectiveMarkers();
        }
        
        private void UnlockNextMission()
        {
            // Unlock Mission 02: The Cut
            GameObject nextMission = GameObject.Find("Mission_02_TheCut");
            if (nextMission != null)
            {
                Mission_02_TheCut mission02 = nextMission.GetComponent<Mission_02_TheCut>();
                if (mission02 != null)
                {
                    mission02.UnlockMission();
                }
            }
        }
        
        private void CleanupObjectiveMarkers()
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("ObjectiveMarker");
            foreach (GameObject marker in markers)
            {
                Destroy(marker);
            }
        }
        
        private void UpdateMissionHUD()
        {
            // This would update the actual HUD
            // For now, just log progress
            Debug.Log($"Mission Progress: {completedObjectives}/{totalObjectives} objectives completed");
        }
        
        // Public methods for external systems
        public bool IsMissionCompleted()
        {
            return missionCompleted;
        }
        
        public float GetMissionProgress()
        {
            return (float)completedObjectives / totalObjectives;
        }
        
        public string GetCurrentObjective()
        {
            if (!visitDowntown) return "Visit Downtown Newport";
            if (!exploreRiverfront) return "Explore the White River Waterfront";
            if (!talkToMentor) return "Talk to Your Old Mentor";
            if (!findAccommodation) return "Find a Place to Stay";
            return "Mission Complete!";
        }
    }
    
    // Helper component for objective markers
    public class ObjectiveMarker : MonoBehaviour
    {
        public string description;
        private float bobSpeed = 2f;
        private float bobHeight = 0.5f;
        private Vector3 startPosition;
        
        void Start()
        {
            startPosition = transform.position;
            gameObject.tag = "ObjectiveMarker";
        }
        
        void Update()
        {
            // Simple bobbing animation
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            
            // Rotate slowly
            transform.Rotate(Vector3.up * 30f * Time.deltaTime);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Show objective description
                Debug.Log($"Objective: {description}");
            }
        }
    }
}