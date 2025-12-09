/// <summary>
/// Nimmt Audiodaten entgegen und transkribiert sie in Text (Spracherkennung).
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
public class TranscriptionService : MonoBehaviour
{
    [SerializeField] public BackendConfig config;
    private string _serverUrl;

    public event Action<string> OnTranscriptionComplete;
    public event Action<string> OnTranscriptionError;

    private void Awake() {
        _serverUrl = config.baseUrl + config.transcribeEndpoint;
    }

    public void StartTranscription(byte[] audioData)
    {
        StartCoroutine(TranscribeCoroutine(audioData));
    }

    private IEnumerator TranscribeCoroutine(byte[] audioData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(_serverUrl, form))
        {
            yield return www.SendWebRequest();

             if (www.result == UnityWebRequest.Result.Success)
            {
                string result = www.downloadHandler.text;
                Debug.Log("Transkription erfolgreich: " + result);
                Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                result = dict["text"];
                OnTranscriptionComplete?.Invoke(result); // Event auslösen
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
