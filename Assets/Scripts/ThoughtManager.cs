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
    [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4]; // the 4 option labels

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
        if (currentThought == null || currentThought.options == null) return null;
        if (optionIndex < 0 || optionIndex >= currentThought.options.Length) return null;
        return currentThought.options[optionIndex].dialogue;
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

        // Update the 4 option labels (short titles)
        for (int i = 0; i < optionTexts.Length && i < currentThought.options.Length; i++)
        {
            if (optionTexts[i] != null)
                optionTexts[i].text = currentThought.options[i].title;
        }

        Debug.Log("[ThoughtManager] UI updated with new thought and strategies");
    }
}
