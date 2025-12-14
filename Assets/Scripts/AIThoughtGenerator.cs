using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Calls OpenAI API to generate worrying thoughts and coping strategies
/// Handles JSON serialization for API requests/responses
/// </summary>
public class AIThoughtGenerator : MonoBehaviour
{
    [SerializeField] private string apiKey = "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"; // Set in Inspector or use PlayerPrefs
    [SerializeField] private string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private float temperature = 0.7f;

        // Single-call prompt returning compact JSON with 3 non-optimal and 1 optimal option
        private const string COMBINED_PROMPT = "You are helping in an anxiety-awareness game.\n" +
            "Task: Given no input, generate a concise worrying thought and four coping options:\n" +
            "- EXACTLY 3 non-optimal themes: Avoidance, Self-criticism, Over-control (brief 6-12 words each)\n" +
            "- EXACTLY 1 optimal theme: Cognitive Reframe (brief 6-12 words)\n" +
            "- Provide a short introDialogue (<= 40 words) empathetic to the worrying thought\n" +
            "- For EACH option, include a 15-25 word explanation dialogue tailored to the worrying thought.\n\n" +
            "Output STRICT JSON ONLY in this schema (no extra text):\n" +
            "{\n" +
            "  \"thought\": \"under 14 words\",\n" +
            "  \"introDialogue\": \"under 40 words\",\n" +
            "  \"options\": [\n" +
            "    { \"title\": \"short option text\", \"theme\": \"Avoidance|Self-criticism|Over-control|Cognitive Reframe\", \"dialogue\": \"15-25 words explanation\", \"optimal\": false },\n" +
            "    { \"title\": \"short option text\", \"theme\": \"Avoidance|Self-criticism|Over-control|Cognitive Reframe\", \"dialogue\": \"15-25 words explanation\", \"optimal\": false },\n" +
            "    { \"title\": \"short option text\", \"theme\": \"Avoidance|Self-criticism|Over-control|Cognitive Reframe\", \"dialogue\": \"15-25 words explanation\", \"optimal\": false },\n" +
            "    { \"title\": \"short option text\", \"theme\": \"Cognitive Reframe\", \"dialogue\": \"15-25 words explanation\", \"optimal\": true }\n" +
            "  ]\n" +
            "}\n" +
            "Ensure the last option is the Cognitive Reframe with \"optimal\": true. Keep all texts concise to fit UI.";

    /// <summary>
    /// Generates a worrying thought, concise dialogue, and 4 options (3 non-optimal, 1 optimal)
    /// </summary>
    public void GenerateThought(Action<AnxietyThoughtData> onComplete, Action<string> onError)
    {
        StartCoroutine(GenerateThoughtCoroutine(onComplete, onError));
    }

    private IEnumerator GenerateThoughtCoroutine(Action<AnxietyThoughtData> onComplete, Action<string> onError)
    {
        string json = null;
        yield return StartCoroutine(CallOpenAI(COMBINED_PROMPT, (result) => json = result, onError));

        if (string.IsNullOrEmpty(json))
        {
            onError?.Invoke("Failed to generate AI JSON response");
            yield break;
        }

        var parsed = JsonUtility.FromJson<AIResponseJson>(json);
        if (parsed == null || parsed.options == null || parsed.options.Length != 4)
        {
            Debug.LogError("[AI] JSON parse failed or wrong schema: " + json);
            onError?.Invoke("AI response invalid. Try again.");
            yield break;
        }

        var data = new AnxietyThoughtData
        {
            worriedThought = parsed.thought,
            introDialogue = parsed.introDialogue,
            options = parsed.options
        };

        Debug.Log("[AI] Generated thought + 4 options (3 non-optimal, 1 optimal)");
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

    // (No longer used) strategy parser removed in favor of strict JSON

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
}

/// <summary>
/// Data container for a generated anxiety thought with coping strategies
/// </summary>
[System.Serializable]
public class AnxietyThoughtData
{
    public string worriedThought;
    public string introDialogue;
    public OptionData[] options; // 4 items: 3 non-optimal, 1 optimal
}

[System.Serializable]
public class OptionData
{
    public string title;     // short label for button
    public string theme;     // Avoidance | Self-criticism | Over-control | Cognitive Reframe
    public string dialogue;  // 15-25 word explanation
    public bool optimal;     // true for the Cognitive Reframe
}

[System.Serializable]
public class AIResponseJson
{
    public string thought;
    public string introDialogue;
    public OptionData[] options;
}
