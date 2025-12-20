using UnityEngine;
using TMPro;

/// <summary>
/// Manages the current anxiety thought for a level.
/// Stores AI-generated thought and strategies, and updates dialogue/options UI accordingly.
/// </summary>
public class ThoughtManager : MonoBehaviour
{
    public static ThoughtManager instance { get; private set; }

    private AnxietyThoughtData currentThought;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI thoughtDisplayText; // shows the worry on screen
    [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4]; // the 4 coping strategy buttons

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Set the current thought and update all UI accordingly
    /// </summary>
    public void SetCurrentThought(AnxietyThoughtData thought)
    {
        currentThought = thought;
        UpdateUI();
    }

    /// <summary>
    /// Bind UI references for the currently active level by locating child objects
    /// named 'Thought Bubble' and 'Option 1'..'Option 4' under the provided level root.
    /// </summary>
    public void BindToLevel(GameObject levelRoot)
    {
        if (levelRoot == null)
        {
            Debug.LogWarning("[ThoughtManager] BindToLevel called with null level root");
            return;
        }

        // Find thought bubble text
        var thoughtText = FindTMPByName(levelRoot, "Thought Bubble");

        // Find option texts 1..4
        var opts = new TextMeshProUGUI[4];
        for (int i = 1; i <= 4; i++)
        {
            opts[i - 1] = FindTMPByName(levelRoot, $"Option {i}");
        }

        // Apply bindings
        thoughtDisplayText = thoughtText;
        optionTexts = opts;

        Debug.Log($"[ThoughtManager] Bound UI for level '{levelRoot.name}'. Thought={(thoughtDisplayText!=null)}, Options={(optionTexts!=null)}");

        // Refresh UI with existing thought if available
        UpdateUI();
    }

    private TextMeshProUGUI FindTMPByName(GameObject root, string childName)
    {
        if (root == null) return null;

        // Try direct child lookup
        var t = root.transform.Find(childName);
        TextMeshProUGUI tmp = null;
        if (t != null)
            tmp = t.GetComponentInChildren<TextMeshProUGUI>(true);

        // Fallback: search all TMP components by GameObject name
        if (tmp == null)
        {
            var tmps = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var x in tmps)
            {
                if (x.gameObject.name == childName)
                {
                    tmp = x;
                    break;
                }
            }
        }

        if (tmp == null)
            Debug.LogWarning($"[ThoughtManager] Could not find TMP '{childName}' under '{root.name}'");

        return tmp;
    }

    /// <summary>
    /// Get the current thought
    /// </summary>
    public AnxietyThoughtData GetCurrentThought()
    {
        return currentThought;
    }

    /// <summary>
    /// Get the coping strategy for a specific option (0-3)
    /// </summary>
    public string GetCopingStrategy(int optionIndex)
    {
        if (currentThought == null || optionIndex < 0 || optionIndex >= currentThought.copingStrategies.Length)
            return null;
        return currentThought.copingStrategies[optionIndex];
    }

    private void UpdateUI()
    {
        if (currentThought == null)
        {
            Debug.LogWarning("[ThoughtManager] No current thought to display");
            return;
        }

        // Update the worried thought display
        if (thoughtDisplayText != null)
            thoughtDisplayText.text = currentThought.worriedThought;

        // Update the 4 coping strategy buttons
        for (int i = 0; i < optionTexts.Length && i < currentThought.copingStrategies.Length; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].text = currentThought.copingStrategies[i];
        }

        Debug.Log("[ThoughtManager] UI updated with new thought and strategies");
    }
}
