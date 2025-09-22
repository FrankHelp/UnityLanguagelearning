using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class WhisperTranscriber : MonoBehaviour
{
    public string serverUrl;

    // Event-Typen für Erfolg/Fehler
    public delegate void TranscriptionEventHandler(string result);
    public delegate void TranscriptionErrorHandler(string error);

    // Öffentliche Events, die andere Klassen abonnieren können
    public event TranscriptionEventHandler OnTranscriptionSuccess;
    public event TranscriptionErrorHandler OnTranscriptionError;

    public void Initialize(string serverURL)
    {
        this.serverUrl = serverURL+":8150/frank/transcribe";
    }

    public void SendAudioRequest(byte[] audioData)
    {
        StartCoroutine(UploadAudio(audioData));
    }

    IEnumerator UploadAudio(byte[] audioData)
    {
        // HTTP-Request vorbereiten
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

             if (www.result == UnityWebRequest.Result.Success)
            {
                string result = www.downloadHandler.text;
                Debug.Log("Transkription erfolgreich: " + result);
                OnTranscriptionSuccess?.Invoke(result); // Event auslösen
            }
            else
            {
                string error = "Fehler: " + www.error;
                Debug.LogError(error);
                OnTranscriptionError?.Invoke(error); // Event auslösen
            }
        }
    }
}