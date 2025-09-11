using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Newtonsoft.Json; // Use Newtonsoft.Json
using ChatGPTIntegration;
using UnityEngine.XR.Interaction.Toolkit;


public class DialogueHandler : MonoBehaviour
{
    private List<ChatMessage> messages;
    // [SerializeField] public Button recordingButton;
    [SerializeField]
    public string serverURL = "http://192.168.178.138";
    public InputActionReference voiceRecordButton;
    [SerializeField]
    public AudioSource audioSource;
    [SerializeField]
    public ScrollRect scrollRect;
    [SerializeField]
    public GameObject scrollText;
    [SerializeField]
    public XRBaseController rightController;

    private OpenAIChatGPT chatGPT;
    private TTSService tts;
    private WhisperTranscriber _transcriber;

    private UIHandler _UI;

    private bool isRecording;
    private AudioClip recordedClip;

    private PromptHandler prompts;
    

    void OnEnable()
    {
        if(_transcriber == null)
        {
            _transcriber = gameObject.AddComponent<WhisperTranscriber>();
        }
        _transcriber.Initialize(serverURL);
        // Events abonnieren
        _transcriber.OnTranscriptionSuccess += HandleTranscriptionSuccess;
        _transcriber.OnTranscriptionError += HandleTranscriptionError;
    }

    void OnDisable()
    {
        // Events wieder abmelden (wichtig um Memory Leaks zu vermeiden!)
        _transcriber.OnTranscriptionSuccess -= HandleTranscriptionSuccess;
        _transcriber.OnTranscriptionError -= HandleTranscriptionError;
    }

    void Start()
    {
        tts = gameObject.AddComponent<TTSService>();
        tts.Initialize(audioSource, serverURL);

        _UI = gameObject.AddComponent<UIHandler>();
        _UI.Initialize(scrollText);
        
        // tts = new TTSService(audioSource);
        chatGPT = gameObject.AddComponent<OpenAIChatGPT>();
        if(_transcriber == null)
        {
            _transcriber = gameObject.AddComponent<WhisperTranscriber>();
        }
        
        prompts = new PromptHandler();

        // Erstelle das Dictionary mit den Nachrichten
        messages = new List<ChatMessage>();

        // recordingButton.onClick.AddListener(ToggleRecording);
        voiceRecordButton.action.started += ButtonWasPressed;
        voiceRecordButton.action.canceled += ButtonWasReleased;
        sendMessage(prompts.GetCurrentUserPrompt());
    }

    void Update()
    {
    }

    IEnumerator ForceScrollDown () {
        // Wait for end of frame AND force update all canvases before setting to bottom.
        
        Canvas.ForceUpdateCanvases ();
        scrollRect.verticalNormalizedPosition = 0.1f;
        Canvas.ForceUpdateCanvases ();
        yield return new WaitForEndOfFrame ();
    }

    void ButtonWasPressed(InputAction.CallbackContext context)
    {
        // mesh.enabled = false;
        if (rightController != null)
        {
            rightController.SendHapticImpulse(0.3f, 0.1f); // Amplitude, Dauer
        }
        ToggleRecording();
    }
    void ButtonWasReleased(InputAction.CallbackContext context)
    {
        // mesh.enabled = true;
        if (rightController != null)
        {
            rightController.SendHapticImpulse(0.5f, 0.2f); // Amplitude, Dauer
        }
        // XRBaseController.SendHapticImpulse
        ToggleRecording();
    }

    public void sendMessage(string userPrompt)
    {
        if (userPrompt == null)
        {
            messages.Add(new ChatMessage("system", prompts.GetCurrentSystemPrompt()));
            messages.Add(new ChatMessage("user", prompts.GetCurrentUserPrompt()));
            Debug.Log("Promptswitch!");
        }
        else
        {
            messages.Add(new ChatMessage("system", prompts.GetCurrentSystemPrompt()));
            messages.Add(new ChatMessage("user", userPrompt));
        }
        VRDebugConsole.Log("An GPT senden...");
        StartCoroutine(chatGPT.GetChatGPTResponse(messages, OnResponseReceived));
    }

    void OnResponseReceived(string jsonResponse)
    {
         // Deserialisiere die JSON-Antwort

        // ---------------------------------------------------------------
        var parts = ResponseDeserializer.GetParts(jsonResponse);
        VRDebugConsole.Log(""+ parts);
        string full_response = "";
        // VRDebugConsole.Log("An TTS senden");
        if (parts != null)
        {
            tts.announceParts(parts.Count);
            foreach (var part in parts)
            {
                if (part.language == "de")
                {
                    tts.RequestTTSde(part.text); // Text auf Deutsch an TTS senden
                    full_response += part.text;
                }
                else if (part.language == "fr") // HIER fr
                {
                    tts.RequestTTSfr(part.text); // Text auf Französisch an TTS senden
                    full_response += part.text;
                }
                else
                {
                    Debug.LogWarning($"Unbekannte Sprache: {part.text}");
                    
                    VRDebugConsole.Log("Unbekannte Sprache...");
                }
            }
            messages.Add(new ChatMessage("assistant", full_response));
            
            // VRDebugConsole.Log("Antwort bekommen: \n"+ full_response);
            _UI.displayMessage(0, full_response);
            // scrollRect.verticalNormalizedPosition = 0;
            // StartCoroutine(ForceScrollDown());
        }

        // Get tool calls
        var toolCalls = ResponseDeserializer.GetToolCalls(jsonResponse);
        if (toolCalls != null)
        {
            foreach (var toolCall in toolCalls)
            {
                if(toolCall.function.name == "switch_prompt")
                {
                    // VRDebugConsole.Log("Switch...");
                    messages = new List<ChatMessage>(); // Nur Summary mitsenden
                    var arguments = ResponseDeserializer.GetToolCallArguments(toolCall);
                    messages.Add(new ChatMessage("assistant", "Das weiß ich über den Nutzer: " + arguments.summary + "Das ist mein geschätztes Sprachniveau des Nutzers: " + arguments.level));
                    prompts.switch_prompt();
                    if(parts == null)
                    {
                        sendMessage(null);
                    }
                }
                Debug.Log($"Tool Call: {toolCall.function.name}, Arguments: {toolCall.function.arguments}");
                VRDebugConsole.Log($"Tool Call: {toolCall.function.name}, Arguments: {toolCall.function.arguments}");
                
                
            }
        }
        // -----------------------------------
    }

    void ToggleRecording()
    {
        if (!isRecording)
        {
            // Starte Aufnahme
            recordedClip = Microphone.Start(null, false, 60, 16000); // 60 Sekunden, 16 kHz
            // recordingButton.GetComponentInChildren<Text>().text = "Stop Recording";
        }
        else
        {
            // Stoppe Aufnahme
            Microphone.End(null);

            // Konvertiere AudioClip in WAV-Daten
            byte[] wavData = ConvertAudioClipToWav(recordedClip);
            _transcriber.SendAudioRequest(wavData);

            // recordingButton.GetComponentInChildren<Text>().text = "Start Recording";
        }
        isRecording = !isRecording;
    }

    byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        return WavUtility.FromAudioClip(clip);
    }

    private void HandleTranscriptionSuccess(string result)
    {
        Debug.Log("Transkription erhalten: " + result);
        _UI.displayMessage(1, result);
        sendMessage(result);
    }

    // Event-Handler für Fehler
    private void HandleTranscriptionError(string error)
    {
        Debug.LogError("Transkription fehlgeschlagen: " + error);
        // Fehlerbehandlung (z. B. Fehlermeldung anzeigen)
    }
}