using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

/// <summary>
/// Fetches and displays a Quote of the Day from OpenAI API.
/// Loads the API key from secrets.json and displays a new quote each time the lobby loads.
/// Works with 3D TextMeshPro objects.
/// </summary>
public class QuoteOfTheDayManager : MonoBehaviour
{
    [SerializeField] private TextMeshPro quoteText;
    
    private const string quotePrompt = "Generate one short inspiring quote about mental health, anxiety, self-care, or wellness. Keep it uplifting and concise. Try to change the quotes up.";
    private string apiKey;

    void Start()
    {
        if (quoteText == null)
        {
            Debug.LogError("[QuoteOfTheDayManager] Quote text UI not assigned!");
            return;
        }

        LoadApiKey();
        if (!string.IsNullOrEmpty(apiKey))
        {
            StartCoroutine(FetchQuoteOfTheDay());
        }
        else
        {
            quoteText.text = "Quote of the Day: Unable to load quote.";
            Debug.LogError("[QuoteOfTheDayManager] API key not found!");
        }
    }

    void LoadApiKey()
    {
        string secretsPath = Application.persistentDataPath + "/secrets.json";
        
        // Try persistent data path first
        if (System.IO.File.Exists(secretsPath))
        {
            LoadKeyFromFile(secretsPath);
            return;
        }

        // Try Assets folder
        TextAsset secretsAsset = Resources.Load<TextAsset>("secrets");
        if (secretsAsset != null)
        {
            ParseApiKey(secretsAsset.text);
            return;
        }

        // Try direct path in Assets
        string assetPath = Application.dataPath + "/secrets.json";
        if (System.IO.File.Exists(assetPath))
        {
            LoadKeyFromFile(assetPath);
        }
    }

    void LoadKeyFromFile(string filePath)
    {
        try
        {
            string json = System.IO.File.ReadAllText(filePath);
            ParseApiKey(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuoteOfTheDayManager] Error reading secrets file: {e.Message}");
        }
    }

    void ParseApiKey(string json)
    {
        try
        {
            // Simple JSON parsing for ApiKey
            int startIndex = json.IndexOf("\"ApiKey\"");
            if (startIndex != -1)
            {
                startIndex = json.IndexOf("\"", startIndex + 8) + 1;
                int endIndex = json.IndexOf("\"", startIndex);
                apiKey = json.Substring(startIndex, endIndex - startIndex);
                Debug.Log("[QuoteOfTheDayManager] API key loaded successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuoteOfTheDayManager] Error parsing API key: {e.Message}");
        }
    }

    IEnumerator FetchQuoteOfTheDay()
    {
        // Prepare the request
        string url = "https://api.openai.com/v1/chat/completions";
        
        // Properly escape the prompt for JSON
        string escapedPrompt = quotePrompt.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        
        string requestBody = "{" +
            "\"model\":\"gpt-3.5-turbo\"," +
            "\"messages\":[" +
                "{\"role\":\"user\",\"content\":\"" + escapedPrompt + "\"}" +
            "]," +
            "\"temperature\":0.7," +
            "\"max_tokens\":150" +
        "}";

        Debug.Log($"[QuoteOfTheDayManager] Sending request to OpenAI API...");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[QuoteOfTheDayManager] API Response received: {request.downloadHandler.text}");
                ParseAndDisplayQuote(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[QuoteOfTheDayManager] API Error: {request.error}");
                Debug.LogError($"[QuoteOfTheDayManager] Response Code: {request.responseCode}");
                Debug.LogError($"[QuoteOfTheDayManager] Response: {request.downloadHandler.text}");
                quoteText.text = "Quote of the Day: Unable to fetch quote at this time.";
            }
        }
    }

    void ParseAndDisplayQuote(string response)
    {
        try
        {
            Debug.Log($"[QuoteOfTheDayManager] Full API Response: {response}");
            
            // Parse JSON response to extract the quote
            // Look for "content" field in the response
            int contentStart = response.IndexOf("\"content\":");
            if (contentStart != -1)
            {
                // Move to the opening quote of the content value
                contentStart = response.IndexOf("\"", contentStart + 10) + 1;
                
                // Find the closing quote, accounting for escaped quotes
                int contentEnd = contentStart;
                bool foundEnd = false;
                while (contentEnd < response.Length && !foundEnd)
                {
                    contentEnd = response.IndexOf("\"", contentEnd);
                    if (contentEnd == -1) break;
                    
                    // Check if this quote is escaped
                    int backslashCount = 0;
                    int checkPos = contentEnd - 1;
                    while (checkPos >= contentStart && response[checkPos] == '\\')
                    {
                        backslashCount++;
                        checkPos--;
                    }
                    
                    // If even number of backslashes (or zero), this quote is not escaped
                    if (backslashCount % 2 == 0)
                    {
                        foundEnd = true;
                    }
                    else
                    {
                        contentEnd++; // Move past this escaped quote
                    }
                }
                
                if (foundEnd && contentEnd > contentStart)
                {
                    string quote = response.Substring(contentStart, contentEnd - contentStart);

                    // Unescape JSON characters
                    quote = quote.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                    
                    // Remove leading and trailing quotation marks if present
                    quote = quote.Trim();
                    if (quote.StartsWith("\"") && quote.EndsWith("\""))
                    {
                        quote = quote.Substring(1, quote.Length - 2);
                    }
                    
                    quoteText.text = quote;
                    Debug.Log($"[QuoteOfTheDayManager] Quote displayed: {quote}");
                }
                else
                {
                    Debug.LogWarning("[QuoteOfTheDayManager] Could not find content end");
                    quoteText.text = "Quote of the Day: Unable to parse quote.";
                }
            }
            else
            {
                Debug.LogWarning("[QuoteOfTheDayManager] No content field found in response");
                quoteText.text = "Quote of the Day: No content in response.";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuoteOfTheDayManager] Error parsing response: {e.Message}");
            quoteText.text = "Quote of the Day: Error parsing response.";
        }
    }
}
