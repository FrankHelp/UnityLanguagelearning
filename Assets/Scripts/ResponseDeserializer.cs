using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ChatGPTIntegration
{
    // Classes to represent the response structure
    [Serializable]
    public class ChatResponse
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public List<Choice> choices;
        public Usage usage;
        public string service_tier;
        public string system_fingerprint;
    }

    [Serializable]
    public class Choice
    {
        public int index;
        public Message message;
        public object logprobs; // Can be null
        public string finish_reason;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content; // Can be null or JSON string with parts
        public List<ToolCall> tool_calls; // Can be null
        public object refusal; // Can be null
        public List<object> annotations; // Can be empty
    }

    [Serializable]
    public class ToolCall
    {
        public string id;
        public string type;
        public Function function;
    }

    [Serializable]
    public class Function
    {
        public string name;
        public string arguments; // JSON string, can be deserialized further if needed
    }
    [Serializable]
    public class Arguments
    {
        public string summary;
        public string level;
    }

    [Serializable]
    public class Part
    {
        public string language;
        public string text;
    }

    [Serializable]
    public class ContentParts
    {
        public List<Part> parts;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
        public TokenDetails prompt_tokens_details;
        public TokenDetails completion_tokens_details;
    }

    [Serializable]
    public class TokenDetails
    {
        public int cached_tokens;
        public int audio_tokens;
        public int reasoning_tokens; // Only in completion_tokens_details
        public int accepted_prediction_tokens; // Only in completion_tokens_details
        public int rejected_prediction_tokens; // Only in completion_tokens_details
    }

    public static class ResponseDeserializer
    {
        /// <summary>
        /// Deserializes a ChatGPT response JSON string and extracts the list of parts if present.
        /// </summary>
        /// <param name="json">The JSON response string from ChatGPT API</param>
        /// <returns>List of Part objects or null if no parts are present</returns>
        public static List<Part> GetParts(string json)
        {
            try
            {
                // VRDebugConsole.Log("Versuche zu deserialisieren: ");
                var response = JsonConvert.DeserializeObject<ChatResponse>(json);
                if (response?.choices == null || response.choices.Count == 0)
                {
                    VRDebugConsole.Log("respone?choices == null: ");
                    return null;
                }
                    

                var message = response.choices[0].message;
                if (string.IsNullOrEmpty(message?.content))
                {
                    VRDebugConsole.Log("isNullOrEmpty");
                    return null;
                }
                    

                // Content is a JSON string containing parts
                var contentParts = JsonConvert.DeserializeObject<ContentParts>(message.content);
                return contentParts?.parts;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to deserialize parts: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes a ChatGPT response JSON string and extracts the list of tool calls if present.
        /// </summary>
        /// <param name="json">The JSON response string from ChatGPT API</param>
        /// <returns>List of ToolCall objects or null if no tool calls are present</returns>
        public static List<ToolCall> GetToolCalls(string json)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<ChatResponse>(json);
                if (response?.choices == null || response.choices.Count == 0)
                    return null;

                var message = response.choices[0].message;
                return message?.tool_calls;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to deserialize tool calls: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializes the arguments of a specific tool call function into a specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the arguments into</typeparam>
        /// <param name="toolCall">The ToolCall object containing the function arguments</param>
        /// <returns>The deserialized arguments as type T, or default(T) if deserialization fails</returns>
        public static Arguments GetToolCallArguments(ToolCall toolCall)
        {
            try
            {
                if (toolCall?.function?.arguments == null)
                    return default;

                return JsonConvert.DeserializeObject<Arguments>(toolCall.function.arguments);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to deserialize tool call arguments: {ex.Message}");
                return default;
            }
        }
    }
}