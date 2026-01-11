using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pet Chat Controller: manages empathetic AI chatbot interaction with high-risk phrase detection.
/// Integrates with existing AI client to provide mental-health-focused support.
/// </summary>
public class PetChatController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject zenMessagePrefab; // Zen's messages with avatar (cyan)
    [SerializeField] private GameObject playerMessagePrefab; // Player's messages without avatar (white)
    [SerializeField] private Transform chatContent;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private GameObject typingIndicator;
    [SerializeField] private ScrollRect chatScroll;

    [Header("AI Integration")]
    [SerializeField] private AIConversationService aiConversationService;

    [Header("High-Risk Phrases (for crisis detection)")]
    [SerializeField] private string[] highRiskPhrases = new string[]
    {
        "i want to disappear",
        "i am hopeless",
        "nothing matters anymore",
        "i want to die",
        "i should kill myself",
        "end it all",
        "can't go on",
        "no point in living",
        "better off dead",
        "harm myself"
    };

    [Header("Scene Navigation")]
    [SerializeField] private string returnSceneName = "Lobby Scene";
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;

    [Header("Debug")]
    [SerializeField] private bool debugLogAllMessages = true;

    private bool isWaitingForAI = false;

    // High-risk response (empathetic + resource suggestion)
    private const string HIGH_RISK_RESPONSE = "I can hear that you're in real pain right now, and I want you to know that your feelings matter. " +
        "You don't have to face this alone. Please reach out to someone you trust—a friend, family member, or counselor. " +
        "If you're in crisis, please contact a crisis helpline or emergency services. You deserve real, professional support. I'm here to listen, but a trained person can help even more.";

    // System prompt for empathetic AI responses
    private const string EMPATHY_SYSTEM_PROMPT =
        "You are a compassionate mental-health support companion in a game. Your role is to:\n" +
        "1. Provide scientifically-grounded, empathetic support.\n" +
        "2. Validate the user's feelings and acknowledge their mental health journey.\n" +
        "3. Be warm, brief (2-3 sentences max), and avoid clinical jargon.\n" +
        "4. Never request personal information or store sensitive data.\n" +
        "5. Encourage self-awareness and healthy coping strategies.\n" +
        "6. Respect confidentiality and user autonomy.\n\n" +
        "User said: \"{0}\"\n\n" +
        "Respond in 2-3 sentences with empathy and support.";

    void Start()
    {
        // Wire up UI
        sendButton.onClick.AddListener(OnSendButtonClicked);
        chatInput.onSubmit.AddListener((_) => OnSendButtonClicked());

        // Auto-focus input
        chatInput.ActivateInputField();

        // Ensure AI service is assigned
        if (aiConversationService == null)
        {
            aiConversationService = FindObjectOfType<AIConversationService>();
            if (aiConversationService == null)
                Debug.LogWarning("[PetChatController] AIConversationService not found in scene!");
        }

        // Ensure typing indicator is hidden
        if (typingIndicator != null)
            typingIndicator.SetActive(false);

        Debug.Log("[PetChatController] Initialized. Ready for chat.");
    }

    void Update()
    {
        // Allow escape to return to lobby
        if (Input.GetKeyDown(exitKey))
        {
            ReturnToLobby();
        }
    }

    void OnSendButtonClicked()
    {
        string userText = chatInput.text.Trim();

        if (string.IsNullOrEmpty(userText) || isWaitingForAI)
            return;

        // Append player message
        AppendMessage(userText, isPlayer: true);
        chatInput.text = string.Empty;
        chatInput.ActivateInputField();

        // Check for high-risk phrases
        if (IsHighRiskInput(userText))
        {
            Debug.LogWarning("[PetChatController] High-risk phrase detected!");
            AppendMessage(HIGH_RISK_RESPONSE, isPlayer: false);
            AutoScrollToBottom();
            return;
        }

        // Request AI response
        StartCoroutine(RequestAIResponseCoroutine(userText));
    }

    private IEnumerator RequestAIResponseCoroutine(string userText)
    {
        isWaitingForAI = true;
        if (typingIndicator != null)
            typingIndicator.SetActive(true);

        string petResponse = null;
        bool success = false;

        if (aiConversationService != null)
        {
            // Request chat response with empathy prompt
            aiConversationService.RequestChatResponse(
                userText,
                EMPATHY_SYSTEM_PROMPT,
                (response) =>
                {
                    petResponse = response;
                    success = true;
                    Debug.Log("[PetChatController] AI response received.");
                },
                (error) =>
                {
                    Debug.LogError($"[PetChatController] AI Error: {error}");
                    petResponse = "I'm having a moment of trouble expressing myself right now. Can you tell me more about what you're feeling?";
                    success = true;
                }
            );

            // Wait for AI response (with timeout)
            float timeout = 10f;
            float elapsed = 0f;
            while (!success && elapsed < timeout)
            {
                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            if (!success)
            {
                petResponse = "I'm here to listen. Take your time—what's on your mind?";
            }
        }
        else
        {
            petResponse = "Sorry, I can't reach my thoughts right now. But I'm listening. What would you like to share?";
        }

        // Append pet response
        if (typingIndicator != null)
            typingIndicator.SetActive(false);

        AppendMessage(petResponse, isPlayer: false);
        AutoScrollToBottom();

        isWaitingForAI = false;
    }

    private bool IsHighRiskInput(string userText)
    {
        string lowerText = userText.ToLower();
        foreach (string phrase in highRiskPhrases)
        {
            if (lowerText.Contains(phrase.ToLower()))
                return true;
        }
        return false;
    }

    private void AppendMessage(string text, bool isPlayer)
    {
        // Choose the correct prefab
        GameObject prefabToUse = isPlayer ? playerMessagePrefab : zenMessagePrefab;
        
        if (prefabToUse == null || chatContent == null)
        {
            Debug.LogError($"[PetChatController] {(isPlayer ? "Player" : "Zen")} MessagePrefab or ChatContent not assigned!");
            return;
        }

        // Instantiate message container
        GameObject msgContainer = Instantiate(prefabToUse, chatContent);
        
        // Get the text component (should be child of container)
        TextMeshProUGUI msgText = msgContainer.GetComponentInChildren<TextMeshProUGUI>();
        Image avatarImg = msgContainer.GetComponentInChildren<Image>();
        
        if (msgText != null)
        {
            // Set message text
            msgText.text = text;
            
            // Style based on sender
            if (isPlayer)
            {
                // Player message: right-aligned, white
                msgText.color = Color.white;
                msgText.alignment = TextAlignmentOptions.Right;
                
                // Hide avatar for player messages
                if (avatarImg != null)
                    avatarImg.gameObject.SetActive(false);
            }
            else
            {
                // Zen message: left-aligned, cyan
                msgText.color = Color.cyan;
                msgText.alignment = TextAlignmentOptions.Left;
                
                // Show avatar for Zen messages
                if (avatarImg != null)
                    avatarImg.gameObject.SetActive(true);
            }
            
            // Critical: Ensure TextMeshPro properly calculates height
            msgText.enableWordWrapping = true;
            msgText.overflowMode = TextOverflowModes.Overflow;
            
            // Force text to recalculate immediately
            msgText.ForceMeshUpdate();
            
            // Get or add ContentSizeFitter to text's immediate parent (MessageBubble)
            Transform textParentTransform = msgText.transform.parent;
            if (textParentTransform != null)
            {
                ContentSizeFitter bubbleFitter = textParentTransform.GetComponent<ContentSizeFitter>();
                if (bubbleFitter == null)
                {
                    bubbleFitter = textParentTransform.gameObject.AddComponent<ContentSizeFitter>();
                }
                bubbleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                bubbleFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }
        
        // CRITICAL: Remove or disable Layout Elements that have fixed heights
        // They override ContentSizeFitter and cause equal spacing
        LayoutElement[] allLayoutElements = msgContainer.GetComponentsInChildren<LayoutElement>(true);
        foreach (LayoutElement le in allLayoutElements)
        {
            // Destroy Layout Elements completely - they conflict with ContentSizeFitter
            Destroy(le);
        }
        
        // Disable child height control on Horizontal Layout Group
        HorizontalLayoutGroup horizLayout = msgContainer.GetComponent<HorizontalLayoutGroup>();
        if (horizLayout != null)
        {
            horizLayout.childControlHeight = false;
            horizLayout.childForceExpandHeight = false;
            horizLayout.childScaleHeight = false;
        }
        
        // Add ContentSizeFitter to container to grow based on children
        ContentSizeFitter containerFitter = msgContainer.GetComponent<ContentSizeFitter>();
        if (containerFitter == null)
        {
            containerFitter = msgContainer.AddComponent<ContentSizeFitter>();
        }
        containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        containerFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        if (debugLogAllMessages)
            Debug.Log($"[PetChatController] {(isPlayer ? "Player" : "Zen")}: {text}");
    }

    private void AutoScrollToBottom()
    {
        if (chatScroll == null) return;

        // Force immediate layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent as RectTransform);
        
        // Wait one frame then scroll to bottom
        StartCoroutine(ScrollToBottomNextFrame());
    }
    
    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // Wait one frame
        if (chatScroll != null)
        {
            chatScroll.verticalNormalizedPosition = 0f;
        }
    }

    private void ReturnToLobby()
    {
        Debug.Log("[PetChatController] Returning to lobby...");
        SceneManager.LoadScene(returnSceneName);
    }

    // Inspector context menu for testing
    [ContextMenu("Test High-Risk Detection")]
    public void TestHighRiskDetection()
    {
        string testPhrase = "I am hopeless";
        bool isHighRisk = IsHighRiskInput(testPhrase);
        Debug.Log($"[PetChatController] Test phrase '{testPhrase}' is high-risk: {isHighRisk}");
    }
}
