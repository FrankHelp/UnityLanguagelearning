/// <summary>
/// Koordiniert den Dialogfluss. Er verwendet die Network-Services, um Spracherkennung, KI-Antwort und Sprachsynthese zu steuern.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
using JSONParser;
using UnityEngine.InputSystem;

public class DialogueController : MonoBehaviour
{
    // kennt die Network-Services (TranscriptionService, ChatGPTService, SynthesisService) und den DialogueState.

    [SerializeField] private TranscriptionService transcriptionService;
    [SerializeField] private ChatGPTService chatGPTService;
    [SerializeField] private SynthesisService synthesisService;
    [SerializeField] private IVAController ivaController;
    [SerializeField] private ChatScrollView chatScrollView;
    [SerializeField] private PrefabSpawner prefabSpawner;
    private DialogueState dialogueState;
    private AudioClip recordedClip;

    private static string responseFormatPrompt = "" +
    "SPRACH-AUFTEILUNGSREGELN: \n" +
    "1. JEDER Eintrag im \"parts\"-Array MUSS rein monolingual sein (100% deutsch ODER 100% französisch) \n" +
    "2. Wenn ein Wort in einem Satz die Sprache wechselt, muss das Wort bzw der Teilsatz SOFORT in einen neuen language part getrennt werden \n" +
    "\n" +
    "BEISPIEL Falsch: \n" +
    "{\n" +
    "  \"parts\": [\n" +
    "    { \"language\": \"de\", \"text\": \"Das Wort Lenkrad nennt man auf französisch volant, man sagt zum Beispiel Tournez le volant.\" },\n" +
    "    { \"language\": \"fr\", \"text\": \"Le mot Lenkrad s'appelle volant en français. On dit par exemple Tournez le volant\" }\n" +
    "  ]\n" +
    "}\n" +
    "\n" +
    "BEISPIEL Richtig: \n" +
    "{\n" +
    "  \"parts\": [\n" +
    "    { \"language\": \"de\", \"text\": \"Das Wort Lenkrad nennt man auf französisch \" },\n" +
    "    { \"language\": \"fr\", \"text\": \"volant\" },\n" +
    "    { \"language\": \"de\", \"text\": \", man sagt zum Beispiel \" },\n" +
    "    { \"language\": \"fr\", \"text\": \"Tournez le volant\" }\n" +
    "  ]\n" +
    "}\n" +
    "\n" +
    "Überprüfe nach der Aufteilung, dass KEIN part Wörter beide Sprachen enthält.";
    private static string _prompt1 = "Tu es Chris, un assistant conversationnel de 25 ans vivant à Paris. " +
    "Ton rôle est d'aider les utilisateurs à pratiquer le français par des conversations naturelles.\n\n" +
    "RÈGLES:\n" +
    "- Tu as apporté des objets, appelle la fonction materialize avec un function call pour les faire apparaître dans le monde virtuel. You can spawn objects from this list (apple, banana, orange, strawberry, watermelon, cabbage, carrot, cucumber, pepper, tomato), but only if the user asks about your objects. The user can't really interact with these objects, except for grabbing and moving them.\n" +
    "- Réponses courtes (1-2 phrases maximum)\n" + 
    "- Essayez de répondre uniquement en français, sauf si l'utilisateur veut que vous répondiez en allemand. \n" +
    "- Si une entrée est incompréhensible, demande poliment de répéter\n" + responseFormatPrompt;

    private void Start()
    {
        ivaController.RegisterServices(synthesisService);
        SubscribeToEvents();
        dialogueState = new DialogueState(
            systemPrompt: _prompt1
        );
        chatGPTService.SendMessage(dialogueState.GetMessages());
    }

    private void SubscribeToEvents()
    {
        transcriptionService.OnTranscriptionComplete += OnTranscriptionComplete;
        transcriptionService.OnTranscriptionError += OnTranscriptionError;
        
        chatGPTService.OnGPTResponseReceived += OnGPTResponseReceived;
        chatGPTService.OnGPTError += OnGPTError;
        
        // synthesisService.OnSynthesisComplete += OnSynthesisComplete;
        // synthesisService.OnSynthesisError += OnSynthesisError;
    }

    private void OnTranscriptionComplete(string transcribedText)
    {
        dialogueState.AddUserMessage(transcribedText);
        chatScrollView.AddUserMessage(transcribedText);
        chatGPTService.SendMessage(dialogueState.GetMessages());
    }
    // List<Part>, List<ToolCall>
    private void OnGPTResponseReceived(string full_response, List<Part> parts, List<ToolCall> toolCalls)
    {
        dialogueState.AddAssistantMessage(full_response);
        chatScrollView.AddAssistantMessage(full_response);
        if (toolCalls != null)
        {
            foreach (var toolCall in toolCalls)
            {
                if(toolCall.function.name == "materialize")
                {
                    var arguments = ResponseDeserializer.GetToolCallArguments(toolCall);
                    prefabSpawner.SpawnObject(arguments.item);
                    dialogueState.AddSystemMessage($"Spawned object: {arguments.item}");
                }
                // Weitere ToolCalls hier
            }
        }
        if (parts != null)
        {
            synthesisService.StartSynthesis(parts);
        } else
        {
            Debug.LogWarning("No parts received for synthesis. Likely due to toolCalls being used.");
            chatGPTService.SendMessage(dialogueState.GetMessages());
        }
    }

    // private void OnSynthesisComplete(AudioClip[] audioClips)
    // {
    //     ivaController.PlayResponse(audioClips);
    // }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        transcriptionService.OnTranscriptionComplete -= OnTranscriptionComplete;
        transcriptionService.OnTranscriptionError -= OnTranscriptionError;
        
        chatGPTService.OnGPTResponseReceived -= OnGPTResponseReceived;
        chatGPTService.OnGPTError -= OnGPTError;
        
        // synthesisService.OnSynthesisComplete -= OnSynthesisComplete;
        // synthesisService.OnSynthesisError -= OnSynthesisError;
    }

    public void startRecording()
    {
        
        recordedClip = Microphone.Start(null, false, 30, 16000); // 30 Sekunden, 16 kHz
    }

    public void stopRecording()
    {
        Microphone.End(null);

            // Konvertiere AudioClip in WAV-Daten
        byte[] wavData = WavUtility.FromAudioClip(recordedClip);
        transcriptionService.StartTranscription(wavData);
    }

    private void OnTranscriptionError(string error)
    {
        Debug.LogError("Transcription Error: " + error);
        chatScrollView.AddAssistantMessage("Entschuldiung, ein Fehler ist aufgetreten: " + error);
    }
    private void OnGPTError(string error)
    {
        Debug.LogError("GPT Error: " + error);
        chatScrollView.AddAssistantMessage("Entschuldiung, ein Fehler ist aufgetreten: " + error);
    }
    // private void OnSynthesisError(string error)
    // {
    //     Debug.LogError("Synthesis Error: " + error);
    //     chatScrollView.AddAssistantMessage("Entschuldiung, ein Fehler ist aufgetreten: " + error);
    // }
}