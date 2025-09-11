using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json; // Use Newtonsoft.Json
using System.Collections.Generic;

public class OpenAIChatGPT : MonoBehaviour
{
    private string apiKey = "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"; // Replace with an actual API key
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public IEnumerator GetChatGPTResponse(List<ChatMessage> messages, System.Action<string> callback)
    {
        // Convert messages to API format
        var apiMessages = new List<object>();
        
        foreach (var msg in messages)
        {
            apiMessages.Add(new { role = msg.role, content = msg.content });
        }
        var switch_prompt = new
        {
            type = "function",
            function = new
            {
                name = "switch_prompt",
                description = "Switches the System Prompt. Call this Function when important information of current System Prompt have been collected and Prompt is saturated.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        summary = new
                        {
                            type = "string",
                            description = "Summary of important gathered information about learner"
                        },
                        level = new
                        {
                            type = "string",
                            description = "Estimated CERF Level of Learner (A1 - C2)"
                        }
                    },
                    required = new[] { "summary", "level" },
                    additionalProperties = false
                },
                strict = true
            }
        };

        var responseFormat = new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "language_splitter",
                schema = new
                {
                    type = "object",
                    properties = new
                    {
                        parts = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    language = new 
                                    { 
                                        type = "string",
                                        @enum = new[] { "de", "fr" } // hier fr
                                    },
                                    text = new { type = "string" }
                                },
                                required = new[] { "language", "text" },
                                additionalProperties = false
                            }
                        }
                    },
                    required = new[] { "parts" },
                    additionalProperties = false
                },
                strict = true
            }
        };


        // Setting OpenAI API Request Data
        var jsonData = new
        {
            model = "gpt-4.1",
            messages = apiMessages.ToArray(),
            max_tokens = 1000,
            response_format = responseFormat,
            tools = new[] { switch_prompt }
        };

        string jsonString = JsonConvert.SerializeObject(jsonData);

        // HTTP request settings
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            VRDebugConsole.Log("Error: " + request.error);
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            var responseText = request.downloadHandler.text;
            Debug.Log("Response: " + responseText);
            // VRDebugConsole.Log("GPT Antwort erhalten");
            callback(responseText);
            // Parse the JSON response to extract the required parts
            // var response = JsonConvert.DeserializeObject<OpenAIResponse>(responseText);
            // callback(response.choices[0].message.content.Trim());
        }
    }
}