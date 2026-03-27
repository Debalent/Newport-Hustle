using UnityEngine;
using NewportHustle.Characters;
using NewportHustle.Core;

namespace NewportHustle.Missions.MainStory
{
    /// <summary>
    /// Main story mission: "Welcome to the DeVille District"
    /// Early Marcus DeVille mission where Ace and Trix steal a shipment
    /// of counterfeit luxury belts from a rival warehouse.
    ///
    /// This script follows the same pattern as Mission_01 and Mission_02:
    ///  - Auto / manual StartMission()
    ///  - Distance based objective checks
    ///  - Simple in-world objective markers
    ///  - HUD logging via UpdateMissionHUD()
    /// </summary>
    public class Mission_DeVille_WelcomeToDistrict : MonoBehaviour
    {
        [Header("Mission Info")]
        public string missionTitle = "Welcome to the DeVille District";
        [TextArea(3, 6)]
        public string missionDescription = "Meet Marcus DeVille with Trix, hit a rival warehouse in Diaz District, and steal back his counterfeit belt shipment.";

        [Header("Objectives")]
        public bool meetMarcus = false;
        public bool reachWarehouse = false;
        public bool secureShipment = false;
        public bool escapeWarehouse = false;
        public bool deliverShipment = false;

        [Header("Mission Rewards")]
        public float moneyReward = 300f;
        public float respectReward = 15f;

        [Header("Key Characters")]
        public NPCBehavior marcusNPC;
        [Tooltip("Optional override for Marcus' intro dialogue ID. If empty, uses Marcus NPC's configured dialogueID.")]
        public string marcusDialogueIDOverride;

        [Header("Key Locations")]
        public Transform marcusMeetPoint;
        public Transform rivalWarehouseEntrance;
        public Transform escapeVehicleSpawn;
        public Transform deliveryDropoffPoint;

        [Header("Gameplay Settings")]
        public float meetRadius = 5f;
        public float warehouseRadius = 10f;
        public float dropoffRadius = 7f;

        private bool missionUnlocked = false;
        private bool missionStarted = false;
        private bool missionCompleted = false;
        private int completedObjectives = 0;
        private const int totalObjectives = 5;

        private bool marcusDialogueStarted = false;

        void Update()
        {
            if (missionStarted && !missionCompleted)
            {
                CheckObjectives();
            }
        }

        /// <summary>
        /// Call this from a trigger, cutscene controller, or another mission
        /// to begin the DeVille introduction mission.
        /// </summary>
        public void StartMission()
        {
            if (missionStarted || !missionUnlocked) return;

            missionStarted = true;
            Debug.Log($"Mission Started: {missionTitle}");

            // Ensure Marcus has the correct dialogue ID if an override is provided
            if (marcusNPC != null && !string.IsNullOrEmpty(marcusDialogueIDOverride))
            {
                marcusNPC.dialogueID = marcusDialogueIDOverride;
            }

            ShowMissionIntro();
            SetupObjectiveMarkers();
        }

        /// <summary>
        /// Unlocks this mission so that it can be started by triggers or other scripts.
        /// </summary>
        public void UnlockMission()
        {
            missionUnlocked = true;
            Debug.Log("Mission unlocked: " + missionTitle);
        }

        private void ShowMissionIntro()
        {
            string introText = @"
            Cutscene: Night settles over Newport as neon from the DeVille District spills across the cracked pavement.

            Trix leans against a street pole, phone in hand, as a matte-black Cadillac DeVille glides to a stop.
            Marcus DeVille steps out, adjusting a gold belt buckle the size of a dinner plate.

            MARCUS: 'Look at you. Fresh off the bus and already smelling like potential.
            Welcome to the DeVille District — where dreams come true and wallets go missing.'

            TRIX: 'Translation: you're about to do something sketchy for exposure.'

            MARCUS: 'My rivals stole my belts. My belts. A man without a belt is like a car without rims —
            technically functional, but embarrassing.

            You and Trix are gonna pay a visit to my friends in Diaz District and remind them
            whose logo runs this town.'
            ";

            Debug.Log(introText);
            UpdateMissionHUD();
        }

        private void SetupObjectiveMarkers()
        {
            // Meet Marcus
            if (marcusMeetPoint != null)
            {
                CreateObjectiveMarker(marcusMeetPoint.position, "Meet Marcus and Trix");
            }

            // Rival warehouse
            if (rivalWarehouseEntrance != null)
            {
                CreateObjectiveMarker(rivalWarehouseEntrance.position, "Reach the rival warehouse in Diaz District");
            }

            // Delivery dropoff
            if (deliveryDropoffPoint != null)
            {
                CreateObjectiveMarker(deliveryDropoffPoint.position, "Deliver the stolen shipment to DeVille's block");
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
                material.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange tint for DeVille missions
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

            // 1) Meet Marcus
            if (!meetMarcus && marcusMeetPoint != null)
            {
                if (Vector3.Distance(playerPos, marcusMeetPoint.position) < meetRadius)
                {
                    meetMarcus = true;
                    CompleteObjective("meetMarcus", "Met Marcus DeVille in his district.");

                    TriggerMarcusIntroDialogue();
                    Debug.Log("Marcus: 'Look at that. The kid showed up on time. That's already better than half my staff.'");
                }
            }

            // 2) Reach warehouse
            if (meetMarcus && !reachWarehouse && rivalWarehouseEntrance != null)
            {
                if (Vector3.Distance(playerPos, rivalWarehouseEntrance.position) < warehouseRadius)
                {
                    reachWarehouse = true;
                    CompleteObjective("reachWarehouse", "Reached the rival warehouse in Diaz District.");

                    Debug.Log("Trix: 'Alright, we slip in, grab the boxes, and pray these dudes don't have health insurance.'");
                }
            }

            // 3) Secure shipment (placeholder)
            // In a full implementation, this would be driven by loot pickup / interaction events.
            if (reachWarehouse && !secureShipment)
            {
                // For now, simulate securing the shipment when the player spends a short time near the entrance.
                // Hook this to your actual crate/interaction logic later.
                secureShipment = true;
                CompleteObjective("secureShipment", "Secured the counterfeit belt shipment.");

                Debug.Log("Ace loads the crates while Marcus rants over the phone about 'brand violations'.");
            }

            // 4) Escape warehouse (placeholder)
            if (secureShipment && !escapeWarehouse && escapeVehicleSpawn != null)
            {
                if (Vector3.Distance(playerPos, escapeVehicleSpawn.position) < warehouseRadius)
                {
                    escapeWarehouse = true;
                    CompleteObjective("escapeWarehouse", "Escaped the warehouse area with the stolen goods.");

                    Debug.Log("Trix: 'Floor it. If they catch us, Marcus is gonna pretend he never knew us.'");
                }
            }

            // 5) Deliver shipment
            if (escapeWarehouse && !deliverShipment && deliveryDropoffPoint != null)
            {
                if (Vector3.Distance(playerPos, deliveryDropoffPoint.position) < dropoffRadius)
                {
                    deliverShipment = true;
                    CompleteObjective("deliverShipment", "Delivered the shipment back to DeVille's block.");

                    CompleteMission();
                }
            }
        }

        private void TriggerMarcusIntroDialogue()
        {
            if (marcusDialogueStarted) return;

            if (marcusNPC != null)
            {
                if (!string.IsNullOrEmpty(marcusDialogueIDOverride))
                {
                    marcusNPC.dialogueID = marcusDialogueIDOverride;
                }

                marcusNPC.StartDialogue();
                marcusDialogueStarted = true;
            }
        }

        private void CompleteObjective(string objectiveName, string completionMessage)
        {
            completedObjectives++;
            Debug.Log($"Objective Complete: {completionMessage}");
            UpdateMissionHUD();

            // Small per-objective reward
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePlayerMoney(40f);
                GameManager.Instance.UpdatePlayerRespect(3f);
            }
        }

        private void CompleteMission()
        {
            if (missionCompleted) return;

            missionCompleted = true;
            Debug.Log($"Mission Complete: {missionTitle}");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePlayerMoney(moneyReward);
                GameManager.Instance.UpdatePlayerRespect(respectReward);
            }

            string completionText = @"
            Mission Complete: Welcome to the DeVille District

            You and Trix roll back into the DeVille District in a beat-up F-150,
            bed full of counterfeit belts. Marcus is waiting under a neon sign,
            smiling like he just discovered gravity.

            MARCUS: 'See? I told you. With my vision and your legs, we can move mountains.
            Or at least move inventory.'

            TRIX: 'Congrats. You're officially in his contacts under 'Assets'.'

            Newport just saw your first big move. The city took notice – and so did its enemies.
            ";

            Debug.Log(completionText);

            CleanupObjectiveMarkers();
        }

        private void CleanupObjectiveMarkers()
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("ObjectiveMarker");
            foreach (GameObject marker in markers)
            {
                if (marker != null && marker.name.Contains("Objective"))
                {
                    Destroy(marker);
                }
            }
        }

        private void UpdateMissionHUD()
        {
            Debug.Log($"Mission Progress: {completedObjectives}/{totalObjectives} objectives completed");
        }

        // Public helpers
        public bool IsMissionCompleted() => missionCompleted;
        public float GetMissionProgress() => (float)completedObjectives / totalObjectives;

        public string GetCurrentObjective()
        {
            if (!meetMarcus) return "Meet Marcus and Trix in the DeVille District";
            if (!reachWarehouse) return "Travel with Trix to the rival warehouse in Diaz District";
            if (!secureShipment) return "Secure the stolen belt shipment";
            if (!escapeWarehouse) return "Escape the warehouse area";
            if (!deliverShipment) return "Deliver the shipment back to DeVille's block";
            return "Mission Complete!";
        }
    }
}
