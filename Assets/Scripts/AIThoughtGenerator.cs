using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Calls OpenAI API to generate worrying thoughts and coping strategies
/// Handles JSON serialization for API requests/responses
/// </summary>
public class AIThoughtGenerator : MonoBehaviour
{
    [SerializeField] private string apiKey = ""; // Loaded from secrets.json or PlayerPrefs; keep empty in public repo
    [SerializeField] private string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private float temperature = 0.7f;

    // Location(s) to look for secrets.json. Preferred: StreamingAssets.
    private const string SecretsFileName = "secrets.json";
    private const string SecretsJsonKey = "ApiKey";
    private const string PlayerPrefsApiKey = "OpenAI_ApiKey";

    private const string THOUGHT_PROMPT = @"Generate a realistic worrying thought that someone with anxiety might experience. 
The thought should be:
- Specific and relatable (e.g., 'I'm going to fail this exam')
- Not too extreme or harmful
- Common among people with anxiety

Respond with ONLY the thought, nothing else. Keep it under 12 words.";

    private const string COPING_PROMPT = @"Given this worrying thought: ""{0}""

Create 4 distinct player choices in this exact order:
1) Avoidance/escape that sounds doable but ends in a blocked path.
2) Self-criticism that invites trouble and offers no route upward.
3) Over-control that is technically possible but long, tiring, and roundabout.
4) Healthy, balanced cognitive reframe with the simplest, kindest way forward.

Write one short sentence for each (<=10 words). Format exactly as 4 bullet lines, no labels:
- ...
- ...
- ...
- ...

Example for the thought ""Everyone else is handling things better than me."":
- Just don't think about it.
- I'm falling behind. I'll never catch up.
- I must work harder to catch up.
- Everyone struggles sometimes. I'm doing my best, and that's enough.";

    private const string DIALOGUE_PROMPT = @"Create a short, supportive dialogue (2-3 lines) that introduces this worrying thought to a player in an anxiety awareness game:

Worrying Thought: ""{0}""

The dialogue should:
- Be empathetic and validating
- Acknowledge the thought without judgment
- Introduce the coping strategies that follow

Keep it under 50 words total. Format as a single paragraph.";

    private const string EXPLANATION_PROMPT = @"Given this worrying thought and 4 coping options:

Thought: ""{0}""

Options:
1. {1}
2. {2}
3. {3}
4. {4}

Write a brief explanation for EACH option that:
- Describes why this approach is helpful or unhelpful
- Explains the outcome (e.g., 'blocks progress', 'creates more anxiety', 'takes longer path', 'healthy and direct')
- For options 1-3 (the unhelpful ones), END with a gentle suggestion
- For option 4, affirm it's a healthy choice
- Keep each explanation to 2-3 sentences (under 30 words each)
- Keep your tone encouraging and upbeat!

Format exactly as 4 bullet lines, no labels:
- ...
- ...
- ...
- ...";

    /// <summary>
    /// Generates a worrying thought, coping strategies, and dialogue
    /// </summary>
    public void GenerateThought(Action<AnxietyThoughtData> onComplete, Action<string> onError)
    {
        // Ensure API key is loaded before making any calls
        EnsureApiKeyLoaded();
        StartCoroutine(GenerateThoughtCoroutine(onComplete, onError));
    }

    private IEnumerator GenerateThoughtCoroutine(Action<AnxietyThoughtData> onComplete, Action<string> onError)
    {
        // Step 1: Generate the worrying thought
        string thought = null;
        yield return StartCoroutine(CallOpenAI(THOUGHT_PROMPT, (result) => thought = result, onError));

        if (thought == null)
        {
            onError?.Invoke("Failed to generate worrying thought");
            yield break;
        }

        Debug.Log($"[AI] Generated thought: {thought}");

        // Step 2: Generate dialogue
        string dialogue = null;
        string dialoguePrompt = string.Format(DIALOGUE_PROMPT, thought);
        yield return StartCoroutine(CallOpenAI(dialoguePrompt, (result) => dialogue = result, onError));

        if (dialogue == null)
        {
            onError?.Invoke("Failed to generate dialogue");
            yield break;
        }

        Debug.Log($"[AI] Generated dialogue: {dialogue}");

        // Step 3: Generate 4 coping strategies
        string strategiesRaw = null;
        string strategiesPrompt = string.Format(COPING_PROMPT, thought);
        yield return StartCoroutine(CallOpenAI(strategiesPrompt, (result) => strategiesRaw = result, onError));

        if (strategiesRaw == null)
        {
            onError?.Invoke("Failed to generate coping strategies");
            yield break;
        }

        // Parse strategies (split by '- ')
        string[] strategies = ParseStrategies(strategiesRaw);

        // Step 4: Generate explanations for each option
        string explanationsRaw = null;
        string explanationsPrompt = string.Format(EXPLANATION_PROMPT, thought, 
            strategies.Length > 0 ? strategies[0] : "",
            strategies.Length > 1 ? strategies[1] : "",
            strategies.Length > 2 ? strategies[2] : "",
            strategies.Length > 3 ? strategies[3] : "");
        yield return StartCoroutine(CallOpenAI(explanationsPrompt, (result) => explanationsRaw = result, onError));

        if (explanationsRaw == null)
        {
            onError?.Invoke("Failed to generate option explanations");
            yield break;
        }

        // Parse explanations
        string[] explanations = ParseStrategies(explanationsRaw);

        // Create final data object
        AnxietyThoughtData data = new AnxietyThoughtData
        {
            worriedThought = thought,
            introDialogue = dialogue,
            copingStrategies = strategies,
            optionExplanations = explanations
        };

        Debug.Log($"[AI] Generated complete thought data with {strategies.Length} strategies and {explanations.Length} explanations");
        onComplete?.Invoke(data);
    }

    private IEnumerator CallOpenAI(string prompt, Action<string> onResult, Action<string> onError)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onError?.Invoke("OpenAI API key not set. Set it in the Inspector or via PlayerPrefs.");
            yield break;
        }

        var requestData = new OpenAIRequest
        {
            model = model,
            messages = new[] { new Message { role = "user", content = prompt } },
            temperature = temperature,
            max_tokens = 200
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"[AI] Sending request to OpenAI: model={model}, temperature={temperature}, promptLen={prompt.Length}");
        // Note: Unity JsonUtility doesn't support dictionaries well; keep classes in sync with response

        using (UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.timeout = 20; // seconds

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AI] OpenAI API Error: {request.error}");
                Debug.LogError($"[AI] Response: {request.downloadHandler.text}");
                // Try to parse error JSON for more detail
                try
                {
                    var err = JsonUtility.FromJson<OpenAIErrorWrapper>(request.downloadHandler.text);
                    if (err != null && err.error != null && !string.IsNullOrEmpty(err.error.message))
                        onError?.Invoke($"OpenAI Error: {err.error.message}");
                    else
                        onError?.Invoke($"OpenAI API Error: {request.error}");
                }
                catch
                {
                    onError?.Invoke($"OpenAI API Error: {request.error}");
                }
                yield break;
            }

            try
            {
                var response = JsonUtility.FromJson<OpenAIResponse>(request.downloadHandler.text);
                if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0] == null || response.choices[0].message == null)
                {
                    Debug.LogError("[AI] Unexpected response shape; full body:" + request.downloadHandler.text);
                    onError?.Invoke("Unexpected response from OpenAI. Check model/endpoint.");
                    yield break;
                }
                string result = response.choices[0].message.content.Trim();
                onResult?.Invoke(result);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AI] Failed to parse OpenAI response: {ex.Message}");
                onError?.Invoke($"Failed to parse response: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Loads the API key from PlayerPrefs or secrets.json if not already set.
    /// Search order: Inspector (serialized) -> PlayerPrefs -> StreamingAssets -> Assets root.
    /// </summary>
    private void EnsureApiKeyLoaded()
    {
        if (!string.IsNullOrEmpty(apiKey)) return;

        // Try PlayerPrefs first (allows setting securely at runtime without files)
        var ppKey = PlayerPrefs.GetString(PlayerPrefsApiKey, string.Empty);
        if (!string.IsNullOrEmpty(ppKey))
        {
            apiKey = ppKey;
            return;
        }

        // Try StreamingAssets/secrets.json
        string streamingPath = Path.Combine(Application.streamingAssetsPath, SecretsFileName);
        if (File.Exists(streamingPath))
        {
            var json = File.ReadAllText(streamingPath);
            apiKey = ExtractApiKeyFromJson(json);
            if (!string.IsNullOrEmpty(apiKey)) return;
        }

        // Fallback: Assets/secrets.json (useful in Editor/testing; exclude via .gitignore)
        string assetsPath = Path.Combine(Application.dataPath, SecretsFileName);
        if (File.Exists(assetsPath))
        {
            var json = File.ReadAllText(assetsPath);
            apiKey = ExtractApiKeyFromJson(json);
        }
    }

    private string ExtractApiKeyFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return string.Empty;
        try
        {
            var kv = JsonUtility.FromJson<SimpleSecrets>(json);
            return kv != null ? kv.ApiKey : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string[] ParseStrategies(string rawStrategies)
    {
        string[] lines = rawStrategies.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        System.Collections.Generic.List<string> strategies = new System.Collections.Generic.List<string>();

        foreach (string line in lines)
        {
            string clean = line.Trim();
            if (clean.StartsWith("- "))
                clean = clean.Substring(2);
            if (!string.IsNullOrEmpty(clean))
                strategies.Add(clean);
        }

        // Return exactly 4, or pad with defaults if fewer
        while (strategies.Count < 4)
            strategies.Add("Take a deep breath and remind yourself this thought is just a thought.");

        return strategies.GetRange(0, 4).ToArray();
    }

    // ===== JSON SERIALIZATION CLASSES =====
    [System.Serializable]
    private class OpenAIRequest
    {
        public string model;
        public Message[] messages;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class OpenAIErrorWrapper
    {
        public OpenAIError error;
    }

    [System.Serializable]
    private class OpenAIError
    {
        public string message;
        public string type;
        public object param;
        public object code;
    }

    [System.Serializable]
    private class SimpleSecrets
    {
        public string ApiKey;
    }
}

/// <summary>
/// Data container for a generated anxiety thought with coping strategies
/// </summary>
[System.Serializable]
public class AnxietyThoughtData
{
    public string worriedThought;
    public string introDialogue;
    public string[] copingStrategies;
    public string[] optionExplanations; // Explains why each option is good/bad
}
