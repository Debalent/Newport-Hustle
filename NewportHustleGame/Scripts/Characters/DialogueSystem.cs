using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace NewportHustle.Characters
{
    public class DialogueSystem : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject dialoguePanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueText;
        public Button[] responseButtons;
        public Button continueButton;
        public Image speakerPortrait;
        
        [Header("Dialogue Settings")]
        public float typewriterSpeed = 0.05f;
        public bool skipTypewriter = false;
        
        // Current dialogue state
        private DialogueData currentDialogue;
        private DialogueNode currentNode;
        private NPCBehavior currentNPC;
        private bool isDialogueActive;
        private bool isTyping;
        private Coroutine typingCoroutine;
        
        // Dialogue history and flags
        private Dictionary<string, bool> dialogueFlags = new Dictionary<string, bool>();
        private List<string> dialogueHistory = new List<string>();
        
        void Start()
        {
            InitializeDialogueSystem();
        }
        
        void Update()
        {
            if (isDialogueActive)
            {
                HandleDialogueInput();
            }
        }
        
        private void InitializeDialogueSystem()
        {
            // Hide dialogue panel initially
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            // Setup button listeners
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueDialogue);
            }
            
            for (int i = 0; i < responseButtons.Length; i++)
            {
                int index = i; // Capture for closure
                responseButtons[i].onClick.AddListener(() => SelectResponse(index));
            }
        }
        
        private void HandleDialogueInput()
        {
            // Skip typing on tap/click
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                if (isTyping)
                {
                    SkipTypewriter();
                }
            }
            
            // Close dialogue with escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
        
        public void StartDialogue(string dialogueID, NPCBehavior npc)
        {
            // Load dialogue data
            currentDialogue = LoadDialogueData(dialogueID);
            if (currentDialogue == null)
            {
                Debug.LogError($"Dialogue data not found for ID: {dialogueID}");
                return;
            }
            
            currentNPC = npc;
            isDialogueActive = true;
            
            // Show dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
            
            // Pause game
            Time.timeScale = 0f;
            
            // Start with first node
            currentNode = GetStartingNode();
            DisplayCurrentNode();
            
            // Set speaker info
            UpdateSpeakerInfo();
        }
        
        public void EndDialogue()
        {
            isDialogueActive = false;
            
            // Hide dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            // Resume game
            Time.timeScale = 1f;
            
            // Notify NPC that dialogue ended
            if (currentNPC != null)
            {
                currentNPC.EndDialogue();
            }
            
            // Clear current dialogue
            currentDialogue = null;
            currentNode = null;
            currentNPC = null;
        }
        
        private void DisplayCurrentNode()
        {
            if (currentNode == null) return;
            
            // Check conditions
            if (!CheckNodeConditions(currentNode))
            {
                // Skip to next node or end dialogue
                AdvanceToNextNode();
                return;
            }
            
            // Display speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = GetSpeakerName();
            }
            
            // Start typing dialogue text
            if (dialogueText != null)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                typingCoroutine = StartCoroutine(TypeText(currentNode.text));
            }
            
            // Setup response buttons
            SetupResponseButtons();
            
            // Execute node effects
            ExecuteNodeEffects(currentNode);
        }
        
        private void SetupResponseButtons()
        {
            // Hide all buttons first
            foreach (Button button in responseButtons)
            {
                button.gameObject.SetActive(false);
            }
            
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }
            
            if (currentNode.responses != null && currentNode.responses.Count > 0)
            {
                // Show response buttons
                for (int i = 0; i < currentNode.responses.Count && i < responseButtons.Length; i++)
                {
                    DialogueResponse response = currentNode.responses[i];
                    
                    // Check if response is available
                    if (CheckResponseConditions(response))
                    {
                        responseButtons[i].gameObject.SetActive(true);
                        responseButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = response.text;
                    }
                }
            }
            else
            {
                // Show continue button if no responses
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                }
            }
        }
        
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";
            
            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }
            
            isTyping = false;
        }
        
        private void SkipTypewriter()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialogueText.text = currentNode.text;
            isTyping = false;
        }
        
        public void ContinueDialogue()
        {
            if (isTyping)
            {
                SkipTypewriter();
                return;
            }
            
            AdvanceToNextNode();
        }
        
        public void SelectResponse(int responseIndex)
        {
            if (currentNode.responses != null && responseIndex < currentNode.responses.Count)
            {
                DialogueResponse selectedResponse = currentNode.responses[responseIndex];
                
                // Execute response effects
                ExecuteResponseEffects(selectedResponse);
                
                // Move to next node
                if (!string.IsNullOrEmpty(selectedResponse.nextNodeID))
                {
                    currentNode = GetNodeByID(selectedResponse.nextNodeID);
                    DisplayCurrentNode();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
        
        private void AdvanceToNextNode()
        {
            if (!string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                currentNode = GetNodeByID(currentNode.nextNodeID);
                DisplayCurrentNode();
            }
            else
            {
                EndDialogue();
            }
        }
        
        private bool CheckNodeConditions(DialogueNode node)
        {
            if (node.conditions == null || node.conditions.Count == 0)
                return true;
            
            foreach (DialogueCondition condition in node.conditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            return true;
        }
        
        private bool CheckResponseConditions(DialogueResponse response)
        {
            if (response.conditions == null || response.conditions.Count == 0)
                return true;
            
            foreach (DialogueCondition condition in response.conditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            return true;
        }
        
        private bool EvaluateCondition(DialogueCondition condition)
        {
            switch (condition.type)
            {
                case ConditionType.Flag:
                    return dialogueFlags.ContainsKey(condition.key) && dialogueFlags[condition.key] == condition.boolValue;
                
                case ConditionType.PlayerLevel:
                    return GameManager.Instance.playerLevel >= condition.intValue;
                
                case ConditionType.PlayerMoney:
                    return GameManager.Instance.playerMoney >= condition.floatValue;
                
                case ConditionType.PlayerRespect:
                    return GameManager.Instance.playerRespect >= condition.floatValue;
                
                case ConditionType.Relationship:
                    return currentNPC != null && currentNPC.relationshipWithPlayer >= condition.floatValue;
                
                case ConditionType.TimeOfDay:
                    TimeCycle timeCycle = FindObjectOfType<TimeCycle>();
                    return timeCycle != null && timeCycle.IsTimeOfDay((TimeOfDayPeriod)condition.intValue);
                
                default:
                    return true;
            }
        }
        
        private void ExecuteNodeEffects(DialogueNode node)
        {
            if (node.effects == null) return;
            
            foreach (DialogueEffect effect in node.effects)
            {
                ExecuteEffect(effect);
            }
        }
        
        private void ExecuteResponseEffects(DialogueResponse response)
        {
            if (response.effects == null) return;
            
            foreach (DialogueEffect effect in response.effects)
            {
                ExecuteEffect(effect);
            }
        }
        
        private void ExecuteEffect(DialogueEffect effect)
        {
            switch (effect.type)
            {
                case EffectType.SetFlag:
                    dialogueFlags[effect.key] = effect.boolValue;
                    break;
                
                case EffectType.GiveMoney:
                    GameManager.Instance.UpdatePlayerMoney(effect.floatValue);
                    break;
                
                case EffectType.TakeMoney:
                    GameManager.Instance.UpdatePlayerMoney(-effect.floatValue);
                    break;
                
                case EffectType.ChangeRelationship:
                    if (currentNPC != null)
                    {
                        currentNPC.ChangeRelationship(effect.floatValue);
                    }
                    break;
                
                case EffectType.GiveItem:
                    // TODO: Implement item system
                    Debug.Log($"Gave item: {effect.stringValue}");
                    break;
                
                case EffectType.StartMission:
                    // TODO: Implement mission system
                    Debug.Log($"Started mission: {effect.stringValue}");
                    break;
                
                case EffectType.CompleteObjective:
                    // TODO: Implement mission system
                    Debug.Log($"Completed objective: {effect.stringValue}");
                    break;
            }
        }
        
        private string GetSpeakerName()
        {
            if (currentNPC != null)
            {
                return currentNPC.npcName;
            }
            return currentNode.speakerName;
        }
        
        private void UpdateSpeakerInfo()
        {
            // Update speaker portrait if available
            if (speakerPortrait != null && currentNPC != null)
            {
                // Load portrait sprite based on NPC type
                // This would be implemented with actual sprite loading
            }
        }
        
        private DialogueData LoadDialogueData(string dialogueID)
        {
            // This would load from JSON files in the Dialogue folder
            // For now, return a placeholder
            return CreateSampleDialogue(dialogueID);
        }
        
        private DialogueNode GetStartingNode()
        {
            if (currentDialogue != null && currentDialogue.nodes.Count > 0)
            {
                return currentDialogue.nodes[0];
            }
            return null;
        }
        
        private DialogueNode GetNodeByID(string nodeID)
        {
            if (currentDialogue != null)
            {
                foreach (DialogueNode node in currentDialogue.nodes)
                {
                    if (node.nodeID == nodeID)
                    {
                        return node;
                    }
                }
            }
            return null;
        }
        
        // Create sample dialogue for testing
        private DialogueData CreateSampleDialogue(string dialogueID)
        {
            DialogueData dialogue = new DialogueData();
            dialogue.dialogueID = dialogueID;
            
            // Marcus DeVille intro dialogue for the "Welcome to the DeVille District" mission
            if (dialogueID == "Marcus_WelcomeToDistrict")
            {
                DialogueNode marcusIntro1 = new DialogueNode
                {
                    nodeID = "start",
                    speakerName = "Marcus DeVille",
                    text = "Look at you. Fresh off the bus and already smelling like potential. Welcome to the DeVille District — where dreams come true and wallets go missing.",
                    nextNodeID = "marcus_pitch"
                };

                DialogueNode marcusIntro2 = new DialogueNode
                {
                    nodeID = "marcus_pitch",
                    speakerName = "Marcus DeVille",
                    text = "My rivals stole my belts. My belts. A man without a belt is like a car without rims — technically functional, but embarrassing. You and Trix are gonna pay a visit to my friends in Diaz District and remind them whose logo runs this town.",
                    responses = new List<DialogueResponse>
                    {
                        new DialogueResponse
                        {
                            text = "Sounds risky... but I'm in.",
                            nextNodeID = "player_yes"
                        },
                        new DialogueResponse
                        {
                            text = "Why should I do this for you?",
                            nextNodeID = "player_question"
                        }
                    }
                };

                DialogueNode playerYes = new DialogueNode
                {
                    nodeID = "player_yes",
                    speakerName = "Trix",
                    text = "That's the spirit. Worst case, we end up on a group chat we don't like.",
                    nextNodeID = "end"
                };

                DialogueNode playerQuestion = new DialogueNode
                {
                    nodeID = "player_question",
                    speakerName = "Marcus DeVille",
                    text = "Because Newport already wrote you off. I'm giving you a chance to write back. You help me get what's mine, I make sure this city remembers your name.",
                    nextNodeID = "end"
                };

                DialogueNode endNode = new DialogueNode
                {
                    nodeID = "end",
                    speakerName = "Marcus DeVille",
                    text = "Meet Trix by the car. Diaz District's waiting. Try not to scuff my inventory.",
                    nextNodeID = null
                };

                dialogue.nodes.Add(marcusIntro1);
                dialogue.nodes.Add(marcusIntro2);
                dialogue.nodes.Add(playerYes);
                dialogue.nodes.Add(playerQuestion);
                dialogue.nodes.Add(endNode);

                return dialogue;
            }

            if (dialogueID == "Lexi_WelcomeToAlgorithm")
            {
                DialogueNode lexiIntro = new DialogueNode
                {
                    nodeID = "start",
                    speakerName = "Lexi Vance",
                    text = "Five point two million followers. Not one of them sent me a decent fixer. Until now, apparently. Don't smile — I'll post it and it'll trend for the wrong reasons.",
                    nextNodeID = "lexi_offer"
                };

                DialogueNode lexiOffer = new DialogueNode
                {
                    nodeID = "lexi_offer",
                    speakerName = "Lexi Vance",
                    text = "Someone's running bots against my engagement. They're deleting comments, flagging my sponsors, and — this is the part that really hurts — clipping me out of context. I need it stopped and I need footage I can use. You in?",
                    responses = new List<DialogueResponse>
                    {
                        new DialogueResponse { text = "I'll handle it. What's the pay?",       nextNodeID = "lexi_yes" },
                        new DialogueResponse { text = "Sounds like your problem, not mine.",    nextNodeID = "lexi_refuse" }
                    }
                };

                DialogueNode lexiYes = new DialogueNode
                {
                    nodeID = "lexi_yes",
                    speakerName = "Lexi Vance",
                    text = "The Algorithm provides. I'll send the access codes to your phone. And if you do this right — and I mean CINEMATICALLY right — we talk about a longer arrangement.",
                    nextNodeID = "lexi_end"
                };

                DialogueNode lexiRefuse = new DialogueNode
                {
                    nodeID = "lexi_refuse",
                    speakerName = "Lexi Vance",
                    text = "My camera drones are already filming you. Reconsider. There's a thumbnail opportunity here for both of us.",
                    nextNodeID = "lexi_end"
                };

                DialogueNode lexiEnd = new DialogueNode
                {
                    nodeID = "lexi_end",
                    speakerName = "Kai",
                    text = "She's already drafting the caption. You might as well go.",
                    nextNodeID = null
                };

                dialogue.nodes.Add(lexiIntro);
                dialogue.nodes.Add(lexiOffer);
                dialogue.nodes.Add(lexiYes);
                dialogue.nodes.Add(lexiRefuse);
                dialogue.nodes.Add(lexiEnd);

                return dialogue;
            }

            if (dialogueID == "Rico_WelcomeToFleet")
            {
                DialogueNode ricoIntro = new DialogueNode
                {
                    nodeID = "start",
                    speakerName = "Big Rico",
                    text = "You see that rig right there? That's Deborah. You look at Deborah wrong, we got a problem. You touch Deborah, we got a bigger problem. Now — who sent you?",
                    nextNodeID = "rico_territory"
                };

                DialogueNode ricoTerritory = new DialogueNode
                {
                    nodeID = "rico_territory",
                    speakerName = "Big Rico",
                    text = "Three of my best trucks are down. Cables cut, hydraulics drained. Somebody's hitting me intentional. You find out who — and you make it stop — I'll make sure your vehicle never gets towed in Newport. That's a lifestyle benefit, kid.",
                    responses = new List<DialogueResponse>
                    {
                        new DialogueResponse { text = "Deal. I'll find your guy.",          nextNodeID = "rico_yes" },
                        new DialogueResponse { text = "How do I know this isn't a setup?",  nextNodeID = "rico_question" }
                    }
                };

                DialogueNode ricoYes = new DialogueNode
                {
                    nodeID = "rico_yes",
                    speakerName = "Big Rico",
                    text = "Smart kid. Elena'll walk you through the lot. Don't pet the dogs without asking.",
                    nextNodeID = "rico_end"
                };

                DialogueNode ricoQuestion = new DialogueNode
                {
                    nodeID = "rico_question",
                    speakerName = "Big Rico",
                    text = "If it was a setup, Deborah would already be on your hood. Eat the quesadilla and get to work.",
                    nextNodeID = "rico_end"
                };

                DialogueNode ricoEnd = new DialogueNode
                {
                    nodeID = "rico_end",
                    speakerName = "Elena",
                    text = "Don't mind him. He named the truck after his grandma. Come on, I'll show you where it happened.",
                    nextNodeID = null
                };

                dialogue.nodes.Add(ricoIntro);
                dialogue.nodes.Add(ricoTerritory);
                dialogue.nodes.Add(ricoYes);
                dialogue.nodes.Add(ricoQuestion);
                dialogue.nodes.Add(ricoEnd);

                return dialogue;
            }

            if (dialogueID == "Fable_WelcomeToEnlightenment")
            {
                DialogueNode fableIntro = new DialogueNode
                {
                    nodeID = "start",
                    speakerName = "Dr. Fable",
                    text = "The universe sent you here. Not the door. Not me. The universe. I want you to sit with that for a moment. ... Good. Now: there's a man named Chad who is about to destroy everything I've built.",
                    nextNodeID = "fable_pitch"
                };

                DialogueNode fablePitch = new DialogueNode
                {
                    nodeID = "fable_pitch",
                    speakerName = "Dr. Fable",
                    text = "Chad was a member here. A beloved member. Now he's talking to journalists about my supplements. Totally out of context. Will you help me ensure Chad's narrative reaches a natural conclusion?",
                    responses = new List<DialogueResponse>
                    {
                        new DialogueResponse { text = "I'll talk to him. No promises.",                     nextNodeID = "fable_yes" },
                        new DialogueResponse { text = "I'm skeptical about all of this.",                  nextNodeID = "fable_skeptic" }
                    }
                };

                DialogueNode fableYes = new DialogueNode
                {
                    nodeID = "fable_yes",
                    speakerName = "Dr. Fable",
                    text = "Perfect. Here — take an Inner Fire sample. It's just water, but it's water with intention. The energy you bring to Chad will determine the outcome.",
                    nextNodeID = "fable_end"
                };

                DialogueNode fableSkeptic = new DialogueNode
                {
                    nodeID = "fable_skeptic",
                    speakerName = "Dr. Fable",
                    text = "Skepticism is just unexplored faith. I respect it. I also pay in cash. Five hundred. The supplements are unrelated.",
                    nextNodeID = "fable_end"
                };

                DialogueNode fableEnd = new DialogueNode
                {
                    nodeID = "fable_end",
                    speakerName = "Dr. Fable",
                    text = "Chad is at Miss Pearl's Diner. He eats the same booth every Tuesday. The universe is very consistent with him.",
                    nextNodeID = null
                };

                dialogue.nodes.Add(fableIntro);
                dialogue.nodes.Add(fablePitch);
                dialogue.nodes.Add(fableYes);
                dialogue.nodes.Add(fableSkeptic);
                dialogue.nodes.Add(fableEnd);

                return dialogue;
            }

            if (dialogueID == "Mayor_PoliticalBriefing")
            {
                DialogueNode mayorIntro = new DialogueNode
                {
                    nodeID = "start",
                    speakerName = "Mayor Bucksworth",
                    text = "Close the door. I know who you are. I know who you've been working for. And before you reach for whatever's in your pocket — I don't need threats. I need a fixer.",
                    nextNodeID = "mayor_offer"
                };

                DialogueNode mayorOffer = new DialogueNode
                {
                    nodeID = "mayor_offer",
                    speakerName = "Mayor Bucksworth",
                    text = "Patricia Dawn has a file. It connects me to some... ambitious infrastructure arrangements. I need that file gone before Election Day. You get it done quietly, Newport stays the same. You get it done loudly, Newport stays the same — but you won't be here to see it.",
                    responses = new List<DialogueResponse>
                    {
                        new DialogueResponse { text = "I'll get it. But this clears my record.",   nextNodeID = "mayor_yes" },
                        new DialogueResponse { text = "I don't work for people like you.",          nextNodeID = "mayor_refuse" }
                    }
                };

                DialogueNode mayorYes = new DialogueNode
                {
                    nodeID = "mayor_yes",
                    speakerName = "Mayor Bucksworth",
                    text = "Your record? Son, I AM your record. Rodney will give you the address. Don't read the file.",
                    nextNodeID = "mayor_end"
                };

                DialogueNode mayorRefuse = new DialogueNode
                {
                    nodeID = "mayor_refuse",
                    speakerName = "Mayor Bucksworth",
                    text = "You'd be surprised what people like me look like up close. Think about it overnight. Newport has long arms and they all bend my direction.",
                    nextNodeID = "mayor_end"
                };

                DialogueNode mayorEnd = new DialogueNode
                {
                    nodeID = "mayor_end",
                    speakerName = "Rodney",
                    text = "... I'm sorry. I just — I work here. Please don't get me involved.",
                    nextNodeID = null
                };

                dialogue.nodes.Add(mayorIntro);
                dialogue.nodes.Add(mayorOffer);
                dialogue.nodes.Add(mayorYes);
                dialogue.nodes.Add(mayorRefuse);
                dialogue.nodes.Add(mayorEnd);

                return dialogue;
            }

            // Sample Newport-themed dialogue
            DialogueNode node1 = new DialogueNode
            {
                nodeID = "start",
                text = "Welcome to Newport! You look new around here. This town's got its own rhythm, especially down by the White River.",
                speakerName = "Local",
                nextNodeID = "choice1"
            };
            
            DialogueNode node2 = new DialogueNode
            {
                nodeID = "choice1",
                text = "What brings you to our little slice of Arkansas?",
                responses = new List<DialogueResponse>
                {
                    new DialogueResponse { text = "Just looking around.", nextNodeID = "end" },
                    new DialogueResponse { text = "I'm here to make a name for myself.", nextNodeID = "ambitious" },
                    new DialogueResponse { text = "Mind your own business.", nextNodeID = "rude" }
                }
            };
            
            dialogue.nodes.Add(node1);
            dialogue.nodes.Add(node2);
            
            return dialogue;
        }
        
        // Public methods for setting flags from other systems
        public void SetDialogueFlag(string key, bool value)
        {
            dialogueFlags[key] = value;
        }
        
        public bool GetDialogueFlag(string key)
        {
            return dialogueFlags.ContainsKey(key) ? dialogueFlags[key] : false;
        }
    }
    
    // Dialogue data structures
    [System.Serializable]
    public class DialogueData
    {
        public string dialogueID;
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }
    
    [System.Serializable]
    public class DialogueNode
    {
        public string nodeID;
        public string speakerName;
        public string text;
        public string nextNodeID;
        public List<DialogueResponse> responses;
        public List<DialogueCondition> conditions;
        public List<DialogueEffect> effects;
    }
    
    [System.Serializable]
    public class DialogueResponse
    {
        public string text;
        public string nextNodeID;
        public List<DialogueCondition> conditions;
        public List<DialogueEffect> effects;
    }
    
    [System.Serializable]
    public class DialogueCondition
    {
        public ConditionType type;
        public string key;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
    }
    
    [System.Serializable]
    public class DialogueEffect
    {
        public EffectType type;
        public string key;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
    }
    
    public enum ConditionType
    {
        Flag,
        PlayerLevel,
        PlayerMoney,
        PlayerRespect,
        Relationship,
        TimeOfDay,
        Weather,
        MissionComplete
    }
    
    public enum EffectType
    {
        SetFlag,
        GiveMoney,
        TakeMoney,
        ChangeRelationship,
        GiveItem,
        TakeItem,
        StartMission,
        CompleteObjective,
        ChangeScene
    }
}