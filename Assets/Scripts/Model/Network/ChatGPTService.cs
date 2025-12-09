/// <summary>
/// Sendet den transkribierten Text (und ggf. Kontext) an ChatGPT-4.1 und erhält eine Antwort.
/// </summary>

// using System.Collections;
// using UnityEngine;
// using UnityEngine.Networking;
// using Newtonsoft.Json; // Use Newtonsoft.Json
// using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using JSONParser;
using Newtonsoft.Json;

public class ChatGPTService : MonoBehaviour
{
    [SerializeField] public BackendConfig config;
    private string _apiKey;
    private string _apiUrl;

    public event Action<string, List<Part>, List<ToolCall>> OnGPTResponseReceived;
    public event Action<string> OnGPTError;

    private void Awake() {
        _apiKey = config.GPTApiKey;
        _apiUrl = config.GPTApiUrl;
    }

    // var switch_prompt = new
    // {
    //     type = "function",
    //     function = new
    //     {
    //         name = "switch_prompt",
    //         description = "Switches the System Prompt. Call this Function when important information of current System Prompt have been collected and Prompt is saturated.",
    //         parameters = new
    //         {
    //             type = "object",
    //             properties = new
    //             {
    //                 summary = new
    //                 {
    //                     type = "string",
    //                     description = "Summary of important gathered information about learner"
    //                 },
    //                 level = new
    //                 {
    //                     type = "string",
    //                     description = "Estimated CERF Level of Learner (A1 - C2)"
    //                 }
    //             },
    //             required = new[] { "summary", "level" },
    //             additionalProperties = false
    //         },
    //         strict = true
    //     }
    // };
    object materialize = new
    {
        type = "function",
        function = new
        {
            name = "materialize",
            description = "Materializes an Object into the virtual 3D World for the user to see and interact with.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    item = new
                    {
                        type = "string",
                        description = "Name of the item to be materialized in the virtual world, from list: (apple, banana, orange, strawberry, watermelon, cabbage, carrot, cucumber, pepper, tomato)"
                    }
                },
                required = new[] { "item"},
                additionalProperties = false
            },
            strict = true
        }
    };

    object responseFormat = new
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
    public void SendMessage(List<ChatMessage> messages)
    {
        StartCoroutine(GetChatGPTResponse(messages));
    }

    public IEnumerator GetChatGPTResponse(List<ChatMessage> messages)
    {
        // Convert messages to API format
        var apiMessages = new List<object>();
        
        foreach (var msg in messages)
        {
            apiMessages.Add(new { role = msg.role, content = msg.content });
        }


        // Setting OpenAI API Request Data
        var jsonData = new
        {
            model = "gpt-4.1",
            messages = apiMessages.ToArray(),
            max_tokens = 1000,
            response_format = responseFormat,
            tools = new object[] { materialize } // Hier switch_prompt einfügen falls bedarf
        };

        string jsonString = JsonConvert.SerializeObject(jsonData);

        // HTTP request settings
        UnityWebRequest request = new UnityWebRequest(_apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + _apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            OnGPTError?.Invoke(request.error); // Event auslösen
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            var jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

            // Deserialize
            List<Part> parts = ResponseDeserializer.GetParts(jsonResponse);

            string full_response = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    full_response += part.text;
                }
            }

            List<ToolCall> toolCalls = ResponseDeserializer.GetToolCalls(jsonResponse);
            OnGPTResponseReceived?.Invoke(full_response, parts, toolCalls); // Event auslösen
        }
    }
}