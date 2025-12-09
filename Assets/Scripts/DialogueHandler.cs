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
    private string serverURL = "http://192.100.14.194:65432";
    public InputActionReference voiceRecordButton;
    [SerializeField]
    public AudioSource audioSource;
    [SerializeField]
    public ScrollRect scrollRect;
    [SerializeField]
    public GameObject scrollText;
    [SerializeField]
    public XRBaseController rightController;
    [SerializeField]
    public ObjectSpawner objectSpawner;

    private OpenAIChatGPT chatGPT;
    private TTSService tts;
    private WhisperTranscriber _transcriber;
    

    private UIHandler _UI;

    private bool isRecording;
    private AudioClip recordedClip;

    private PromptHandler prompts;
    private int currentStatus;
    

    void OnEnable()
    {
        if(_transcriber == null)
        {
            _transcriber = gameObject.AddComponent<WhisperTranscriber>();
        }
        currentStatus = SessionManager.GetInt("GameStatus", 0);
        _transcriber.Initialize(serverURL, currentStatus);
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

    void OnDestroy()
    {
        StartCoroutine(UploadMessages());
    }
    IEnumerator UploadMessages()
    {
        string jsonMessages = JsonConvert.SerializeObject(messages);
        
        // Synchron warten bis Upload fertig ist
        yield return StartCoroutine(UploadToFirebase(jsonMessages));
        
        // Erst danach wird das GameObject zerstört
    }

    IEnumerator UploadToFirebase(string jsonData)
    {
        // string url = "https://batranscripts.firebaseio.com/chatlogs.json";
        int id = SessionManager.GetInt("UserID");
        currentStatus = SessionManager.GetInt("GameStatus", 0);
        // int id = 42;
        string url = $"https://batranscripts-default-rtdb.europe-west1.firebasedatabase.app/{id}/{currentStatus}/chats.json";
        // EINFACHE VARIANTE ohne CreateRequest:
        UnityWebRequest request = new UnityWebRequest(url, "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error: {request.error}");
        }
        else
        {
            Debug.Log("Upload successful!");
        }
    }

    void Awake()
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
        messages.Add(new ChatMessage("system", prompts.GetCurrentSystemPrompt()));

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
            // messages.Add(new ChatMessage("system", prompts.GetCurrentSystemPrompt()));
            // messages.Add(new ChatMessage("user", prompts.GetCurrentUserPrompt()));
        }
        else
        {
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
            StartCoroutine(UploadMessages());
            
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
                if(toolCall.function.name == "materialize")
                {
                    var arguments = ResponseDeserializer.GetToolCallArguments(toolCall);
                    messages.Add(new ChatMessage("system", arguments.item + " was spawned in."));
                    if(parts == null)
                    {
                        sendMessage(null);
                    }
                    objectSpawner.SpawnObject(arguments.item);
                }
                Debug.Log($"Tool Call: {toolCall.function.name}, Arguments: {toolCall.function.arguments}");
                // VRDebugConsole.Log($"Tool Call: {toolCall.function.name}, Arguments: {toolCall.function.arguments}");
                
                
            }
        }

        // Falls ChatGPT leer Antwortet (Bug)
        if (toolCalls == null && parts == null)
        {
            // VRDebugConsole.Log("Leere Antwort von GPT, sende nochmal...");
            sendMessage(null);
        }
        // -----------------------------------
    }

    void ToggleRecording()
    {
        if (!isRecording)
        {
            // Starte Aufnahme
            recordedClip = Microphone.Start(null, false, 30, 16000); // 30 Sekunden, 16 kHz
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
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
        result = dict["text"];
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