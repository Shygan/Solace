using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

/// <summary>
/// Dedicated service for chat conversation with OpenAI.
/// Simpler than AIThoughtGenerator—just returns text responses.
/// </summary>
public class AIConversationService : MonoBehaviour
{
    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private float temperature = 0.7f;
    [SerializeField] private int maxTokens = 150;
    
    private string apiKey = "";
    private const string OPENAI_URL = "https://api.openai.com/v1/chat/completions";

    void Start()
    {
        LoadApiKeyFromSecrets();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogWarning("[AIConversationService] API key not loaded from secrets.json!");
        }
    }

    private void LoadApiKeyFromSecrets()
    {
        string secretsPath = Path.Combine(Application.streamingAssetsPath, "..", "Assets", "secrets.json");
        
        // Handle both relative and absolute paths
        if (!File.Exists(secretsPath))
        {
            secretsPath = "Assets/secrets.json";
        }

        if (File.Exists(secretsPath))
        {
            try
            {
                string json = File.ReadAllText(secretsPath);
                var secrets = JsonUtility.FromJson<SecretsData>(json);
                apiKey = secrets.ApiKey;
                Debug.Log("[AIConversationService] API key loaded successfully from secrets.json");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIConversationService] Failed to load secrets.json: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[AIConversationService] secrets.json not found at {secretsPath}");
        }
    }

    /// <summary>
    /// Request a chat response from OpenAI
    /// </summary>
    public void RequestChatResponse(
        string userMessage,
        string systemPrompt,
        Action<string> onSuccess,
        Action<string> onError)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onError("API key not configured");
            return;
        }

        StartCoroutine(ChatRequestCoroutine(userMessage, systemPrompt, onSuccess, onError));
    }

    private IEnumerator ChatRequestCoroutine(
        string userMessage,
        string systemPrompt,
        Action<string> onSuccess,
        Action<string> onError)
    {
        // Build request payload
        var requestData = new ChatRequest
        {
            model = model,
            messages = new Message[]
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = userMessage }
            },
            temperature = temperature,
            max_tokens = maxTokens
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(OPENAI_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<ChatResponse>(responseText);

                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string content = response.choices[0].message.content.Trim();
                        Debug.Log($"[AIConversationService] Response: {content}");
                        onSuccess(content);
                    }
                    else
                    {
                        onError("No response choices from API");
                    }
                }
                catch (Exception e)
                {
                    onError($"Parse error: {e.Message}");
                }
            }
            else
            {
                string errorMsg = $"HTTP Error {request.responseCode}: {request.error}";
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    errorMsg += $"\n{request.downloadHandler.text}";
                
                Debug.LogError($"[AIConversationService] {errorMsg}");
                onError(errorMsg);
            }
        }
    }

    // JSON serialization classes
    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public Message[] messages;
        public float temperature;
        public int max_tokens;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;
            public string finish_reason;
        }
    }

    [System.Serializable]
    private class SecretsData
    {
        public string ApiKey;
    }
}
