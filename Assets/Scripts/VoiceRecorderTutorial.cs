using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;


public class VoiceRecorder : MonoBehaviour
{
    [Header("Audio Settings")]
    private AudioClip recordedClip;
    private bool isRecording = false;
    
    [Header("References")]
    [SerializeField] private InputActionReference voiceRecordButton;
    [SerializeField] private WhisperTranscriber transcriber;
    [SerializeField] private XRBaseController rightController;
    [SerializeField] private GameObject transcription_panel;
    
    [SerializeField] private TMP_Text messageText;
    [SerializeField] public VRStudyTutorial tutorial;
    
    private const int RECORDING_LENGTH = 30;
    private const int SAMPLE_RATE = 16000;

    #region Unity Lifecycle
    
    private void OnEnable()
    {
        SubscribeToEvents();
        transcriber.OnTranscriptionSuccess += HandleTranscriptionSuccess;
        transcriber.OnTranscriptionError += HandleTranscriptionError;
        if (transcription_panel != null)
            transcription_panel.SetActive(false);
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        if(transcriber != null)
        {
            transcriber.OnTranscriptionSuccess -= HandleTranscriptionSuccess;
            transcriber.OnTranscriptionError -= HandleTranscriptionError;
        }
        
        
        // Stoppe Aufnahme falls aktiv
        if (isRecording)
        {
            StopRecording();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    #endregion

    #region Event Handling
    
    private void SubscribeToEvents()
    {
        if (voiceRecordButton?.action != null)
        {
            voiceRecordButton.action.started += HandleButtonPressed;
            voiceRecordButton.action.canceled += HandleButtonReleased;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (voiceRecordButton?.action != null)
        {
            voiceRecordButton.action.started -= HandleButtonPressed;
            voiceRecordButton.action.canceled -= HandleButtonReleased;
        }
    }

    private void HandleButtonPressed(InputAction.CallbackContext context)
    {
        SendHapticFeedback(0.3f, 0.1f);
        StartRecording();
    }

    private void HandleButtonReleased(InputAction.CallbackContext context)
    {
        SendHapticFeedback(0.5f, 0.2f);
        StopRecording();
    }
    
    #endregion

    #region Recording Logic
    
    private void StartRecording()
    {
        if (isRecording) return;
        transcription_panel.SetActive(true);
        
        try
        {
            recordedClip = Microphone.Start(null, false, RECORDING_LENGTH, SAMPLE_RATE);
            isRecording = true;
            Debug.Log("Recording started");
            messageText.text = "Aufnahme läuft...";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start recording: {e.Message}");
            messageText.text = "Fehler bei der Aufnahme";
        }
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        
        Microphone.End(null);
        isRecording = false;
        Debug.Log("Recording stopped");
        
        ProcessRecording();
    }

    private void ProcessRecording()
    {
        if (recordedClip == null)
        {
            Debug.LogWarning("No recorded clip to process");
            messageText.text = "Keine Aufnahme gefunden";
            return;
        }

        try
        {
            byte[] wavData = ConvertAudioClipToWav(recordedClip);
            messageText.text = "Konvertiert...";
            transcriber.SendAudioRequest(wavData);
            messageText.text = "Sende zur Transkription...";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to process recording: {e.Message}");
        }
    }
    
    #endregion

    #region Transcription Handlers
    
    private void HandleTranscriptionSuccess(string result)
    {
        Debug.Log($"Transcription received: {result}");
        
        if (messageText != null)
        {
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            string displayText = $"Erkannt: {dict["text"]}";
            messageText.text = displayText;
        }
        tutorial.OnPushToTalkTriggered();
    }

    private void HandleTranscriptionError(string error)
    {
        Debug.LogError($"Transcription failed: {error}");
        messageText.text = $"Whisper Fehler: {error}";
        
        if (messageText != null)
        {
            messageText.text = "Fehler bei der Spracherkennung";
        }
    }
    
    #endregion

    #region Utility Methods
    
    private void SendHapticFeedback(float amplitude, float duration)
    {
        if (rightController != null)
        {
            rightController.SendHapticImpulse(amplitude, duration);
        }
    }

    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        // Implementierung der Audio-Konvertierung
        // Diese Methode muss je nach Anforderungen implementiert werden
        return WavUtility.FromAudioClip(clip);
    }
    
    #endregion
}