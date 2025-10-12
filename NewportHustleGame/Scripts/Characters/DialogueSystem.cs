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