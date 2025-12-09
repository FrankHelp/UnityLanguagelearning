/// <summary>
/// Synthetisiert Text zu Sprache (Text-to-Speech).
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using JSONParser;

using System.Linq;
public class SynthesisService : MonoBehaviour
{
    [SerializeField] public BackendConfig config;
    private string _serverUrl;

    public event Action<int, AudioClip> OnClipReady;
    public event Action OnAllClipsRequested;
    public event Action<string> OnSynthesisError;

    private int _totalParts;
    private int _completedRequests;

    private void Awake()
    {
        _serverUrl = config.baseUrl + config.synthesizeEndpoint;
    }

    public void StartSynthesis(List<Part> parts)
    {
        _totalParts = parts.Count;
        _completedRequests = 0;

        for (int i = 0; i < parts.Count; i++)
        {
            StartCoroutine(RequestClip(parts[i], i));
        }
    }

    private IEnumerator RequestClip(Part part, int index)
    {
        string url = _serverUrl;
        if (part.language == "de")
            url += "Deutsch";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(part.text);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
            www.SetRequestHeader("Content-Type", "text/plain");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                OnClipReady?.Invoke(index, clip);
            }
            else
            {
                OnSynthesisError?.Invoke(www.error);
            }
        }

        _completedRequests++;

        if (_completedRequests >= _totalParts)
            OnAllClipsRequested?.Invoke();
    }
}
