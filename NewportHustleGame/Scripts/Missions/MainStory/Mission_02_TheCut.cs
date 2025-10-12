using UnityEngine;
using NewportHustle.Characters;

namespace NewportHustle.Missions.MainStory
{
    public class Mission_02_TheCut : MonoBehaviour
    {
        [Header("Mission Info")]
        public string missionTitle = "The Cut";
        public string missionDescription = "Your mentor has a proposition. There's money to be made in Newport's thriving barbershop scene, but first you need to prove yourself worthy of the community's trust.";
        
        [Header("Objectives")]
        public bool visitBarberDistrict = false;
        public bool meetBarbershopOwner = false;
        public bool completeTestCut = false;
        public bool earnFirstTips = false;
        public bool gainCommunityRespect = false;
        
        [Header("Mission Rewards")]
        public float moneyReward = 250f;
        public float respectReward = 20f;
        public float barbershopReputation = 25f;
        
        [Header("Newport Barber Locations")]
        public Transform barberDistrictCenter;
        public Transform[] barbershopLocations;
        public Transform[] barberNPCLocations;
        
        [Header("Mission NPCs")]
        public NPCBehavior barbershopOwnerNPC;
        public NPCBehavior[] communityMembersNPCs;
        
        private bool missionUnlocked = false;
        private bool missionStarted = false;
        private bool missionCompleted = false;
        private int completedObjectives = 0;
        private const int totalObjectives = 5;
        
        private int successfulCuts = 0;
        private int requiredCuts = 3;
        private float tipTotal = 0f;
        private float requiredTips = 50f;
        
        void Start()
        {
            // This mission is unlocked by Mission 01
            if (!missionUnlocked)
            {
                gameObject.SetActive(false);
            }
        }
        
        void Update()
        {
            if (missionStarted && !missionCompleted)
            {
                CheckObjectives();
            }
        }
        
        public void UnlockMission()
        {
            missionUnlocked = true;
            gameObject.SetActive(true);
            
            // Wait a moment, then start the mission
            Invoke("StartMission", 2f);
        }
        
        public void StartMission()
        {
            if (!missionUnlocked) return;
            
            missionStarted = true;
            Debug.Log($"Mission Started: {missionTitle}");
            
            ShowMissionIntro();
            SetupObjectiveMarkers();
        }
        
        private void ShowMissionIntro()
        {
            string introText = @"
            Your mentor pulls you aside with a serious look...
            
            'Listen, you want to make it in Newport? You need to understand this town runs on respect and reputation. 
            The barbershop district on Main Street - that's where real business happens. 
            
            It's not just about cutting hair. It's about being part of the community, earning trust, 
            and proving you can handle yourself in this town. The old-timers there have been cutting hair 
            since before you were born, and they don't just let anyone join their ranks.
            
            But here's the thing - business is booming. Folks come from all over Jackson County to get their cuts here. 
            There's money to be made, but first you need to prove yourself.
            
            Go down to Barber's Row, introduce yourself properly, and show them what you're made of. 
            If you can earn their respect, doors will open for you all over Newport.'
            
            Your mentor hands you an old leather case.
            
            'These were my father's tools. He cut hair in Newport for 40 years. 
            Time they found a new purpose.'
            ";
            
            Debug.Log(introText);
            UpdateMissionHUD();
        }
        
        private void SetupObjectiveMarkers()
        {
            if (barberDistrictCenter != null)
            {
                CreateObjectiveMarker(barberDistrictCenter.position, "Visit Barber's Row");
            }
            
            foreach (Transform barbershop in barbershopLocations)
            {
                if (barbershop != null)
                {
                    CreateObjectiveMarker(barbershop.position, "Find the Head Barber");
                }
            }
        }
        
        private void CreateObjectiveMarker(Vector3 position, string description)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.position = position + Vector3.up * 2f;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.name = $"Objective: {description}";
            marker.tag = "ObjectiveMarker";
            
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0f, 1f, 1f, 0.7f); // Cyan for mission 02
                material.SetFloat("_Mode", 3);
                renderer.material = material;
            }
            
            ObjectiveMarker markerScript = marker.AddComponent<ObjectiveMarker>();
            markerScript.description = description;
        }
        
        private void CheckObjectives()
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            
            // Check barber district visit
            if (!visitBarberDistrict && barberDistrictCenter != null)
            {
                if (Vector3.Distance(playerPos, barberDistrictCenter.position) < 15f)
                {
                    CompleteObjective("visitBarberDistrict", "Arrived at Barber's Row!");
                    visitBarberDistrict = true;
                    
                    ShowBarberDistrictDescription();
                }
            }
            
            // Check barbershop owner meeting
            if (!meetBarbershopOwner && visitBarberDistrict)
            {
                if (barbershopOwnerNPC != null && Vector3.Distance(playerPos, barbershopOwnerNPC.transform.position) < 5f)
                {
                    TriggerBarbershopOwnerMeeting();
                }
            }
            
            // Other objectives are triggered by specific actions
            CheckSkillProgression();
        }
        
        private void ShowBarberDistrictDescription()
        {
            string districtText = @"
            Barber's Row - Main Street, Newport
            
            The smell of aftershave and pomade fills the air as you walk down this historic stretch of Main Street. 
            Red, white, and blue barber poles spin lazily in the Arkansas breeze, each one marking a business 
            that's been serving Newport families for generations.
            
            'Thompson's Traditional Cuts' - Est. 1952
            'Willie's Riverside Barbershop' - 'Best Cut in Jackson County'
            'The Newport Clipper' - 'Where Style Meets Tradition'
            
            Through the windows, you can see old-timers in white coats working their magic, 
            scissors dancing through hair with practiced precision. The conversations flow as smoothly as the cuts - 
            talk of fishing the White River, high school football, and the changing face of Newport.
            
            This is where reputations are made and broken with every snip of the scissors.
            ";
            
            Debug.Log(districtText);
        }
        
        private void TriggerBarbershopOwnerMeeting()
        {
            CompleteObjective("meetBarbershopOwner", "Met the Head Barber!");
            meetBarbershopOwner = true;
            
            string ownerText = @"
            The head barber, a weathered man with kind eyes and steady hands, looks up from his work...
            
            'So you're the one [Mentor's Name] was telling me about. Says you've got potential, 
            but potential don't mean much until it's proven. 
            
            I've been cutting hair in Newport since the White River flooded back in '82. 
            Seen a lot of folks come through that door thinking they know the trade. 
            Most of them don't last a week.
            
            But [Mentor's Name] don't recommend just anybody, so I'll tell you what - 
            you prove you can handle the basics, treat my customers with respect, 
            and maybe we can work something out.
            
            First test: give me three clean cuts. Nothing fancy, just solid work. 
            Show me you understand that every person in that chair is trusting you with how they present themselves to this town.
            
            In Newport, a good haircut ain't just about looking good - it's about feeling like you belong.'
            
            The owner hands you a clean apron and points to the empty chair by the window.
            
            'The afternoon crowd will be here soon. Let's see what you've got.'
            ";
            
            Debug.Log(ownerText);
            
            // Enable haircut mini-game mechanics
            EnableHaircutSystem();
        }
        
        private void EnableHaircutSystem()
        {
            // This would enable the haircut mini-game
            // For now, simulate the process
            Debug.Log("Haircut system enabled! Approach customers to offer services.");
        }
        
        private void CheckSkillProgression()
        {
            // Check test cuts completion
            if (!completeTestCut && meetBarbershopOwner && successfulCuts >= requiredCuts)
            {
                CompleteObjective("completeTestCut", "Completed Test Cuts!");
                completeTestCut = true;
                
                string testPassText = @"
                The head barber nods approvingly as you finish the third cut...
                
                'Not bad. Not bad at all. You've got steady hands and you listen to what the customer wants. 
                That last gentleman said you reminded him of his regular barber from back in Batesville.
                
                You're starting to understand - this isn't just a job, it's about being part of the community. 
                Every person who sits in that chair has a story, and part of our job is making them feel heard.
                
                Keep it up, and you might just make it in this business.'
                ";
                
                Debug.Log(testPassText);
            }
            
            // Check tips earned
            if (!earnFirstTips && completeTestCut && tipTotal >= requiredTips)
            {
                CompleteObjective("earnFirstTips", "Earned Your First Tips!");
                earnFirstTips = true;
                
                Debug.Log($"You've earned ${tipTotal:F2} in tips! The customers appreciate your work.");
            }
            
            // Check community respect (based on NPC relationships)
            if (!gainCommunityRespect && earnFirstTips)
            {
                float averageRespect = CalculateAverageCommunityRespect();
                if (averageRespect >= 10f)
                {
                    CompleteObjective("gainCommunityRespect", "Gained Community Respect!");
                    gainCommunityRespect = true;
                }
            }
            
            // Check mission completion
            if (visitBarberDistrict && meetBarbershopOwner && completeTestCut && earnFirstTips && gainCommunityRespect && !missionCompleted)
            {
                CompleteMission();
            }
        }
        
        private float CalculateAverageCommunityRespect()
        {
            if (communityMembersNPCs == null || communityMembersNPCs.Length == 0)
                return 0f;
            
            float totalRespect = 0f;
            int validNPCs = 0;
            
            foreach (NPCBehavior npc in communityMembersNPCs)
            {
                if (npc != null)
                {
                    totalRespect += npc.relationshipWithPlayer;
                    validNPCs++;
                }
            }
            
            return validNPCs > 0 ? totalRespect / validNPCs : 0f;
        }
        
        private void CompleteObjective(string objectiveName, string completionMessage)
        {
            completedObjectives++;
            Debug.Log($"Objective Complete: {completionMessage}");
            
            UpdateMissionHUD();
            
            // Give objective rewards
            GameManager.Instance.UpdatePlayerMoney(50f);
            GameManager.Instance.UpdatePlayerRespect(4f);
        }
        
        private void CompleteMission()
        {
            missionCompleted = true;
            Debug.Log($"Mission Complete: {missionTitle}");
            
            // Give mission rewards
            GameManager.Instance.UpdatePlayerMoney(moneyReward);
            GameManager.Instance.UpdatePlayerRespect(respectReward);
            
            string completionText = @"
            Mission Complete: The Cut
            
            The head barber extends his hand with a genuine smile...
            
            'Well, I'll be damned. You actually did it. Not only did you prove you can handle the scissors, 
            but you earned the respect of some of the most particular customers in Newport. 
            
            Mrs. Henderson hasn't complimented a haircut in fifteen years, and she told me you reminded her 
            of her late husband - and he was a barber here for thirty years.
            
            You've got a place here on Barber's Row if you want it. The pay's fair, the work's honest, 
            and you'll be part of something that's been the backbone of this community since before 
            the interstate came through.
            
            But I get the feeling you've got bigger plans brewing. That's alright - Newport's got room 
            for ambitious folks, long as they remember where they came from.
            
            You keep those tools. You've earned them.'
            
            The afternoon sun streams through the barbershop window as you look out at Main Street. 
            Newport feels different now - less like a place you're visiting, more like a place you belong.
            
            The Cut is complete. Your reputation in Newport has begun.
            ";
            
            Debug.Log(completionText);
            
            // Unlock next mission
            UnlockNextMission();
            CleanupObjectiveMarkers();
        }
        
        private void UnlockNextMission()
        {
            GameObject nextMission = GameObject.Find("Mission_03_SpaSecrets");
            if (nextMission != null)
            {
                Mission_03_SpaSecrets mission03 = nextMission.GetComponent<Mission_03_SpaSecrets>();
                if (mission03 != null)
                {
                    mission03.UnlockMission();
                }
            }
        }
        
        private void CleanupObjectiveMarkers()
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("ObjectiveMarker");
            foreach (GameObject marker in markers)
            {
                if (marker.name.Contains("Mission_02") || marker.name.Contains("The Cut"))
                {
                    Destroy(marker);
                }
            }
        }
        
        private void UpdateMissionHUD()
        {
            Debug.Log($"Mission Progress: {completedObjectives}/{totalObjectives} objectives completed");
        }
        
        // Public methods for haircut system
        public void RecordSuccessfulCut(float tipAmount)
        {
            successfulCuts++;
            tipTotal += tipAmount;
            
            Debug.Log($"Successful cut recorded! Tips earned: ${tipAmount:F2} | Total: {successfulCuts}/{requiredCuts} cuts, ${tipTotal:F2} tips");
        }
        
        public void AddCommunityRespect(NPCBehavior npc, float amount)
        {
            if (npc != null)
            {
                npc.ChangeRelationship(amount);
                Debug.Log($"Relationship with {npc.npcName} improved by {amount}");
            }
        }
        
        // Getters
        public bool IsMissionCompleted() => missionCompleted;
        public float GetMissionProgress() => (float)completedObjectives / totalObjectives;
        
        public string GetCurrentObjective()
        {
            if (!visitBarberDistrict) return "Visit Barber's Row";
            if (!meetBarbershopOwner) return "Meet the Head Barber";
            if (!completeTestCut) return $"Complete Test Cuts ({successfulCuts}/{requiredCuts})";
            if (!earnFirstTips) return $"Earn Tips (${tipTotal:F2}/${requiredTips:F2})";
            if (!gainCommunityRespect) return "Gain Community Respect";
            return "Mission Complete!";
        }
    }
}