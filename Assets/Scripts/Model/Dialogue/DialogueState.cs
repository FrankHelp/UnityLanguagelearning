/// <summary>
/// Verwaltet den Gesprächsverlauf.
/// </summary>
using System.Collections.Generic;

[System.Serializable]
public class DialogueState
{
    public List<ChatMessage> Messages { get; private set; }
    public string SystemPrompt { get; private set; }

    public DialogueState(string systemPrompt = "")
    {
        Messages = new List<ChatMessage>();
        SystemPrompt = systemPrompt;
        
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            Messages.Add(new ChatMessage("system", systemPrompt));
        }
    }

    public void AddUserMessage(string content)
    {
        Messages.Add(new ChatMessage("user", content));
    }

    public void AddAssistantMessage(string content)
    {
        Messages.Add(new ChatMessage("assistant", content));
    }

    public void AddSystemMessage(string content)
    {
        Messages.Add(new ChatMessage("system", content));
    }

    public List<ChatMessage> GetMessages()
    {
        return new List<ChatMessage>(Messages);
    }
}